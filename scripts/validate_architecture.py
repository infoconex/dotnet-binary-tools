#!/usr/bin/env python3
"""Check approved hexagonal project references, solution membership and AOT identity."""
from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PROJECTS = {
    "domain": ROOT / "src/Infoconex.DotNetFramework.Domain/Infoconex.DotNetFramework.Domain.csproj",
    "application": ROOT / "src/Infoconex.DotNetFramework.Application/Infoconex.DotNetFramework.Application.csproj",
    "infrastructure": ROOT / "src/Infoconex.DotNetFramework.Infrastructure/Infoconex.DotNetFramework.Infrastructure.csproj",
    "cli": ROOT / "src/Infoconex.DotNetFramework.Cli/Infoconex.DotNetFramework.Cli.csproj",
    "domain-tests": ROOT / "tests/Infoconex.DotNetFramework.Domain.Tests/Infoconex.DotNetFramework.Domain.Tests.csproj",
    "cli-tests": ROOT / "tests/Infoconex.DotNetFramework.Cli.Tests/Infoconex.DotNetFramework.Cli.Tests.csproj",
}
EXPECTED = {
    "domain": set(),
    "application": {"domain"},
    "infrastructure": {"application"},
    "cli": {"application", "infrastructure"},
    "domain-tests": {"domain"},
    "cli-tests": {"cli"},
}
errors: list[str] = []


def parse(path: Path) -> ET.Element | None:
    if not path.is_file():
        errors.append(f"Missing {path.relative_to(ROOT)}")
        return None
    try:
        return ET.parse(path).getroot()
    except ET.ParseError as exc:
        errors.append(f"Bad XML {path.relative_to(ROOT)}: {exc}")
        return None


def property_values(root: ET.Element, name: str) -> list[str]:
    return [n.text.strip() for n in root.findall(".//" + name) if n.text]


def check_graph() -> None:
    names = {path.name: label for label, path in PROJECTS.items()}
    solution = parse(ROOT / "DotNetFramework.slnx")
    if solution is not None:
        actual = {n.attrib.get("Path", "").replace("\\", "/") for n in solution.findall(".//Project")}
        required = {p.relative_to(ROOT).as_posix() for p in PROJECTS.values()}
        if solution.tag != "Solution" or actual != required:
            errors.append(f"Wrong solution membership: {sorted(actual)}")
    for label, path in PROJECTS.items():
        root = parse(path)
        if root is None:
            continue
        actual = set()
        for reference in root.findall(".//ProjectReference"):
            filename = Path(reference.get("Include", "").replace("\\", "/")).name
            if filename not in names:
                errors.append(f"{label}: unknown reference {filename}")
            else:
                actual.add(names[filename])
        if actual != EXPECTED[label]:
            errors.append(f"{label}: outward or missing reference {sorted(actual)} != {sorted(EXPECTED[label])}")
        if label == "domain" and root.findall(".//PackageReference"):
            errors.append("Domain must not directly depend on packages")
        if label == "cli":
            for key, expected in {
                "PackAsTool": "true",
                "ToolCommandName": "dotnet-framework",
                "PackageId": "Infoconex.DotNetFramework.Tools",
                "PublishAot": "true",
                "IsAotCompatible": "true",
            }.items():
                if property_values(root, key) != [expected]:
                    errors.append(f"CLI: {key} must be {expected}")
            for key in ("RuntimeIdentifiers", "ToolPackageRuntimeIdentifiers"):
                values = property_values(root, key)
                if len(values) != 1 or set(values[0].split(";")) != {"win-x64", "linux-x64", "osx-x64"}:
                    errors.append(f"CLI: {key} must list three x64 RIDs")
    if any((ROOT / "src/Infoconex.DotNetFramework.Core").rglob("*")) or any((ROOT / "tests/Infoconex.DotNetFramework.Core.Tests").rglob("*")):
        errors.append("Superseded Core projects remain")


def check_build_config() -> None:
    if (ROOT / "DotNetFramework.sln").exists():
        errors.append("Legacy .sln is not allowed")
    config = parse(ROOT / "nuget.config")
    if config is not None:
        if config.tag != "configuration":
            errors.append("Bad nuget.config root")
        for section in ("packageSources", "disabledPackageSources", "packageSourceMapping", "auditSources"):
            node = config.find(section)
            if node is None or node.find("clear") is None:
                errors.append(f"nuget.config: {section} must clear inherited configuration")
        sources = config.findall("./packageSources/add")
        if len(sources) != 1 or sources[0].get("key") != "nuget.org" or sources[0].get("value") != "https://api.nuget.org/v3/index.json":
            errors.append("nuget.config must select only nuget.org")
    if (ROOT / "NuGet.Config").exists():
        errors.append("Use lowercase nuget.config")
    props = parse(ROOT / "Directory.Packages.props")
    if props is not None:
        if property_values(props, "ManagePackageVersionsCentrally") != ["true"] or not props.findall(".//PackageVersion"):
            errors.append("Central package management is missing")


def check_no_execution() -> None:
    for path in (ROOT / "src").rglob("*.cs"):
        source = path.read_text(encoding="utf-8")
        for expression in ("Assembly.Load(", "Assembly.LoadFrom(", "Assembly.LoadFile(", "AssemblyLoadContext.LoadFromAssemblyPath("):
            if expression in source:
                errors.append(f"{path.relative_to(ROOT)}: execution-capable inspection API {expression}")


def main() -> int:
    check_graph()
    check_build_config()
    check_no_execution()
    for message in errors:
        print(f"ERROR: {message}", file=sys.stderr)
    if errors:
        return 1
    print("Validated inward Domain/Application/Infrastructure/CLI project references and x64 Native AOT metadata.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
