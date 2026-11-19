#!/bin/bash
# Issue #4: SDK/runtime-free Apple Silicon physical consumer operator entry point.
# The x64 test runs through Rosetta; it does not qualify as Intel-native acceptance.
set -euo pipefail
umask 077

if [ "$#" -ne 3 ]; then
  echo 'Usage: bash run-osx-apple-silicon-physical.sh <original osx-arm64 workflow ZIP> <original osx-x64 workflow ZIP> <new results directory>' >&2
  exit 2
fi

arm_artifact=$1
x64_artifact=$2
results=$3
script_dir=$(cd "$(dirname "$0")" && pwd -P)
expected_arm=085074157d2b5657ad866decb755b24eabf43f20d31554de2a69a2f47db16e45
expected_x64=591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f

if [ "$(uname -s)" != Darwin ] || [ "$(uname -m)" != arm64 ]; then
  echo 'ERROR: Requires a genuine Apple Silicon macOS consumer.' >&2
  exit 1
fi
if [ ! -f "$arm_artifact" ] || [ ! -f "$x64_artifact" ] ||
   [ ! -f "$script_dir/osx-apple-silicon.sh" ] || [ -e "$results" ] || [ -L "$results" ]; then
  echo 'ERROR: Both original artifact ZIPs and test script must exist; results directory must not exist.' >&2
  exit 1
fi

# Check immutable downloaded ZIPs before asking for attestation or executing anything.
actual_arm=$(shasum -a 256 "$arm_artifact" | awk '{print $1}')
actual_x64=$(shasum -a 256 "$x64_artifact" | awk '{print $1}')
if [ "$actual_arm" != "$expected_arm" ] || [ "$actual_x64" != "$expected_x64" ]; then
  echo 'FAIL: Workflow ZIP SHA-256 mismatch; no product executable was run.' >&2
  exit 1
fi

echo 'Confirm: this physical Mac has never had a .NET SDK or shared runtime installed.'
echo 'The script will independently check common paths, package receipts and PATH.'
printf 'Type YES to attest this installation history and proceed: '
read -r attestation
if [ "$attestation" != YES ]; then
  echo 'STOP: Installation-history attestation not provided. No product executable was run.' >&2
  exit 1
fi

mkdir -p "$results"
results=$(cd "$results" && pwd -P)
{
  echo 'Consumer type: physical Apple Silicon macOS; NOT a VM.'
  echo 'Operator attested YES: no .NET SDK/shared runtime has ever been installed.'
  printf 'Attestation UTC: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  echo 'OS:'
  sw_vers
  printf 'Hardware architecture: %s\n' "$(uname -m)"
  echo 'Initial inventory provided separately by owner: no dotnet on PATH, no standard installation paths, receipts or Homebrew packages.'
} > "$results/provenance.txt"

set +e
bash "$script_dir/osx-apple-silicon.sh" "$arm_artifact" "$x64_artifact" \
  "$results/evidence" "$results/provenance.txt" 2>&1 | tee "$results/operator-run.log"
test_exit=${PIPESTATUS[0]}
set -e
printf '%s\n' "$test_exit" > "$results/test-exit-code.txt"

if [ -d "$results/evidence" ]; then
  {
    printf '# Issue #4 — physical Mac mini ARM64 consumer test\n\n'
    printf -- '- Immutable candidate: `6dffde08e6a9853c68555c1e751bcd63f082e0bd`\n'
    printf -- '- Original workflow ZIP hashes: matched on this Mac before execution\n'
    printf -- '- Physical ARM64 execution: test script exit `%s` (review scenario results below)\n' "$test_exit"
    printf -- '- x64 execution: Rosetta translated; physical Intel acceptance remains deferred\n'
    printf -- '- Windows x64 consumer acceptance: pending\n'
    printf -- '- Runtime-free claim: requires review of `provenance.txt` and `environment.txt`\n\n'
    printf '## Raw scenario assessment\n\n````text\n'
    if [ -f "$results/evidence/summary.txt" ]; then
      sed -n '1,100p' "$results/evidence/summary.txt"
    else
      echo 'Script stopped before executing scenarios; inspect environment/checksum logs.'
    fi
    printf '````\n\n'
    printf '## Log inventory\n\n'
    printf -- '- `cases/osx-arm64-*` and `cases/osx-x64-*`: command vectors, UTC timestamps, raw stdout/stderr and exit codes\n'
    printf -- '- `checksums.txt`, `environment.txt`, `context.txt`, `provenance.txt`\n'
    printf -- '- `rosetta-*`: translated-process probe evidence\n'
  } > "$results/evidence/test-summary.md"
  tar -czf "$results/evidence-mac-physical.tar.gz" -C "$results" evidence
  shasum -a 256 "$results/evidence-mac-physical.tar.gz" > "$results/evidence-mac-physical.sha256.txt"
  printf 'Evidence archive: %s\n' "$results/evidence-mac-physical.tar.gz"
  printf 'Archive SHA-256: '
  shasum -a 256 "$results/evidence-mac-physical.tar.gz"
fi

if [ "$test_exit" -ne 0 ]; then
  echo 'FAIL: Preserve the evidence; any retry must use a NEW results directory.' >&2
  exit "$test_exit"
fi
echo 'PASS: Scripted cases passed. Raw outputs require independent review before acceptance.'
