"""Safe preflight tests; these do not claim native macOS consumer execution."""

import os
import platform
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path


SCRIPT = Path(__file__).resolve().parents[1] / "consumer-acceptance" / "osx-apple-silicon.sh"
VM_SCRIPT = SCRIPT.with_name("provision-osx-apple-silicon-vm.sh")


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


@unittest.skipUnless(shutil.which("bash"), "system Bash unavailable")
class AppleSiliconVMProvisioningPreflightTests(unittest.TestCase):
    def test_missing_inputs_prints_usage_and_does_not_provision(self):
        result = subprocess.run(["bash", str(VM_SCRIPT)], capture_output=True, text=True, check=False)

        self.assertEqual(2, result.returncode)
        self.assertIn("Usage:", result.stderr)

    @unittest.skipUnless(platform.system() == "Linux", "requires a non-macOS host")
    def test_non_macos_host_is_rejected_before_provisioning(self):
        result = subprocess.run(
            ["bash", str(VM_SCRIPT), "arm.zip", "x64.zip", "new-output"],
            capture_output=True,
            text=True,
            check=False,
        )

        self.assertEqual(1, result.returncode)
        self.assertIn("Apple Silicon macOS host", result.stderr)

    @unittest.skipUnless(platform.system() == "Linux", "mocked macOS host only")
    def test_wrong_artifact_sha_blocks_vm_creation(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            fake_tools = root / "tools"
            fake_tools.mkdir()
            (fake_tools / "uname").write_text(
                "#!/bin/sh\nif [ \"$1\" = -s ]; then echo Darwin; else echo arm64; fi\n",
                encoding="utf-8",
            )
            (fake_tools / "tart").write_text(
                "#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$ISSUE4_TEST_TART_LOG\"\n",
                encoding="utf-8",
            )
            for fake_tool in (fake_tools / "uname", fake_tools / "tart"):
                fake_tool.chmod(0o755)
            (root / "arm.zip").write_bytes(b"wrong ARM artifact")
            (root / "x64.zip").write_bytes(b"wrong x64 artifact")
            calls = root / "tart-calls.txt"
            env = dict(os.environ)
            env["PATH"] = f"{fake_tools}{os.pathsep}{env.get('PATH', '')}"
            env["ISSUE4_TEST_TART_LOG"] = str(calls)

            result = subprocess.run(
                ["bash", str(VM_SCRIPT), str(root / "arm.zip"), str(root / "x64.zip"),
                 str(root / "new-output")],
                env=env,
                capture_output=True,
                text=True,
                check=False,
            )

            self.assertEqual(1, result.returncode)
            self.assertIn("SHA-256 mismatch", result.stderr)
            self.assertFalse(calls.exists(), "A failed hash must never create or launch a VM")
            self.assertFalse((root / "new-output").exists())

    @unittest.skipUnless(platform.system() == "Linux", "mocked macOS host only")
    def test_mocked_tart_stages_artifacts_and_preserves_vm_without_guest_result(self):
        """Unit-test orchestration only; fake digests never establish artifact integrity."""
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            fake_tools = root / "tools"
            fake_tools.mkdir()
            tools = {
                "uname": "#!/bin/sh\nif [ \"$1\" = -s ]; then echo Darwin; else echo arm64; fi\n",
                "sw_vers": "#!/bin/sh\necho 'ProductVersion: 26.0 (simulated)'\n",
                "tart": (
                    "#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$ISSUE4_TEST_TART_LOG\"\n"
                    "if [ \"$1\" = --version ]; then echo 'simulated-tart'; fi\n"
                ),
                "shasum": (
                    "#!/bin/sh\ncase \"$*\" in\n"
                    "  *arm.zip*) echo '085074157d2b5657ad866decb755b24eabf43f20d31554de2a69a2f47db16e45' ;;\n"
                    "  *x64.zip*) echo '591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f' ;;\n"
                    "  *) exit 1 ;;\n"
                    "esac\n"
                ),
            }
            for name, source in tools.items():
                tool = fake_tools / name
                tool.write_text(source, encoding="utf-8")
                tool.chmod(0o755)
            (root / "arm.zip").write_bytes(b"mock archive: no product bytes")
            (root / "x64.zip").write_bytes(b"mock archive: no product bytes")
            calls = root / "tart-calls.txt"
            env = dict(os.environ)
            env["PATH"] = f"{fake_tools}{os.pathsep}{env.get('PATH', '')}"
            env["ISSUE4_TEST_TART_LOG"] = str(calls)
            output = root / "new-output"

            result = subprocess.run(
                ["bash", str(VM_SCRIPT), str(root / "arm.zip"), str(root / "x64.zip"), str(output)],
                env=env,
                capture_output=True,
                text=True,
                check=False,
            )

            self.assertEqual(3, result.returncode, result.stderr)
            self.assertIn("PENDING: Guest test has not run", result.stderr)
            tart_calls = calls.read_text(encoding="utf-8")
            self.assertIn("create --from-ipsw=latest issue4-clean-", tart_calls)
            self.assertIn("--dir=acceptance-input:", tart_calls)
            self.assertIn(":ro --dir=acceptance-results:", tart_calls)
            self.assertTrue((output / "input" / "osx-apple-silicon.sh").is_file())
            self.assertTrue((output / "input" / "run-osx-apple-silicon-guest.sh").is_file())
            self.assertTrue((output / "host-provenance.txt").is_file())
            self.assertIn("tart run", (output / "resume-vm.sh").read_text(encoding="utf-8"))
            self.assertFalse(list((output / "results").iterdir()))


if __name__ == "__main__":
    unittest.main()
