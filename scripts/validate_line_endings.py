#!/usr/bin/env python3
"""Validate Git's indexed text normalization and checked-out line endings.

This is an engineering gate, not functional acceptance of the inspector.
"""
from __future__ import annotations

import os
import subprocess
import sys
from pathlib import PurePosixPath

LF_EXTENSIONS = {
    ".cs", ".csx", ".csproj", ".props", ".targets", ".slnx", ".config",
    ".xml", ".md", ".json", ".yml", ".yaml", ".py",
    ".ps1", ".psm1", ".psd1", ".sh", ".txt", ".nuspec",
}
CRLF_EXTENSIONS = {".bat", ".cmd"}
LF_FILENAMES = {".editorconfig", ".gitattributes", ".gitignore", "LICENSE"}


def main() -> int:
    try:
        result = subprocess.run(
            ["git", "ls-files", "--eol", "-z"],
            check=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
    except (OSError, subprocess.CalledProcessError) as exc:
        print(f"Cannot inspect Git line endings: {exc}", file=sys.stderr)
        return 1

    problems: list[str] = []
    checked = 0
    for record in result.stdout.split(b"\0"):
        if not record:
            continue
        try:
            metadata, raw_path = record.split(b"\t", 1)
            index_eol, worktree_eol, *attributes = metadata.decode("ascii").split()
        except (ValueError, UnicodeDecodeError):
            problems.append(f"Unexpected git ls-files --eol record: {record!r}")
            continue

        path = os.fsdecode(raw_path)
        filename = PurePosixPath(path).name
        extension = PurePosixPath(path).suffix.lower()
        if extension in CRLF_EXTENSIONS:
            target = "crlf"
        elif extension in LF_EXTENSIONS or filename in LF_FILENAMES:
            target = "lf"
        else:
            continue

        checked += 1
        expected_attributes = ["attr/text", f"eol={target}"]
        if attributes != expected_attributes:
            problems.append(
                f"{path}: expected {' '.join(expected_attributes)}, got {' '.join(attributes)}"
            )
        if index_eol not in ("i/lf", "i/none"):
            problems.append(
                f"{path}: Git index has {index_eol}; expected normalized LF or no newline"
            )
        allowed_checkout = (
            ("w/lf", "w/none") if target == "lf" else ("w/crlf", "w/none")
        )
        if worktree_eol not in allowed_checkout:
            problems.append(
                f"{path}: checkout has {worktree_eol}; expected {target.upper()} or no newline"
            )

    if checked == 0:
        problems.append("No tracked text files were checked")
    for problem in problems:
        print(f"ERROR: {problem}", file=sys.stderr)
    if problems:
        print(f"Line-ending validation failed: {len(problems)} issue(s).", file=sys.stderr)
        return 1
    print(f"Validated Git index and checkout line endings for {checked} tracked text files.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
