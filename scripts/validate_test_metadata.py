#!/usr/bin/env python3
"""Check scenario IDs referenced in NUnit test metadata against active specifications."""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT=Path(__file__).resolve().parent.parent
SC=re.compile(r"^###\s+((?:SC-[A-Z]{3}-\d{3}|NFR-\d{3}-SC\d{2})):", re.M)
TEST=re.compile(r'\[Property\("Scenario",\s*"((?:SC-[A-Z]{3}-\d{3}|NFR-\d{3}-SC\d{2}))"\)\]')


def main():
    defined=set()
    for folder in ("functional", "non-functional"):
        root = ROOT / "docs/scenarios" / folder
        scenario_files = (
            root.rglob("FR-*.md") if folder == "functional" else root.glob("NFR-*.md")
        )
        for path in scenario_files:
            defined.update(SC.findall(path.read_text(encoding="utf-8")))
    sources=list((ROOT/"tests").rglob("*.cs"))
    test_count=sum(("[Test]" in s.read_text(encoding="utf-8") or "[TestCase(" in s.read_text(encoding="utf-8")) for s in sources)
    errors=[]
    if test_count<2:
        errors.append("Expected two test-bearing files")
    for path in sources:
        for scenario in TEST.findall(path.read_text(encoding="utf-8")):
            if scenario not in defined:
                errors.append(f"{path.relative_to(ROOT)}: undocumented {scenario}")
    for item in errors:
        print(f"ERROR: {item}",file=sys.stderr)
    if errors:
        return 1
    print(f"Validated {len(sources)} test source files ({test_count} test-bearing).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
