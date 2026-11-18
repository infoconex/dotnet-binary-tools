"""The .NET tool and restore graph must match the six approved native RIDs."""
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
    RIDS = "win-x64;win-arm64;linux-x64;linux-arm64;osx-x64;osx-arm64"

    def test_all_six_restore_and_tool_rids_are_approved(self):
        root = project(self.RIDS, self.RIDS)
        self.assertEqual([], validate_cli_rids(root))

    def test_linux_arm64_must_be_in_common_restore_graph(self):
        restore = self.RIDS.replace(";linux-arm64", "")
        root = project(restore, self.RIDS)
        self.assertTrue(any("RuntimeIdentifiers" in issue for issue in validate_cli_rids(root)))

    def test_windows_arm64_must_be_a_tool_package(self):
        root = project(self.RIDS, self.RIDS.replace(";win-arm64", ""))
        self.assertTrue(any("ToolPackageRuntimeIdentifiers" in issue for issue in validate_cli_rids(root)))

    def test_duplicate_restore_identifier_is_rejected(self):
        root = project(self.RIDS + ";osx-arm64", self.RIDS)
        self.assertTrue(any("RuntimeIdentifiers" in issue for issue in validate_cli_rids(root)))


if __name__ == "__main__":
    unittest.main()
