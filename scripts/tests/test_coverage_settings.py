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


if __name__ == "__main__":
    unittest.main()
