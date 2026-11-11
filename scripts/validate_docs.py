#!/usr/bin/env python3
"""Validate requirement catalogs, normative scenarios, traceability and local Markdown links.

Uses Python's standard library. This validates documentation structure,
not C# behavior and not the execution state of planned scenarios.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parent.parent
REQUIREMENT_ROW = re.compile(r"^\|\s*((?:FR|NFR)-\d{3})\s*\|", re.M)
SCENARIO_HEADING = re.compile(r"^###\s+((?:FR|NFR)-\d{3}-SC\d{2}):\s+(.+)$", re.M)
TRACE_ROW = re.compile(r"^\|\s*\[((?:FR|NFR)-\d{3}-SC\d{2})\]\(", re.M)
MARKDOWN_LINK = re.compile(r"(?<!!)\[[^\]]*\]\(([^)]+)\)")
errors: list[str] = []


def error(message: str) -> None:
    errors.append(message)


def load(relative: str) -> str:
    path = ROOT / relative
    if not path.is_file():
        error(f"Missing required file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


def catalog(prefix: str, name: str) -> set[str]:
    rows = REQUIREMENT_ROW.findall(load(f"docs/requirements/{name}.md"))
    found: set[str] = set()
    for key in rows:
        if not key.startswith(prefix + "-"):
            error(f"docs/requirements/{name}.md: unexpected prefix {key}")
        if key in found:
            error(f"docs/requirements/{name}.md: duplicate {key}")
        found.add(key)
    if not found:
        error(f"docs/requirements/{name}.md: no {prefix} requirements")
    return found


def validate_scenarios(requirements: set[str]) -> set[str]:
    found: set[str] = set()
    for requirement in sorted(requirements):
        folder = "functional" if requirement.startswith("FR-") else "non-functional"
        relative = f"docs/scenarios/{folder}/{requirement}.md"
        content = load(relative)
        if not content.startswith(f"# {requirement}: "):
            error(f"{relative}: incorrect title")
        matches = SCENARIO_HEADING.findall(content)
        if not matches:
            error(f"{relative}: no Given/When/Then scenarios")
        for scenario, _ in matches:
            if not scenario.startswith(requirement + "-SC"):
                error(f"{relative}: unexpected scenario {scenario}")
            if scenario in found:
                error(f"Duplicate scenario: {scenario}")
            found.add(scenario)
            start = content.index("### " + scenario)
            following = content.find("\n### ", start + 4)
            section = content[start:following if following >= 0 else len(content)]
            for marker in ("**Given**", "**When**", "**Then**"):
                if marker not in section:
                    error(f"{relative}: {scenario} missing {marker}")
    return found


def validate_traceability(scenarios: set[str]) -> None:
    text = load("docs/testing/traceability.md")
    rows = TRACE_ROW.findall(text)
    seen: set[str] = set()
    for row in rows:
        if row in seen:
            error(f"Traceability: duplicate {row}")
        seen.add(row)
    for missing in sorted(scenarios - seen):
        error(f"Traceability: missing {missing}")
    for unknown in sorted(seen - scenarios):
        error(f"Traceability: undocumented {unknown}")


def validate_links() -> None:
    for path in sorted([ROOT / "README.md", ROOT / "AGENTS.md", *ROOT.glob("docs/**/*.md")]):
        if not path.is_file():
            continue
        content = path.read_text(encoding="utf-8")
        for target in MARKDOWN_LINK.findall(content):
            url = unquote(target.strip().split(" ", 1)[0].strip("<>"))
            if not url or url.startswith(("#", "http:", "https:", "mailto:")):
                continue
            relative_target = url.split("#", 1)[0].split("?", 1)[0]
            if not relative_target:
                continue
            destination = (path.parent / relative_target).resolve()
            if not destination.is_relative_to(ROOT.resolve()) or not destination.exists():
                error(f"{path.relative_to(ROOT)}: broken local link {target}")


def main() -> int:
    requirements = catalog("FR", "functional") | catalog("NFR", "non-functional")
    scenarios = validate_scenarios(requirements)
    validate_traceability(scenarios)
    validate_links()
    for item in errors:
        print(f"ERROR: {item}", file=sys.stderr)
    if errors:
        print(f"Documentation validation failed: {len(errors)} problem(s).", file=sys.stderr)
        return 1
    print(f"Validated {len(requirements)} requirements and {len(scenarios)} scenario rows; local links resolve.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
