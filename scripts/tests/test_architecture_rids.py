"""The restore graph can include Apple Silicon without expanding x64 releases."""
from __future__ import annotations

import sys
import unittest
from pathlib import Path
from xml.etree import ElementTree

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from validate_architecture import validate_cli_rids  # noqa: E402


def project(restore_rids: str, tool_rids: str) -> ElementTree.Element:
    return ElementTree.fromstring(
        "<Project><PropertyGroup>"
        f"<RuntimeIdentifiers>{restore_rids}</RuntimeIdentifiers>"
        f"<ToolPackageRuntimeIdentifiers>{tool_rids}</ToolPackageRuntimeIdentifiers>"
        "</PropertyGroup></Project>"
    )


class CliRidTests(unittest.TestCase):
    def test_four_restore_rids_with_three_x64_tool_rids_are_approved(self):
        root = project("win-x64;linux-x64;osx-x64;osx-arm64", "win-x64;linux-x64;osx-x64")
        self.assertEqual([], validate_cli_rids(root))

    def test_apple_silicon_must_be_in_common_restore_graph(self):
        root = project("win-x64;linux-x64;osx-x64", "win-x64;linux-x64;osx-x64")
        self.assertTrue(any("RuntimeIdentifiers" in issue for issue in validate_cli_rids(root)))

    def test_apple_silicon_must_not_be_advertised_as_a_tool_package(self):
        all_rids = "win-x64;linux-x64;osx-x64;osx-arm64"
        root = project(all_rids, all_rids)
        self.assertTrue(any("ToolPackageRuntimeIdentifiers" in issue for issue in validate_cli_rids(root)))

    def test_duplicate_restore_identifier_is_rejected(self):
        root = project("win-x64;linux-x64;osx-x64;osx-arm64;osx-arm64", "win-x64;linux-x64;osx-x64")
        self.assertTrue(any("RuntimeIdentifiers" in issue for issue in validate_cli_rids(root)))


if __name__ == "__main__":
    unittest.main()
