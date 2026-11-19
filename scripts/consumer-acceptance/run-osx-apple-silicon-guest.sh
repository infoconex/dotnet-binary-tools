#!/bin/bash
# Run this once per attempt in the FRESH Tart macOS guest, not on the host.
# Inputs are shared read-only; results are written to a separate host share.
set -euo pipefail
umask 077

if [ "$(uname -s)" != Darwin ] || [ "$(uname -m)" != arm64 ]; then
  echo 'ERROR: This command requires an Apple Silicon macOS guest.' >&2
  exit 1
fi

input='/Volumes/My Shared Files/acceptance-input'
results='/Volumes/My Shared Files/acceptance-results'
if [ ! -f "$input/osx-apple-silicon.sh" ] ||
   [ ! -f "$input/pre-release-osx-arm64.zip" ] ||
   [ ! -f "$input/pre-release-osx-x64.zip" ] ||
   [ ! -f "$input/host-provenance.txt" ] || [ ! -d "$results" ]; then
  echo 'ERROR: Required read-only input and writable results shares are not mounted.' >&2
  exit 1
fi

printf '\nThis is a clean-VM consumer test. Do not run it on an SDK-equipped macOS installation.\n'
printf 'Confirm that the guest was freshly installed from Apple macOS media, and you have not installed .NET SDK/shared runtime.\n'
printf 'Type YES to attest that this guest has not been used for development: '
read -r confirmation
if [ "$confirmation" != YES ]; then
  echo 'STOP: Fresh guest provenance was not confirmed. No application binary executed.' >&2
  exit 1
fi

attempt="$(date -u +%Y%m%dT%H%M%SZ)-$$"
name="evidence-mac-$attempt"
evidence="$results/$name"
provenance="$results/provenance-$attempt.txt"
{
  printf 'Operator attested YES: fresh Apple macOS guest, no .NET installed, no development use.\n'
  printf 'Attestation UTC: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  printf 'Host-provided VM creation evidence:\n'
  sed -n '1,80p' "$input/host-provenance.txt"
  printf 'Observed guest macOS:\n'
  sw_vers
  printf 'Observed guest architecture: %s\n' "$(uname -m)"
} > "$provenance"

set +e
bash "$input/osx-apple-silicon.sh" \
  "$input/pre-release-osx-arm64.zip" \
  "$input/pre-release-osx-x64.zip" \
  "$evidence" "$provenance"
test_exit=$?
set -e

printf '%s\n' "$test_exit" > "$results/guest-test-exit-code-$attempt.txt"
if [ -d "$evidence" ]; then
  summary_md="$evidence/test-summary.md"
  {
    printf '# Issue #4 — macOS clean-consumer test summary\n\n'
    printf -- '- Candidate source SHA: `6dffde08e6a9853c68555c1e751bcd63f082e0bd`\n'
    printf -- '- [Exact Native AOT artifact run](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433040)\n'
    printf -- '- Guest test exit code: `%s` (0 means all scripted checks passed)\n' "$test_exit"
    printf -- '- macOS ARM64: native execution, subject to separate evidence review\n'
    printf -- '- macOS x64: translated by Rosetta, **not** Intel-native acceptance\n'
    printf -- '- Windows x64 clean-consumer acceptance: pending\n'
    printf -- '- Physical Intel macOS x64 acceptance: deferred, not waived\n'
    printf -- '- .NET installation absence: supported by fresh-image attestation and `environment.txt`; review required\n\n'
    printf '## Scripted scenario outcomes\n\n````text\n'
    if [ -f "$evidence/summary.txt" ]; then
      sed -n '1,100p' "$evidence/summary.txt"
    else
      echo 'Script stopped before producing scenario summary; investigate the guest exit and raw logs.'
    fi
    printf '````\n\n'
    printf '## Evidence locations\n\n'
    printf -- '- `provenance.txt`, `environment.txt`, `context.txt`, `checksums.txt`\n'
    printf -- '- `cases/osx-arm64-*/` and `cases/osx-x64-*/` contain exact commands, raw stdout/stderr, UTC timestamps and exit codes\n'
    printf -- '- `rosetta-*` contains the translation probe output and exit code\n'
    printf -- '- Review output semantics before final acceptance; no release or merge authorization is implied\n'
  } > "$summary_md"
  tar -czf "$results/$name.tar.gz" -C "$results" "$name"
  shasum -a 256 "$results/$name.tar.gz" > "$results/$name.sha256.txt"
  printf 'Preserved guest logs: %s\n' "$evidence"
  printf 'Preserved evidence archive: %s\n' "$results/$name.tar.gz"
  printf 'Archive digest: '
  shasum -a 256 "$results/$name.tar.gz"
fi

printf 'Guest script exit code: %s\n' "$test_exit"
if [ "$test_exit" -ne 0 ]; then
  echo 'FAIL/PENDING: Preserve this attempt. Diagnose from logs; any retry creates another evidence archive.' >&2
  exit "$test_exit"
fi
echo 'All scripted cases passed. Independent owner review of raw reports is still required.'
