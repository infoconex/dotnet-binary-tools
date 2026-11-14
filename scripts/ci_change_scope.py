#!/usr/bin/env python3
"""Classify PR/push file changes to avoid unnecessary CI work.

Never skip an applicable check after a change-detection failure. A manual run
intentionally runs all checks. Intended for pull_request, push and workflow_dispatch.
"""
from __future__ import annotations

import os
import subprocess
import sys
from pathlib import PurePosixPath
from typing import Iterable

OUTPUTS = ("documentation", "managed", "aot", "architecture", "test_metadata", "line_endings")
FULL_BUILD_CONFIG = {
    "DotNetBinary.slnx",
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "nuget.config",
    "global.json",
    ".gitattributes",
}
DOC_ROOT = {"README.md", "AGENTS.md"}
ALL_CHECKS_FILES = {"scripts/ci_change_scope.py", "scripts/tests/test_ci_change_scope.py"}


def classify(changed_files: Iterable[str]) -> dict[str, bool]:
    result = dict.fromkeys(OUTPUTS, False)
    for path in changed_files:
        result["line_endings"] = True

        if path in ALL_CHECKS_FILES:
            return dict.fromkeys(OUTPUTS, True)

        # Documentation and its validator run only when the source of
        # documentation or its validation rules changes.
        if path in DOC_ROOT or path.startswith("docs/") or (
            path.startswith(".github/") and path.endswith(".md")
        ) or path == "scripts/validate_docs.py":
            result["documentation"] = True

        if path.startswith("docs/") or path.startswith("tests/") or path == "scripts/validate_test_metadata.py":
            result["test_metadata"] = True

        if path.startswith("src/"):
            result["managed"] = True
            result["aot"] = True
            result["architecture"] = True

        if path.startswith("tests/"):
            result["managed"] = True
            if path.endswith(".csproj"):
                result["architecture"] = True

        if path in FULL_BUILD_CONFIG or path.endswith(".props") or path.endswith(".targets"):
            result["managed"] = True
            result["aot"] = True
            result["architecture"] = True

        if path in {
            ".editorconfig",
            ".github/workflows/build-test.yml",
            "coverage.runsettings",
            "scripts/collect-coverage.ps1",
            "scripts/validate-managed.ps1",
            "scripts/coverage_report.py",
            "scripts/tests/test_coverage_report.py",
        }:
            result["managed"] = True

        if path in {".github/workflows/native-aot-smoke.yml", "scripts/validate-aot.ps1", "scripts/validate-tool-package.ps1"}:
            result["aot"] = True

        if path in {"scripts/validate_architecture.py", ".github/workflows/validate-engineering.yml"}:
            result["architecture"] = True

        if path in {"scripts/validate_docs.py", ".github/workflows/validate-engineering.yml"}:
            result["test_metadata"] = True

    return result


def changes_from_git(event: str, base: str, head: str) -> list[str]:
    if not head or not base:
        raise ValueError("Both base and head commits are required")
    if len(base) != 40 or len(head) != 40 or any(c not in "0123456789abcdef" for c in (base + head).lower()):
        raise ValueError("Expected 40-character Git commit IDs")
    if base == "0" * 40:
        # Initial push: safest behavior is to run all checks.
        return ["scripts/ci_change_scope.py"]
    diff = f"{base}...{head}" if event == "pull_request" else f"{base}..{head}"
    completed = subprocess.run(
        ["git", "diff", "--name-only", "-z", diff],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=True,
    )
    return [os.fsdecode(name) for name in completed.stdout.split(b"\0") if name]


def main() -> int:
    event = os.environ.get("GITHUB_EVENT_NAME", "")
    if event == "workflow_dispatch":
        flags = dict.fromkeys(OUTPUTS, True)
    elif event in {"push", "pull_request"}:
        try:
            paths = changes_from_git(
                event,
                os.environ.get("BASE_SHA", ""),
                os.environ.get("HEAD_SHA", ""),
            )
            flags = classify(paths)
            print(f"Classified {len(paths)} changed paths")
        except (ValueError, OSError, subprocess.CalledProcessError) as exc:
            print(f"Change detection failed; CI must not silently skip checks: {exc}", file=sys.stderr)
            return 1
    else:
        print(f"Unsupported workflow event {event!r}", file=sys.stderr)
        return 1

    output = os.environ.get("GITHUB_OUTPUT", "")
    if not output:
        print("GITHUB_OUTPUT is unavailable; refusing to skip checks without outputs", file=sys.stderr)
        return 1
    with open(output, "a", encoding="utf-8") as stream:
        for key in OUTPUTS:
            value = "true" if flags[key] else "false"
            stream.write(f"{key}={value}\n")
            print(f"{key}: {value}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
