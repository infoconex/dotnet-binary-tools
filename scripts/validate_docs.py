#!/usr/bin/env python3
"""Validate v0.1 requirement, scenario, capability and traceability integrity.

Structural documentation verification does not establish feature acceptance.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parent.parent
CAPABILITIES = {
    "INP": "CAP-001",
    "ASM": "CAP-002",
    "CFG": "CAP-003",
    "LCY": "CAP-004",
    "INV": "CAP-005",
    "RPT": "CAP-006",
    "CLI": "CAP-007",
    "DST": "CAP-008",
}
FR_ID = re.compile(r"^FR-([A-Z]{3})-\d{3}$")
NFR_ID = re.compile(r"^NFR-\d{3}$")
SCENARIO = re.compile(r"^### ((?:SC-[A-Z]{3}-\d{3}|NFR-\d{3}-SC\d{2})): (.+)$", re.M)
TRACE_LINK = re.compile(r"^\[([^\]]+)\]\(([^)]+)\)$")
MARKDOWN_LINK = re.compile(r"(?<!!)\[[^\]]*\]\(([^)]+)\)")
ERRORS: list[str] = []


def error(message: str) -> None:
    ERRORS.append(message)


def load(relative: str) -> str:
    path = ROOT / relative
    try:
        return path.read_text(encoding="utf-8")
    except OSError as exc:
        error(f"{relative}: cannot read: {exc}")
        return ""


def columns(line: str) -> list[str]:
    return [part.strip() for part in line.strip().strip("|").split("|")]


def read_catalog() -> tuple[dict[str, tuple[str, str]], dict[str, str]]:
    functional: dict[str, tuple[str, str]] = {}
    quality: dict[str, str] = {}
    source = "docs/requirements/functional.md"
    for line in load(source).splitlines():
        if not line.startswith("| FR-"):
            continue
        cells = columns(line)
        if len(cells) != 4:
            error(f"{source}: malformed requirement row: {line}")
            continue
        identity, capability, description, link = cells
        match = FR_ID.fullmatch(identity)
        if match is None or match.group(1) not in CAPABILITIES:
            error(f"{source}: invalid requirement ID {identity}")
            continue
        expected_cap = CAPABILITIES[match.group(1)]
        if capability != expected_cap:
            error(f"{identity}: expected {expected_cap}, found {capability}")
        if link != f"[Scenarios](../scenarios/functional/{identity}.md)":
            error(f"{identity}: requirement scenario link is incorrect")
        if identity in functional:
            error(f"Duplicate functional requirement {identity}")
        functional[identity] = (capability, description)
    if len(functional) != 71:
        error(f"Expected 71 functional requirements, found {len(functional)}")

    source = "docs/requirements/non-functional.md"
    for line in load(source).splitlines():
        if not line.startswith("| NFR-"):
            continue
        cells = columns(line)
        if len(cells) != 3:
            error(f"{source}: malformed quality requirement row: {line}")
            continue
        identity, description, link = cells
        if not NFR_ID.fullmatch(identity):
            error(f"{source}: invalid quality requirement ID {identity}")
            continue
        if link != f"[Scenarios](../scenarios/non-functional/{identity}.md)":
            error(f"{identity}: quality scenario link is incorrect")
        if identity in quality:
            error(f"Duplicate quality requirement {identity}")
        quality[identity] = description
    expected_quality = {f"NFR-{n:03d}" for n in range(1, 5)}
    if set(quality) != expected_quality:
        error(f"Expected four proposed NFRs {sorted(expected_quality)}, found {sorted(quality)}")
    return functional, quality


def check_scenarios(
    functional: dict[str, tuple[str, str]], quality: dict[str, str]
) -> dict[str, tuple[str, str, str]]:
    expected_files = {
        "functional": {f"{identity}.md" for identity in functional},
        "non-functional": {f"{identity}.md" for identity in quality},
    }
    for folder, expected in expected_files.items():
        base = ROOT / "docs/scenarios" / folder
        actual = {p.name for p in base.glob("*.md")}
        if actual != expected:
            error(
                f"docs/scenarios/{folder}: missing={sorted(expected - actual)}, "
                f"orphaned={sorted(actual - expected)}"
            )

    mapping: dict[str, tuple[str, str, str]] = {}
    for identity in [*functional, *quality]:
        is_fr = identity in functional
        folder = "functional" if is_fr else "non-functional"
        relative = f"docs/scenarios/{folder}/{identity}.md"
        source = load(relative)
        if not source.startswith(f"# {identity}: "):
            error(f"{relative}: incorrect title")
        if is_fr and not identity.startswith("FR-INP-"):
            description = functional[identity][1]
            if f"## Requirement\n\n{description}\n" not in source:
                error(f"{relative}: requirement text differs from catalog")
        matches = list(SCENARIO.finditer(source))
        if not matches:
            error(f"{relative}: missing Given/When/Then scenarios")
        for index, match in enumerate(matches):
            scenario = match.group(1)
            required_prefix = (
                "SC-" + identity.split("-")[1] + "-"
                if is_fr
                else identity + "-SC"
            )
            if not scenario.startswith(required_prefix):
                error(f"{relative}: invalid scenario prefix {scenario}")
            if scenario in mapping:
                error(f"Duplicate acceptance scenario {scenario}")
            cap = functional[identity][0] if is_fr else "Proposed"
            mapping[scenario] = (identity, cap, relative)
            finish = matches[index + 1].start() if index + 1 < len(matches) else len(source)
            block = source[match.end():finish]
            for word in ("Given", "When", "Then"):
                if not re.search(rf"^\*\*{word}\*\*\s+\S+", block, re.M):
                    error(f"{relative}: {scenario} missing {word}")
                if re.search(rf"^\*\*{word}\*\*\s+{word}\b", block, re.M | re.I):
                    error(f"{relative}: {scenario} repeats {word}")
        index_path = load("docs/scenarios/README.md")
        short_folder = "functional" if is_fr else "non-functional"
        if f"]({short_folder}/{identity}.md)" not in index_path:
            error(f"docs/scenarios/README.md: missing index for {identity}")
    functional_count = sum(1 for item in mapping.values() if item[0] in functional)
    quality_count = len(mapping) - functional_count
    if functional_count != 102 or quality_count != 8:
        error(
            f"Expected 102 functional and 8 proposed-quality scenarios; "
            f"found {functional_count} and {quality_count}"
        )
    return mapping


def check_traceability(mapping: dict[str, tuple[str, str, str]]) -> None:
    relative = "docs/testing/traceability.md"
    documented: set[str] = set()
    for line in load(relative).splitlines():
        if not line.startswith("| [SC-") and not line.startswith("| [NFR-"):
            continue
        cells = columns(line)
        if len(cells) != 7:
            error(f"{relative}: malformed traceability row: {line}")
            continue
        match = TRACE_LINK.fullmatch(cells[0])
        if match is None:
            error(f"{relative}: malformed scenario link {cells[0]}")
            continue
        scenario, destination = match.groups()
        if scenario in documented:
            error(f"{relative}: duplicate traceability {scenario}")
        documented.add(scenario)
        expected = mapping.get(scenario)
        if expected is None:
            error(f"{relative}: unknown scenario {scenario}")
            continue
        requirement, capability, path = expected
        expected_destination = "../" + path.removeprefix("docs/")
        if cells[1] != capability or cells[2] != requirement:
            error(f"{scenario}: traceability requirement or capability mismatch")
        if destination != expected_destination:
            error(f"{scenario}: traceability link should be {expected_destination}")
    if documented != set(mapping):
        error(f"{relative}: scenarios missing from traceability: {sorted(set(mapping) - documented)}")


def check_governance(functional: dict[str, tuple[str, str]]) -> None:
    """Verify the README capability index and one detailed file per approved CAP."""
    index = load("docs/capabilities/README.md")
    architecture = load("docs/requirements/architecture.md")
    capability_folder = ROOT / "docs/capabilities"
    expected_ids = [f"CAP-{number:03d}" for number in range(1, 9)]
    expected_files = {"README.md"} | {f"{cap}.md" for cap in expected_ids}
    actual_files = {path.name for path in capability_folder.glob("*.md")}
    if actual_files != expected_files:
        error(
            "docs/capabilities: incorrect file set; "
            f"missing={sorted(expected_files - actual_files)}, "
            f"orphaned={sorted(actual_files - expected_files)}"
        )
    if (ROOT / "docs/capabilities.md").exists():
        error("Obsolete docs/capabilities.md: use docs/capabilities/README.md")

    indexed = re.findall(r"^\|\s*\[(CAP-\d{3})\]\(([^)]+)\)\s*\|", index, re.M)
    expected_index = [(cap, f"{cap}.md") for cap in expected_ids]
    if indexed != expected_index:
        error(
            "docs/capabilities/README.md: expected exactly one ordered linked "
            f"index row per capability; found {indexed}"
        )

    for number, cap in enumerate(expected_ids, start=1):
        relative = f"docs/capabilities/{cap}.md"
        source = load(relative)
        if not re.match(rf"^# {cap}: \S[^\n]*\n", source):
            error(f"{relative}: missing capability ID and title")
        for heading in (
            "Purpose",
            "Approved behavior",
            "Inputs and outputs",
            "Boundaries and exclusions",
            "Failure and completeness behavior",
            "Requirement coverage",
            "Verification status",
        ):
            if f"## {heading}\n" not in source:
                error(f"{relative}: missing section {heading}")

        linked = re.findall(
            r"^- \[(FR-[A-Z]{3}-\d{3})\]\(([^)]+)\)$",
            source,
            re.M,
        )
        expected_requirements = {
            identity
            for identity, (capability, _) in functional.items()
            if capability == cap
        }
        found_requirements = [identity for identity, _ in linked]
        if len(found_requirements) != len(set(found_requirements)):
            error(f"{relative}: duplicate requirement links")
        if set(found_requirements) != expected_requirements:
            error(
                f"{relative}: requirement coverage mismatch; "
                f"missing={sorted(expected_requirements - set(found_requirements))}, "
                f"incorrect={sorted(set(found_requirements) - expected_requirements)}"
            )
        for identity, target in linked:
            if target != f"../scenarios/functional/{identity}.md":
                error(f"{relative}: incorrect scenario link for {identity}: {target}")
        if not re.search(r"\bPlanned\b", source):
            error(f"{relative}: missing unverified acceptance status")
        if not re.search(rf"^\|\s*ARCH-{number:03d}\s*\|", architecture, re.M):
            error(f"Missing ARCH-{number:03d} from architectural constraints")


def check_links_and_retired_refs() -> None:
    paths = [
        ROOT / "README.md",
        ROOT / "AGENTS.md",
        *ROOT.glob("docs/**/*.md"),
        *ROOT.glob(".github/**/*.md"),
    ]
    retired = (
        "Infoconex.DotNetBinary.Core",
        "Infoconex.DotNetFramework",
        "DotNetFramework.slnx",
        "dotnet-framework",
        "docs/capabilities.md",
        "0001-core-cli-boundary.md",
        "0003-hexagonal-project-boundaries.md",
        "docs/scenarios/functional/FR-001.md",
    )
    root_resolved = ROOT.resolve()
    for path in sorted(set(paths)):
        if not path.is_file():
            continue
        source = path.read_text(encoding="utf-8")
        for item in retired:
            if item in source:
                error(f"{path.relative_to(ROOT)}: obsolete reference {item}")
        for target in MARKDOWN_LINK.findall(source):
            url = unquote(target.strip().split(" ", 1)[0].strip("<>"))
            if not url or url.startswith(("#", "http:", "https:", "mailto:")):
                continue
            relative = url.split("#", 1)[0].split("?", 1)[0]
            if not relative:
                continue
            resolved = (path.parent / relative).resolve()
            if not resolved.is_relative_to(root_resolved) or not resolved.exists():
                error(f"{path.relative_to(ROOT)}: broken local link {target}")


def main() -> int:
    functional, quality = read_catalog()
    mapping = check_scenarios(functional, quality)
    check_traceability(mapping)
    check_governance(functional)
    check_links_and_retired_refs()
    for problem in ERRORS:
        print(f"ERROR: {problem}", file=sys.stderr)
    if ERRORS:
        print(f"Validation failed: {len(ERRORS)} issue(s).", file=sys.stderr)
        return 1
    print(
        f"Validated {len(functional)} functional requirements, "
        f"{len(quality)} proposed quality requirements, {len(mapping)} scenarios, "
        "capability/architecture mappings, traceability, filenames and links."
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
