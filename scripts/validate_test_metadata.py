#!/usr/bin/env python3
"""Validate any acceptance scenario references added to NUnit test metadata.

Baseline unit tests may be untraced while no FR/NFR acceptance is implemented.
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SCENARIO = re.compile(r"^###\s+((?:FR|NFR)-\d{3}-SC\d{2}):", re.M)
TEST_SCENARIO = re.compile(r'\[Property\("Scenario",\s*"((?:FR|NFR)-\d{3}-SC\d{2})"\)\]')
errors: list[str] = []


def main() -> int:
    defined: set[str] = set()
    for spec in (ROOT / "docs/scenarios").rglob("*.md"):
        defined.update(SCENARIO.findall(spec.read_text(encoding="utf-8")))
    sources = list((ROOT / "tests").rglob("*.cs"))
    if len(sources) < 2:
        errors.append("Expected source files in both NUnit test projects")
    count = 0
    for path in sources:
        source = path.read_text(encoding="utf-8")
        if "[Test]" in source or "[TestCase(" in source:
            count += 1
        for scenario in TEST_SCENARIO.findall(source):
            if scenario not in defined:
                errors.append(f"{path.relative_to(ROOT)}: scenario {scenario} is not documented")
    if count < 2:
        errors.append("Expected test methods in Core.Tests and Cli.Tests")
    for item in errors:
        print(f"ERROR: {item}", file=sys.stderr)
    if errors:
        return 1
    print(f"Validated test sources: {len(sources)} files, {count} test-bearing files.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
