#!/usr/bin/env python3
"""Validate active requirements, scenario traceability, capabilities and Markdown links."""
from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote

ROOT = Path(__file__).resolve().parent.parent
REQ = re.compile(r"^\|\s*((?:FR-[A-Z]{3}-\d{3}|NFR-\d{3}))\s*\|", re.M)
SC = re.compile(r"^###\s+((?:SC-[A-Z]{3}-\d{3}|NFR-\d{3}-SC\d{2})):\s+(.+)$", re.M)
TRACE = re.compile(r"^\|\s*\[((?:SC-[A-Z]{3}-\d{3}|NFR-\d{3}-SC\d{2}))\]\(", re.M)
LINK = re.compile(r"(?<!!)\[[^\]]*\]\(([^)]+)\)")
errors = []


def load(rel):
    path = ROOT / rel
    if not path.is_file():
        errors.append(f"Missing {rel}")
        return ""
    return path.read_text(encoding="utf-8")


def catalog(prefix, name):
    matches = REQ.findall(load(f"docs/requirements/{name}.md"))
    if len(matches) != len(set(matches)) or not matches:
        errors.append(f"Invalid or duplicate {prefix} catalog identifiers")
    return {r for r in matches if r.startswith(prefix + "-")}


def scenarios(required):
    ids = []
    for requirement in sorted(required):
        folder = "functional" if requirement.startswith("FR-") else "non-functional"
        rel = f"docs/scenarios/{folder}/{requirement}.md"
        content = load(rel)
        if not content.startswith(f"# {requirement}: "):
            errors.append(f"{rel}: invalid title")
        matches = SC.findall(content)
        if not matches:
            errors.append(f"{rel}: no scenarios")
        for identity, _ in matches:
            prefix = "SC-" + requirement[3:7] if folder == "functional" else requirement + "-SC"
            if not identity.startswith(prefix):
                errors.append(f"{rel}: incorrect scenario prefix for {identity}")
            start = content.index("### " + identity)
            end = content.find("\n### ", start + 4)
            section = content[start:end if end >= 0 else len(content)]
            if not all(word in section for word in ("**Given**", "**When**", "**Then**")):
                errors.append(f"{rel}: {identity} missing Given/When/Then")
            ids.append(identity)
    if len(ids) != len(set(ids)):
        errors.append("Duplicate scenario ID")
    return set(ids)


def check_traceability(expected):
    rows = TRACE.findall(load("docs/testing/traceability.md"))
    if len(rows) != len(set(rows)):
        errors.append("Duplicate traceability row")
    if set(rows) != expected:
        errors.append(f"Traceability mismatch: missing {sorted(expected-set(rows))}; unknown {sorted(set(rows)-expected)}")


def check_governance():
    capabilities = load("docs/capabilities.md")
    architecture = load("docs/requirements/architecture.md")
    for n in range(1, 9):
        if not re.search(rf"^\|\s*CAP-{n:03d}\s*\|",capabilities,re.M):
            errors.append(f"Missing CAP-{n:03d}")
        if not re.search(rf"^\|\s*ARCH-{n:03d}\s*\|",architecture,re.M):
            errors.append(f"Missing ARCH-{n:03d}")


def check_links():
    for path in [ROOT/"README.md",ROOT/"AGENTS.md",*ROOT.glob("docs/**/*.md")]:
        if not path.is_file():
            continue
        for target in LINK.findall(path.read_text(encoding="utf-8")):
            url = unquote(target.strip().split(" ",1)[0].strip("<>"))
            if not url or url.startswith(("#","http:","https:","mailto:")):
                continue
            rel = url.split("#",1)[0].split("?",1)[0]
            if rel:
                dest = (path.parent/rel).resolve()
                if not dest.is_relative_to(ROOT.resolve()) or not dest.exists():
                    errors.append(f"{path.relative_to(ROOT)}: broken link {target}")


def main():
    active = catalog("FR","functional") | catalog("NFR","non-functional")
    found = scenarios(active)
    check_traceability(found)
    check_governance()
    check_links()
    for e in errors:
        print(f"ERROR: {e}",file=sys.stderr)
    if errors:
        return 1
    print(f"Validated {len(active)} active requirements, {len(found)} scenarios, capabilities and links.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
