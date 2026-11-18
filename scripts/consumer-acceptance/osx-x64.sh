#!/bin/bash
# Issue #4: run only on a genuinely clean Intel macOS consumer.
# Requires macOS system tools; does not install .NET or execute inspected inputs.
set -euo pipefail
umask 077

if [ "$#" -ne 3 ]; then
  echo 'Usage: bash osx-x64.sh <pre-release-osx-x64 workflow ZIP> <new evidence directory> <provenance note file>' >&2
  exit 2
fi

artifact=$1
evidence=$2
provenance=$3
source_sha=6dffde08e6a9853c68555c1e751bcd63f082e0bd
expected_artifact_sha=591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f
expected_standalone_sha=1126739ffb5c75accd01de2827e2555c20b59ef1c57ede11c84fb065ea140f49
expected_binary_sha=330fb27234b739a8571f52bd79239573469642c89e8a0ac570199e88c08c3f9a
archive_name=dotnet-binary-0.1.0-preview.1-osx-x64.tar.gz

if [ "$(uname -s)" != Darwin ]; then
  echo 'ERROR: This acceptance script requires actual macOS, not Linux or an emulator.' >&2
  exit 1
fi
if [ ! -f "$artifact" ] || [ ! -f "$provenance" ] || [ -e "$evidence" ]; then
  echo 'ERROR: Artifact and provenance note must exist; evidence path must not exist.' >&2
  exit 1
fi

mkdir -p "$evidence"
evidence=$(cd "$evidence" && pwd -P)
artifact=$(cd "$(dirname "$artifact")" && pwd -P)/$(basename "$artifact")
cp "$provenance" "$evidence/provenance.txt"
printf 'Source SHA: %s\nArtifact: %s\nUTC start: %s\n' \
  "$source_sha" "$artifact" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > "$evidence/context.txt"

clean=true
{
  echo 'Requirement: physical Intel macOS x64, with no modern .NET SDK/shared runtime.'
  sw_vers
  uname -a
  printf 'CPU: '
  sysctl -n machdep.cpu.brand_string || true
  printf 'Hardware model: '
  sysctl -n hw.model || true
  printf 'uname -m: %s\n' "$(uname -m)"
  printf 'dotnet on PATH: '
  command -v dotnet || echo '(absent)'
  echo 'Standard installation paths:'
  for dir in /usr/local/share/dotnet /usr/share/dotnet /opt/dotnet /opt/homebrew/share/dotnet "$HOME/.dotnet"; do
    if [ -e "$dir" ]; then
      printf 'PRESENT: %s\n' "$dir"
      clean=false
    else
      printf 'absent: %s\n' "$dir"
    fi
  done
  echo 'Installed .NET package receipts:'
  if pkgutil --pkgs | grep -Ei '(dotnet|netcore)' ; then
    clean=false
  else
    echo '(none detected)'
  fi
  if command -v dotnet >/dev/null 2>&1; then clean=false; fi
  if [ "$(uname -m)" != x86_64 ]; then clean=false; fi
  if ! sysctl -n machdep.cpu.brand_string 2>/dev/null | grep -qi intel; then clean=false; fi
  printf 'Clean-host inventory result: %s\n' "$clean"
} > "$evidence/environment.txt" 2>&1

if [ "$clean" != true ]; then
  echo 'FAIL: Host is not demonstrated to be a clean physical Intel macOS x64 consumer.' | tee "$evidence/summary.txt" >&2
  exit 1
fi

actual_artifact_sha=$(shasum -a 256 "$artifact" | awk '{print $1}')
printf 'workflow ZIP expected: %s\nworkflow ZIP actual:   %s\n' \
  "$expected_artifact_sha" "$actual_artifact_sha" > "$evidence/checksums.txt"
if [ "$actual_artifact_sha" != "$expected_artifact_sha" ]; then
  echo 'FAIL: Workflow ZIP checksum mismatch. No executable was run.' | tee "$evidence/summary.txt" >&2
  exit 1
fi

mkdir -p "$evidence/work" "$evidence/cases"
unzip -q "$artifact" -d "$evidence/work"
(
  cd "$evidence/work/aot/osx-x64"
  shasum -a 256 -c SHA256SUMS.txt
) >> "$evidence/checksums.txt" 2>&1
standalone="$evidence/work/distribution/standalone/$archive_name"
actual_standalone_sha=$(shasum -a 256 "$standalone" | awk '{print $1}')
printf 'standalone expected: %s\nstandalone actual:   %s\n' \
  "$expected_standalone_sha" "$actual_standalone_sha" >> "$evidence/checksums.txt"
if [ "$actual_standalone_sha" != "$expected_standalone_sha" ]; then
  echo 'FAIL: Standalone archive checksum mismatch. No executable was run.' | tee "$evidence/summary.txt" >&2
  exit 1
fi

mkdir -p "$evidence/work/standalone" "$evidence/work/fixtures"
tar -xzf "$standalone" -C "$evidence/work/standalone"
binary="$evidence/work/standalone/dotnet-binary"
actual_binary_sha=$(shasum -a 256 "$binary" | awk '{print $1}')
printf 'executable expected: %s\nexecutable actual:   %s\n' \
  "$expected_binary_sha" "$actual_binary_sha" >> "$evidence/checksums.txt"
if [ "$actual_binary_sha" != "$expected_binary_sha" ] || [ ! -x "$binary" ]; then
  echo 'FAIL: Executable hash or executable mode mismatch. No executable was run.' | tee "$evidence/summary.txt" >&2
  exit 1
fi

# Intentionally inert; inspect and scan must never execute this fixture.
printf 'Inert, deliberately non-PE test fixture for static inspection.\n' \
  > "$evidence/work/fixtures/synthetic.exe"

failures=0
printf 'Issue #4 osx-x64 consumer scenarios, UTC %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > "$evidence/summary.txt"
run_case() {
  case_name=$1
  expected_exit=$2
  shift 2
  case_dir="$evidence/cases/$case_name"
  mkdir -p "$case_dir"
  {
    printf 'Executable and exact argument vector: '
    printf '%q ' "$@"
    printf '\nUTC start: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
  } > "$case_dir/command.txt"
  set +e
  "$@" > "$case_dir/stdout.txt" 2> "$case_dir/stderr.txt"
  actual_exit=$?
  set -e
  printf '%s\n' "$actual_exit" > "$case_dir/exit-code.txt"
  printf 'UTC end: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$case_dir/command.txt"
  if [ "$actual_exit" -eq "$expected_exit" ]; then
    printf 'PASS %-20s expected=%s actual=%s\n' "$case_name" "$expected_exit" "$actual_exit" >> "$evidence/summary.txt"
  else
    printf 'FAIL %-20s expected=%s actual=%s\n' "$case_name" "$expected_exit" "$actual_exit" >> "$evidence/summary.txt"
    failures=$((failures + 1))
  fi
}

run_case help 0 "$binary" --help
run_case version 0 "$binary" --version
run_case inspect 0 "$binary" inspect "$evidence/work/fixtures/synthetic.exe" --format json --sha256
run_case scan 0 "$binary" scan "$evidence/work/fixtures" --format json --max-candidates 1
run_case invalid-arity 2 "$binary" inspect
if ! grep -Fq "0.1.0-preview.1+$source_sha" "$evidence/cases/version/stdout.txt"; then
  printf 'FAIL version string differs from the frozen source SHA\n' >> "$evidence/summary.txt"
  failures=$((failures + 1))
fi
if ! grep -Eiq 'usage|commands|options' "$evidence/cases/help/stdout.txt"; then
  printf 'FAIL help output lacks a recognizable usage/commands description\n' >> "$evidence/summary.txt"
  failures=$((failures + 1))
fi
for case_name in inspect scan; do
  if ! grep -Eq '"schemaVersion"[[:space:]]*:[[:space:]]*"1\.0\.0"' "$evidence/cases/$case_name/stdout.txt" ||
     ! grep -Eq '"command"[[:space:]]*:[[:space:]]*"'"$case_name"'"' "$evidence/cases/$case_name/stdout.txt"; then
    printf 'FAIL %s output lacks expected v1 JSON report fields\n' "$case_name" >> "$evidence/summary.txt"
    failures=$((failures + 1))
  fi
done
if [ -s "$evidence/cases/invalid-arity/stdout.txt" ] ||
   ! grep -iq 'exactly one' "$evidence/cases/invalid-arity/stderr.txt"; then
  printf 'FAIL invalid-arity output does not match the CLI contract\n' >> "$evidence/summary.txt"
  failures=$((failures + 1))
fi
printf 'UTC finish: %s\nFailed checks: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$failures" >> "$evidence/summary.txt"
if [ "$failures" -ne 0 ]; then
  echo "FAIL: $failures scenario checks did not pass; inspect $evidence" >&2
  exit 1
fi
echo "PASS: All macOS x64 cases completed; preserve the directory: $evidence"
