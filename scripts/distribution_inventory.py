#!/usr/bin/env python3
"""Validate unpublished CAP-008 artifacts and describe their exact SHA-256 bytes.

This is an offline, read-only release-candidate gate. It neither extracts nor
executes package members and deliberately has no publishing capability.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import stat
import sys
import tarfile
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path, PurePosixPath

PACKAGE_ID = "Infoconex.DotNetBinary.Tools"
SUPPORTED_RIDS = ("win-x64", "linux-x64", "osx-x64")
VERSION_PATTERN = re.compile(r"^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$")
SHA_PATTERN = re.compile(r"^[0-9a-fA-F]{40}$")


def _local_name(element: ET.Element) -> str:
    return element.tag.rsplit("}", 1)[-1]


def _child(element: ET.Element, name: str) -> ET.Element:
    for item in element:
        if _local_name(item) == name:
            return item
    raise ValueError(f"NuGet nuspec missing {name}")


def _text(element: ET.Element, name: str) -> str:
    return (_child(element, name).text or "").strip()


def _safe_names(names: list[str], origin: str) -> None:
    normalized: set[str] = set()
    for name in names:
        path = PurePosixPath(name)
        if (
            not name
            or name.startswith(("/", "\\"))
            or "\\" in name
            or ":" in name
            or "\x00" in name
            or any(part in ("", ".", "..") for part in name.rstrip("/").split("/"))
            or path.is_absolute()
        ):
            raise ValueError(f"unsafe archive member in {origin}: {name!r}")
        folded = name.rstrip("/").casefold()
        if folded in normalized:
            raise ValueError(f"duplicate archive member in {origin}: {name!r}")
        normalized.add(folded)


def _read_nuspec(package_path: Path, expected_id: str, version: str) -> ET.Element:
    with zipfile.ZipFile(package_path) as package:
        members = package.infolist()
        if not members or len(members) > 10000:
            raise ValueError(f"invalid NuGet package member count: {package_path.name}")
        _safe_names([item.filename for item in members], package_path.name)
        if any(stat.S_IFMT(item.external_attr >> 16) == stat.S_IFLNK for item in members):
            raise ValueError(f"unsafe NuGet symlink in {package_path.name}")
        nuspecs = [item for item in members if item.filename.endswith(".nuspec")]
        if len(nuspecs) != 1 or nuspecs[0].file_size > 1024 * 1024:
            raise ValueError(f"expected one bounded nuspec in {package_path.name}")
        try:
            metadata = _child(ET.fromstring(package.read(nuspecs[0])), "metadata")
        except ET.ParseError as exc:
            raise ValueError(f"invalid nuspec XML in {package_path.name}") from exc
        if _text(metadata, "id") != expected_id or _text(metadata, "version") != version:
            raise ValueError(f"NuGet identity/version mismatch in {package_path.name}")
        return metadata


def _validate_pointer(metadata: ET.Element, version: str) -> None:
    dependencies = _child(metadata, "dependencies")
    actual = []
    for dependency in dependencies.iter():
        if _local_name(dependency) == "dependency":
            actual.append((dependency.attrib.get("id"), dependency.attrib.get("version")))
    expected_ids = {f"{PACKAGE_ID}.{rid}" for rid in SUPPORTED_RIDS}
    actual_ids = [name for name, _ in actual]
    if set(actual_ids) != expected_ids or len(actual_ids) != len(expected_ids):
        raise ValueError("pointer dependencies must contain exactly the approved RID packages")
    if any(dependency_version not in (version, f"[{version}]") for _, dependency_version in actual):
        raise ValueError("pointer dependencies require the exact matching version")


def _validate_native_archive(archive_path: Path, rid: str) -> None:
    executable = "dotnet-binary.exe" if rid == "win-x64" else "dotnet-binary"
    if rid == "win-x64":
        with zipfile.ZipFile(archive_path) as archive:
            members = archive.infolist()
            _safe_names([item.filename for item in members], archive_path.name)
            if executable not in [item.filename for item in members if not item.is_dir()]:
                raise ValueError(f"standalone archive missing {executable}")
            if any(stat.S_IFMT(item.external_attr >> 16) == stat.S_IFLNK for item in members):
                raise ValueError("unsafe standalone archive symlink")
    else:
        with tarfile.open(archive_path, "r:gz") as archive:
            members = archive.getmembers()
            _safe_names([item.name for item in members], archive_path.name)
            if any(not (item.isfile() or item.isdir()) for item in members):
                raise ValueError("unsafe standalone archive link or special file")
            executables = [item for item in members if item.name == executable and item.isfile()]
            if len(executables) != 1:
                raise ValueError(f"standalone archive missing {executable}")
            if executables[0].mode & 0o111 == 0:
                raise ValueError(f"standalone archive lost executable mode for {executable}")


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def validate_distribution(packages: Path, standalone: Path, version: str, source_sha: str) -> dict:
    """Check all exact package/archive identities before reporting immutable hashes."""
    if not VERSION_PATTERN.fullmatch(version) or not SHA_PATTERN.fullmatch(source_sha):
        raise ValueError("version or 40-character source SHA is invalid")
    packages = Path(packages)
    standalone = Path(standalone)
    if not packages.is_dir() or not standalone.is_dir():
        raise ValueError("package or standalone directory is missing")

    expected_packages = {f"{PACKAGE_ID}.{version}.nupkg"}
    expected_packages.update(f"{PACKAGE_ID}.{rid}.{version}.nupkg" for rid in SUPPORTED_RIDS)
    actual_packages = {p.name for p in packages.glob("*.nupkg")}
    missing_packages = expected_packages - actual_packages
    unexpected_packages = actual_packages - expected_packages
    if missing_packages or unexpected_packages:
        raise ValueError(f"NuGet artifacts missing {sorted(missing_packages)}, unexpected {sorted(unexpected_packages)}")

    expected_archives = {
        f"dotnet-binary-{version}-{rid}.{'zip' if rid == 'win-x64' else 'tar.gz'}"
        for rid in SUPPORTED_RIDS
    }
    actual_archives = {p.name for p in standalone.iterdir() if p.name.endswith((".zip", ".tar.gz"))}
    if expected_archives != actual_archives:
        raise ValueError(f"standalone archives missing {sorted(expected_archives - actual_archives)}, unexpected {sorted(actual_archives - expected_archives)}")

    for name in expected_packages:
        path = packages / name
        if path.is_symlink() or not path.is_file():
            raise ValueError(f"unsafe NuGet artifact path {name}")
        package_id = name[: -len(f".{version}.nupkg")]
        metadata = _read_nuspec(path, package_id, version)
        if package_id == PACKAGE_ID:
            _validate_pointer(metadata, version)

    for rid in SUPPORTED_RIDS:
        name = f"dotnet-binary-{version}-{rid}.{'zip' if rid == 'win-x64' else 'tar.gz'}"
        path = standalone / name
        if path.is_symlink() or not path.is_file():
            raise ValueError(f"unsafe standalone artifact path {name}")
        _validate_native_archive(path, rid)

    artifacts = []
    for kind, folder, names in (("nuget", packages, expected_packages), ("standalone", standalone, expected_archives)):
        for name in sorted(names):
            artifacts.append({"kind": kind, "name": name, "sha256": _sha256(folder / name), "sizeBytes": (folder / name).stat().st_size})
    return {
        "schemaVersion": 1,
        "packageId": PACKAGE_ID,
        "version": version,
        "sourceSha": source_sha.lower(),
        "supportedRids": list(SUPPORTED_RIDS),
        "artifacts": artifacts,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--packages", required=True, type=Path)
    parser.add_argument("--standalone", required=True, type=Path)
    parser.add_argument("--version", required=True)
    parser.add_argument("--source-sha", required=True)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    try:
        report = validate_distribution(args.packages, args.standalone, args.version, args.source_sha)
        if args.output.exists():
            raise ValueError("refusing to overwrite an existing distribution inventory")
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with args.output.open("x", encoding="utf-8") as stream:
            json.dump(report, stream, indent=2)
            stream.write("\n")
        print(f"Validated {len(report['artifacts'])} unpublished release-candidate artifacts")
        return 0
    except (OSError, ValueError, zipfile.BadZipFile, tarfile.TarError) as exc:
        print(f"Distribution validation failed: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
