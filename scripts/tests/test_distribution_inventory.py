"""Acceptance and negative controls for the unpublished CAP-008 artifact set."""
from __future__ import annotations

import io
import json
import sys
import tarfile
import tempfile
import unittest
import zipfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from distribution_inventory import validate_distribution  # noqa: E402


VERSION = "0.1.0-preview.1"
SOURCE_SHA = "a" * 40
ID = "Infoconex.DotNetBinary.Tools"
RIDS = ("win-x64", "linux-x64", "osx-x64")


class DistributionInventoryTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.packages = self.root / "packages"
        self.archives = self.root / "standalone"
        self.packages.mkdir()
        self.archives.mkdir()
        self.create_packages()
        self.create_archives()

    def create_packages(self):
        for rid in (*RIDS, None):
            package_id = ID if rid is None else f"{ID}.{rid}"
            dependencies = "" if rid else "".join(
                f'<dependency id="{ID}.{target}" version="[{VERSION}]" />'
                for target in RIDS
            )
            nuspec = (
                '<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">'
                f'<metadata><id>{package_id}</id><version>{VERSION}</version>'
                f'<dependencies><group targetFramework="net11.0">{dependencies}</group></dependencies>'
                '</metadata></package>'
            )
            with zipfile.ZipFile(self.packages / f"{package_id}.{VERSION}.nupkg", "w") as package:
                package.writestr(f"{package_id}.nuspec", nuspec)
                package.writestr("tools/net11.0/any/placeholder", "test")

    def create_archives(self):
        for rid in RIDS:
            filename = "dotnet-binary.exe" if rid == "win-x64" else "dotnet-binary"
            base = f"dotnet-binary-{VERSION}-{rid}"
            if rid == "win-x64":
                with zipfile.ZipFile(self.archives / (base + ".zip"), "w") as archive:
                    archive.writestr(filename, b"native")
            else:
                with tarfile.open(self.archives / (base + ".tar.gz"), "w:gz") as archive:
                    info = tarfile.TarInfo(filename)
                    info.mode = 0o755
                    info.size = 6
                    archive.addfile(info, io.BytesIO(b"native"))

    def test_SC_DST_004_accepts_one_pointer_three_RIDs_and_three_archives(self):
        result = validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)
        self.assertEqual(7, len(result["artifacts"]))
        self.assertEqual(list(RIDS), result["supportedRids"])
        self.assertEqual(SOURCE_SHA, result["sourceSha"])
        self.assertTrue(all(len(item["sha256"]) == 64 for item in result["artifacts"]))
        self.assertNotIn("arm64", json.dumps(result).lower())

    def test_SC_DST_007_missing_supported_RID_package_blocks_assembly(self):
        (self.packages / f"{ID}.osx-x64.{VERSION}.nupkg").unlink()
        with self.assertRaisesRegex(ValueError, "missing"):
            validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)

    def test_SC_DST_009_unsupported_ARM64_package_is_rejected(self):
        (self.packages / f"{ID}.osx-arm64.{VERSION}.nupkg").write_bytes(b"not approved")
        with self.assertRaisesRegex(ValueError, "unexpected"):
            validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)

    def test_pointer_manifest_must_reference_all_exact_versioned_RID_packages(self):
        pointer = self.packages / f"{ID}.{VERSION}.nupkg"
        with zipfile.ZipFile(pointer, "w") as package:
            package.writestr(f"{ID}.nuspec", f'<package><metadata><id>{ID}</id><version>{VERSION}</version></metadata></package>')
        with self.assertRaisesRegex(ValueError, "dependencies"):
            validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)

    def test_wrong_standalone_executable_name_is_rejected(self):
        archive_path = self.archives / f"dotnet-binary-{VERSION}-win-x64.zip"
        with zipfile.ZipFile(archive_path, "w") as archive:
            archive.writestr("unexpected.exe", "binary")
        with self.assertRaisesRegex(ValueError, "dotnet-binary.exe"):
            validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)

    def test_archive_path_escape_is_rejected(self):
        archive_path = self.archives / f"dotnet-binary-{VERSION}-win-x64.zip"
        with zipfile.ZipFile(archive_path, "w") as archive:
            archive.writestr("dotnet-binary.exe", "binary")
            archive.writestr("../escape", "unsafe")
        with self.assertRaisesRegex(ValueError, "unsafe"):
            validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)

    def test_unix_archive_must_preserve_executable_mode(self):
        archive_path = self.archives / f"dotnet-binary-{VERSION}-linux-x64.tar.gz"
        with tarfile.open(archive_path, "w:gz") as archive:
            info = tarfile.TarInfo("dotnet-binary")
            info.mode = 0o644
            info.size = 6
            archive.addfile(info, io.BytesIO(b"native"))
        with self.assertRaisesRegex(ValueError, "executable"):
            validate_distribution(self.packages, self.archives, VERSION, SOURCE_SHA)

    def test_invalid_source_sha_cannot_be_reported_as_release_provenance(self):
        with self.assertRaisesRegex(ValueError, "source SHA"):
            validate_distribution(self.packages, self.archives, VERSION, "unverified")

    def test_SC_DST_008_pull_request_workflow_has_no_publication_action(self):
        workflow = (Path(__file__).resolve().parents[2] / ".github/workflows/native-aot-smoke.yml").read_text(encoding="utf-8")
        self.assertIn("contents: read", workflow)
        self.assertNotIn("packages: write", workflow)
        self.assertNotIn("dotnet nuget push", workflow)
        self.assertNotIn("gh release create", workflow)
        self.assertNotIn("git tag ", workflow)


if __name__ == "__main__":
    unittest.main()
