"""Focused tests for CI path classification; no external Python dependencies."""
from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from ci_change_scope import classify  # noqa: E402


class ChangeScopeTests(unittest.TestCase):
    def test_readme_only_runs_documentation_and_lightweight_checks(self):
        result = classify(["README.md"])
        self.assertTrue(result["documentation"])
        self.assertTrue(result["line_endings"])
        self.assertFalse(result["managed"])
        self.assertFalse(result["aot"])
        self.assertFalse(result["architecture"])

    def test_scenario_docs_do_not_trigger_dotnet_build(self):
        result = classify(["docs/scenarios/functional/assembly-inspection/FR-ASM-001.md"])
        self.assertTrue(result["documentation"])
        self.assertTrue(result["test_metadata"])
        self.assertFalse(result["managed"])
        self.assertFalse(result["aot"])

    def test_source_change_triggers_managed_aot_and_architecture(self):
        result = classify(["src/Infoconex.DotNetBinary.Domain/Models/TargetFrameworkIdentity.cs"])
        self.assertFalse(result["documentation"])
        self.assertTrue(result["managed"])
        self.assertTrue(result["aot"])
        self.assertTrue(result["architecture"])

    def test_test_change_does_not_require_native_aot(self):
        result = classify(["tests/Infoconex.DotNetBinary.Domain.Tests/TargetFrameworkIdentityTests.cs"])
        self.assertTrue(result["managed"])
        self.assertTrue(result["test_metadata"])
        self.assertFalse(result["aot"])
        self.assertFalse(result["documentation"])

    def test_central_nuget_change_requires_both_builds(self):
        result = classify(["Directory.Packages.props"])
        self.assertTrue(result["managed"])
        self.assertTrue(result["aot"])
        self.assertTrue(result["architecture"])

    def test_coverage_script_and_settings_trigger_managed_validation(self):
        for changed_file in (
            "scripts/collect-coverage.ps1",
            "scripts/coverage_report.py",
            "scripts/tests/test_coverage_report.py",
            "coverage.runsettings",
        ):
            with self.subTest(changed_file=changed_file):
                result = classify([changed_file])
                self.assertTrue(result["managed"])
                self.assertFalse(result["aot"])

    def test_combined_docs_and_source_runs_both(self):
        result = classify(["docs/README.md", "src/Infoconex.DotNetBinary.Cli/Program.cs"])
        self.assertTrue(result["documentation"])
        self.assertTrue(result["managed"])
        self.assertTrue(result["aot"])

    def test_workflow_implementations_are_independently_scoped(self):
        result = classify([".github/workflows/native-aot-smoke.yml"])
        self.assertTrue(result["aot"])
        self.assertFalse(result["managed"])
        self.assertFalse(result["documentation"])

    def test_change_detection_script_change_runs_all(self):
        result = classify(["scripts/ci_change_scope.py"])
        self.assertTrue(all(result.values()))

    def test_documentation_validator_change_checks_docs(self):
        result = classify(["scripts/validate_docs.py"])
        self.assertTrue(result["documentation"])
        self.assertTrue(result["test_metadata"])
        self.assertFalse(result["managed"])


if __name__ == "__main__":
    unittest.main()
