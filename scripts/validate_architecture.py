#!/usr/bin/env python3
"""Enforce the small Core -> CLI dependency graph and AOT tool metadata."""
from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECTS = {
    "core": ROOT / "src/Infoconex.DotNetFramework.Core/Infoconex.DotNetFramework.Core.csproj",
    "cli": ROOT / "src/Infoconex.DotNetFramework.Cli/Infoconex.DotNetFramework.Cli.csproj",
    "core-tests": ROOT / "tests/Infoconex.DotNetFramework.Core.Tests/Infoconex.DotNetFramework.Core.Tests.csproj",
    "cli-tests": ROOT / "tests/Infoconex.DotNetFramework.Cli.Tests/Infoconex.DotNetFramework.Cli.Tests.csproj",
}
ERRORS: list[str] = []


def report(message: str) -> None:
    ERRORS.append(message)


def project(name: str) -> ET.Element | None:
    path = PROJECTS[name]
    if not path.exists():
        report(f"Missing project {path.relative_to(ROOT)}")
        return None
    return ET.parse(path).getroot()


def project_refs(root: ET.Element) -> set[str]:
    return {Path(item.attrib.get("Include", "").replace("\\", "/")).name
            for item in root.findall(".//ProjectReference")}


def props(root: ET.Element, name: str) -> list[str]:
    return [item.text.strip() for item in root.findall(".//" + name) if item.text]


def validate_repository_layout() -> None:
    solution = ROOT / "DotNetFramework.slnx"
    if not solution.is_file():
        report("Missing XML solution DotNetFramework.slnx")
    else:
        try:
            root = ET.parse(solution).getroot()
            expected_paths = {project.relative_to(ROOT).as_posix() for project in PROJECTS.values()}
            actual_paths = {element.attrib.get("Path", "").replace("\\", "/")
                            for element in root.iter("Project")}
            if root.tag != "Solution" or actual_paths != expected_paths:
                report(f"Invalid slnx root or project membership: {sorted(actual_paths)}")
        except ET.ParseError as exc:
            report(f"Invalid slnx XML: {exc}")
    if (ROOT / "DotNetFramework.sln").exists():
        report("Remove the legacy DotNetFramework.sln; use .slnx only")

    config = ROOT / "nuget.config"
    if not config.is_file():
        report("Missing lowercase nuget.config")
    else:
        try:
            config_root = ET.parse(config).getroot()
            if config_root.tag != "configuration":
                report("nuget.config must have a configuration root")
            for section in ("packageSources", "disabledPackageSources", "packageSourceMapping", "auditSources"):
                node = config_root.find(section)
                if node is None or node.find("clear") is None:
                    report(f"nuget.config: {section} must clear inherited configuration")
            sources = config_root.findall("./packageSources/add")
            if len(sources) != 1 or sources[0].get("key") != "nuget.org" or sources[0].get("value") != "https://api.nuget.org/v3/index.json":
                report("nuget.config must use only the official nuget.org package source")
        except ET.ParseError as exc:
            report(f"Invalid nuget.config XML: {exc}")
    if (ROOT / "NuGet.Config").exists():
        report("Remove uppercase NuGet.Config; use lowercase nuget.config only")


def validate_central_package_management() -> None:
    props = ROOT / "Directory.Packages.props"
    if not props.is_file():
        report("Missing Directory.Packages.props")
        return
    try:
        root = ET.parse(props).getroot()
        values = [node.text for node in root.findall(".//ManagePackageVersionsCentrally")]
        if values != ["true"]:
            report("Directory.Packages.props must enable central package management")
        if not root.findall(".//PackageVersion"):
            report("Directory.Packages.props must define centrally managed package versions")
    except ET.ParseError as exc:
        report(f"Invalid Directory.Packages.props XML: {exc}")


def main() -> int:
    validate_repository_layout()
    validate_central_package_management()
    expected = {
        "core": set(),
        "cli": {"Infoconex.DotNetFramework.Core.csproj"},
        "core-tests": {"Infoconex.DotNetFramework.Core.csproj"},
        "cli-tests": {"Infoconex.DotNetFramework.Cli.csproj"},
    }
    for name, required_refs in expected.items():
        xml = project(name)
        if xml is None:
            continue
        actual_refs = project_refs(xml)
        if actual_refs != required_refs:
            report(f"{name}: wrong ProjectReference set: {sorted(actual_refs)}")
        if name == "core" and xml.findall(".//PackageReference"):
            report("Core must not directly depend on third-party NuGet packages")
        if name == "cli":
            for key, value in {
                "PackAsTool": "true",
                "ToolCommandName": "dotnet-framework",
                "PackageId": "Infoconex.DotNetFramework.Tools",
                "PublishAot": "true",
                "IsAotCompatible": "true",
            }.items():
                if props(xml, key) != [value]:
                    report(f"CLI {key} must be {value}")
            rids = {"win-x64", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64"}
            for rid_property in ("RuntimeIdentifiers", "ToolPackageRuntimeIdentifiers"):
                values = props(xml, rid_property)
                if len(values) != 1 or set(values[0].split(";")) != rids:
                    report(f"CLI: {rid_property} must list six planned native RIDs")
    for path in (ROOT / "src").rglob("*.cs"):
        source = path.read_text(encoding="utf-8")
        for expression in ("Assembly.Load(", "Assembly.LoadFrom(", "Assembly.LoadFile(", "AssemblyLoadContext.LoadFromAssemblyPath("):
            if expression in source:
                report(f"{path.relative_to(ROOT)} uses execution-capable assembly loading: {expression}")
    for err in ERRORS:
        print(f"ERROR: {err}", file=sys.stderr)
    if ERRORS:
        return 1
    print("Validated Core/CLI project references and planned Native AOT metadata.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
