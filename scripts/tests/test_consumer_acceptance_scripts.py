"""Safe preflight tests; these do not claim native macOS consumer execution."""

import platform
import shutil
import subprocess
import unittest
from pathlib import Path


SCRIPT = Path(__file__).resolve().parents[1] / "consumer-acceptance" / "osx-apple-silicon.sh"


@unittest.skipUnless(shutil.which("bash"), "system Bash unavailable")
class AppleSiliconOperatorScriptPreflightTests(unittest.TestCase):
    def test_missing_required_inputs_returns_usage_error(self):
        result = subprocess.run(["bash", str(SCRIPT)], capture_output=True, text=True, check=False)

        self.assertEqual(2, result.returncode)
        self.assertIn("Usage:", result.stderr)
        self.assertIn("osx-apple-silicon.sh", result.stderr)

    @unittest.skipUnless(platform.system() == "Linux", "requires a non-macOS host")
    def test_linux_is_rejected_before_accessing_artifacts_or_executables(self):
        result = subprocess.run(
            ["bash", str(SCRIPT), "not-present-arm.zip", "not-present-x64.zip", "evidence", "note"],
            capture_output=True,
            text=True,
            check=False,
        )

        self.assertEqual(1, result.returncode)
        self.assertIn("requires a real macOS", result.stderr)


if __name__ == "__main__":
    unittest.main()
