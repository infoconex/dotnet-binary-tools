"""Regression tests for the shared Microsoft coverage collection settings."""
from __future__ import annotations

import re
import unittest
from pathlib import Path
from xml.etree import ElementTree


ROOT = Path(__file__).resolve().parents[2]


class CoverageSettingsTests(unittest.TestCase):
    def test_includes_every_production_output_assembly(self):
        config = ElementTree.parse(ROOT / "coverage.runsettings")
        pattern = config.findtext(".//ModulePaths/Include/ModulePath")
        self.assertIsNotNone(pattern)

        for project in (ROOT / "src").glob("*/*.csproj"):
            with self.subTest(project=project.name):
                definition = ElementTree.parse(project)
                assembly = definition.findtext(".//AssemblyName") or project.stem
                self.assertRegex(f"{assembly}.dll", re.compile(pattern, re.IGNORECASE))

    def test_excludes_test_assemblies(self):
        config = ElementTree.parse(ROOT / "coverage.runsettings")
        self.assertEqual("false", config.findtext(".//IncludeTestAssembly").lower())

    def test_process_contract_tests_do_not_mix_child_process_counters(self):
        config = ElementTree.parse(ROOT / "coverage.runsettings")
        setting = config.findtext(".//CollectFromChildProcesses")
        self.assertIsNotNone(setting, "Child-process collection must be explicit.")
        self.assertEqual("false", setting.lower())

    def test_coverage_aggregation_uses_one_test_host_at_a_time(self):
        config = ElementTree.parse(ROOT / "coverage.runsettings")
        self.assertEqual("1", config.findtext(".//RunConfiguration/MaxCpuCount"))


if __name__ == "__main__":
    unittest.main()
