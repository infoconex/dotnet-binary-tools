#!/bin/bash
# Issue #4: provision a fresh macOS ARM64 consumer using Tart/Apple Virtualization.
# First-boot Setup Assistant and one guest command are intentionally interactive.
# Never disables macOS security, configures SSH or removes an existing VM.
set -euo pipefail
umask 077

if [ "$#" -ne 3 ]; then
  echo 'Usage: bash provision-osx-apple-silicon-vm.sh <osx-arm64 workflow ZIP> <osx-x64 workflow ZIP> <new output directory>' >&2
  exit 2
fi

arm_artifact=$1
x64_artifact=$2
output=$3
script_dir=$(cd "$(dirname "$0")" && pwd -P)
source_sha=6dffde08e6a9853c68555c1e751bcd63f082e0bd
expected_arm=085074157d2b5657ad866decb755b24eabf43f20d31554de2a69a2f47db16e45
expected_x64=591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f

if [ "$(uname -s)" != Darwin ] || [ "$(uname -m)" != arm64 ]; then
  echo 'ERROR: This provisioner requires a real Apple Silicon macOS host.' >&2
  exit 1
fi
if ! command -v tart >/dev/null 2>&1; then
  echo 'ERROR: Install Tart on the HOST first (official guidance: https://tart.run/quick-start/).' >&2
  echo 'Homebrew installation: brew install openai/tools/tart' >&2
  exit 1
fi
if ! command -v shasum >/dev/null 2>&1; then
  echo 'ERROR: macOS system shasum is required.' >&2
  exit 1
fi
if [ ! -f "$arm_artifact" ] || [ ! -f "$x64_artifact" ] ||
   [ ! -f "$script_dir/osx-apple-silicon.sh" ] ||
   [ ! -f "$script_dir/run-osx-apple-silicon-guest.sh" ]; then
  echo 'ERROR: Both original workflow ZIPs and both scripts must exist.' >&2
  exit 1
fi
if [ -e "$output" ] || [ -L "$output" ]; then
  echo 'ERROR: Refusing to overwrite an existing output directory.' >&2
  exit 1
fi
case "$output" in
  *:*) echo 'ERROR: Output path cannot contain a colon (Tart directory mount delimiter).' >&2; exit 1 ;;
esac

# Verify the ORIGINAL GitHub Actions ZIP bytes before provisioning anything.
# Do not use `gh run download`: that command unpacks the original workflow ZIP.
actual_arm=$(shasum -a 256 "$arm_artifact" | awk '{print $1}')
actual_x64=$(shasum -a 256 "$x64_artifact" | awk '{print $1}')
if [ "$actual_arm" != "$expected_arm" ] || [ "$actual_x64" != "$expected_x64" ]; then
  printf 'ERROR: SHA-256 mismatch; no VM was created.\n' >&2
  printf 'osx-arm64 expected=%s actual=%s\n' "$expected_arm" "$actual_arm" >&2
  printf 'osx-x64 expected=%s actual=%s\n' "$expected_x64" "$actual_x64" >&2
  exit 1
fi

mkdir -p "$output/input" "$output/results"
output=$(cd "$output" && pwd -P)
input="$output/input"
results="$output/results"
cp "$arm_artifact" "$input/pre-release-osx-arm64.zip"
cp "$x64_artifact" "$input/pre-release-osx-x64.zip"
cp "$script_dir/osx-apple-silicon.sh" "$input/osx-apple-silicon.sh"
cp "$script_dir/run-osx-apple-silicon-guest.sh" "$input/run-osx-apple-silicon-guest.sh"

# A unique name prevents accidental reuse of an SDK-equipped developer image.
vm_name="issue4-clean-$(date -u +%Y%m%dT%H%M%SZ)-$$"
{
  printf 'Issue #4 supplemental, unreleased Native AOT consumer preparation\n'
  printf 'Candidate source SHA: %s\n' "$source_sha"
  printf 'UTC VM creation requested: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  printf 'VM name: %s\n' "$vm_name"
  printf 'macOS installer: Apple IPSW via tart create --from-ipsw=latest\n'
  printf 'Virtualizer: Tart on Apple Virtualization.framework\n'
  printf 'Tart version: '
  tart --version
  printf 'Host OS (NOT the consumer):\n'
  sw_vers
  printf 'Host architecture: %s\n' "$(uname -m)"
  printf 'osx-arm64 original workflow ZIP SHA-256: %s\n' "$actual_arm"
  printf 'osx-x64 original workflow ZIP SHA-256: %s\n' "$actual_x64"
  echo 'The host is NOT the test consumer. Guest must supply separate clean-image attestation.'
} > "$output/host-provenance.txt" 2>&1
cp "$output/host-provenance.txt" "$input/host-provenance.txt"

# Reopen the SAME VM for diagnosis/retesting; never recreate it or erase logs.
{
  printf '#!/bin/bash\nset -euo pipefail\n'
  printf 'tart run %q %q %q\n' \
    "--dir=acceptance-input:$input:ro" "--dir=acceptance-results:$results" "$vm_name"
} > "$output/resume-vm.sh"

printf '\nCreating a new Apple-provided macOS VM: %s\n' "$vm_name"
printf 'The IPSW download and installation may take a while.\n'
set +e
tart create --from-ipsw=latest "$vm_name" 2>&1 | tee "$output/tart-create.log"
create_exit=${PIPESTATUS[0]}
set -e
printf '%s\n' "$create_exit" > "$output/tart-create-exit-code.txt"
if [ "$create_exit" -ne 0 ]; then
  printf 'ERROR: Tart VM creation failed (exit %s). Review %s. No cleanup was attempted.\n' \
    "$create_exit" "$output/tart-create.log" >&2
  exit 1
fi

printf '\nFIRST BOOT — one-time manual steps inside the new VM:\n'
printf '  1. Complete Apple macOS Setup Assistant. Use a disposable local account; do not sign in with a personal Apple ID.\n'
printf '  2. Do not install Xcode, the .NET SDK, the .NET shared runtime, or any developer tooling.\n'
printf '  3. Open Terminal and run this exact command (guest shares auto-mount):\n\n'
printf '     bash "/Volumes/My Shared Files/acceptance-input/run-osx-apple-silicon-guest.sh"\n\n'
printf '  4. Review results, then shut down the guest normally from the Apple menu.\n'
printf 'The script does not change SSH, Gatekeeper, automatic login, or sudo policy.\n\n'

set +e
tart run "--dir=acceptance-input:$input:ro" "--dir=acceptance-results:$results" "$vm_name" 2>&1 | tee "$output/tart-run.log"
run_exit=${PIPESTATUS[0]}
set -e
printf '%s\n' "$run_exit" > "$output/tart-run-exit-code.txt"

printf '\nVM data preserved; nothing deleted. VM name: %s\n' "$vm_name"
printf 'Evidence directory: %s\n' "$results"
printf 'To reopen the same VM: bash %s/resume-vm.sh\n' "$output"
found=false
guest_failures=0
for report in "$results"/guest-test-exit-code-*.txt; do
  if [ -f "$report" ]; then
    found=true
    guest_exit=$(tr -d '\r\n' < "$report")
    printf '%s: %s\n' "$(basename "$report")" "$guest_exit"
    if [ "$guest_exit" != 0 ]; then
      guest_failures=$((guest_failures + 1))
    fi
  fi
done
for archive in "$results"/evidence-mac-*.tar.gz; do
  if [ -f "$archive" ]; then
    printf 'SHA-256 evidence archive: '
    shasum -a 256 "$archive"
  fi
done
if [ "$run_exit" -ne 0 ]; then
  echo 'ERROR: Tart run failed. Preserve VM and logs for diagnosis.' >&2
  exit 1
fi
if [ "$found" != true ]; then
  echo 'PENDING: Guest test has not run; reopen the same VM and run the guest command.' >&2
  exit 3
fi
if [ "$guest_failures" -ne 0 ]; then
  printf 'FAIL: %s recorded guest attempts did not pass. Review all preserved evidence.\n' "$guest_failures" >&2
  exit 1
fi
echo 'Guest execution recorded. Review every attempt and its raw logs before claiming a pass.'
