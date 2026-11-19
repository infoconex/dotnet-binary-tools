#!/bin/bash
# Supplemental Issue #4 testing on a clean Apple Silicon macOS consumer.
# The x64 cases use Rosetta and DO NOT qualify as native Intel x64 acceptance.
# No .NET SDK or shared runtime is installed, hidden or removed by this script.
set -euo pipefail
umask 077

if [ "$#" -ne 4 ]; then
  echo 'Usage: bash osx-apple-silicon.sh <pre-release-osx-arm64.zip> <pre-release-osx-x64.zip> <new evidence directory> <provenance note>' >&2
  exit 2
fi

arm_artifact=$1
x64_artifact=$2
evidence=$3
provenance=$4
source_sha=6dffde08e6a9853c68555c1e751bcd63f082e0bd
expected_arm_zip=085074157d2b5657ad866decb755b24eabf43f20d31554de2a69a2f47db16e45
expected_x64_zip=591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f
expected_x64_standalone=1126739ffb5c75accd01de2827e2555c20b59ef1c57ede11c84fb065ea140f49
expected_x64_binary=330fb27234b739a8571f52bd79239573469642c89e8a0ac570199e88c08c3f9a

if [ "$(uname -s)" != Darwin ]; then
  echo 'ERROR: This script requires a real macOS Apple Silicon consumer or macOS guest.' >&2
  exit 1
fi
if [ ! -f "$arm_artifact" ] || [ ! -f "$x64_artifact" ] ||
   [ ! -f "$provenance" ] || [ -e "$evidence" ]; then
  echo 'ERROR: Both artifact ZIPs and provenance note must exist; evidence path must not exist.' >&2
  exit 1
fi

mkdir -p "$evidence"
evidence=$(cd "$evidence" && pwd -P)
arm_artifact=$(cd "$(dirname "$arm_artifact")" && pwd -P)/$(basename "$arm_artifact")
x64_artifact=$(cd "$(dirname "$x64_artifact")" && pwd -P)/$(basename "$x64_artifact")
cp "$provenance" "$evidence/provenance.txt"
printf 'Issue #4 supplemental Apple Silicon test (not Intel-native acceptance)\nSource SHA: %s\nUTC start: %s\nARM64 workflow ZIP: %s\nx64 workflow ZIP: %s\n' \
  "$source_sha" "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$arm_artifact" "$x64_artifact" > "$evidence/context.txt"

clean=true
{
  echo 'Requirement: freshly provisioned Apple Silicon macOS guest with NO .NET SDK/shared runtime.'
  echo 'Note: x64 executable will run under Rosetta; this is not physical Intel evidence.'
  sw_vers
  uname -a
  printf 'uname -m: %s\n' "$(uname -m)"
  printf 'Hardware model: '
  sysctl -n hw.model || true
  printf 'Apple Silicon support (hw.optional.arm64): '
  sysctl -n hw.optional.arm64 || true
  printf 'dotnet on PATH: '
  command -v dotnet || echo '(absent)'
  echo 'Standard .NET installation paths:'
  for dir in /usr/local/share/dotnet /usr/share/dotnet /opt/dotnet /opt/homebrew/share/dotnet "$HOME/.dotnet"; do
    if [ -e "$dir" ] || [ -L "$dir" ]; then
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
  if [ "$(uname -m)" != arm64 ]; then clean=false; fi
  if [ "$(sysctl -n hw.optional.arm64 2>/dev/null || true)" != 1 ]; then clean=false; fi
  printf 'Clean Apple Silicon guest inventory result: %s\n' "$clean"
} > "$evidence/environment.txt" 2>&1

if [ "$clean" != true ]; then
  echo 'FAIL: A clean Apple Silicon consumer has not been demonstrated.' | tee "$evidence/summary.txt" >&2
  exit 1
fi

mkdir -p "$evidence/work" "$evidence/cases"
printf 'Pinned workflow ZIP SHA-256 verification:\n' > "$evidence/checksums.txt"
verify_and_extract() {
  rid=$1
  artifact=$2
  expected_zip=$3
  actual_zip=$(shasum -a 256 "$artifact" | awk '{print $1}')
  printf '%s workflow ZIP expected: %s\n%s workflow ZIP actual:   %s\n' \
    "$rid" "$expected_zip" "$rid" "$actual_zip" >> "$evidence/checksums.txt"
  if [ "$actual_zip" != "$expected_zip" ]; then
    echo "FAIL: $rid ZIP checksum mismatch. No product executable was run." | tee "$evidence/summary.txt" >&2
    exit 1
  fi
  mkdir -p "$evidence/work/$rid"
  unzip -q "$artifact" -d "$evidence/work/$rid"
  (
    cd "$evidence/work/$rid/aot/$rid"
    shasum -a 256 -c SHA256SUMS.txt
  ) >> "$evidence/checksums.txt" 2>&1

  standalone="$evidence/work/$rid/distribution/standalone/dotnet-binary-0.1.0-preview.1-$rid.tar.gz"
  printf '%s standalone archive: ' "$rid" >> "$evidence/checksums.txt"
  shasum -a 256 "$standalone" >> "$evidence/checksums.txt"
  if [ "$rid" = osx-x64 ]; then
    actual_standalone=$(shasum -a 256 "$standalone" | awk '{print $1}')
    if [ "$actual_standalone" != "$expected_x64_standalone" ]; then
      echo 'FAIL: osx-x64 standalone archive checksum mismatch.' | tee "$evidence/summary.txt" >&2
      exit 1
    fi
  fi
  mkdir -p "$evidence/work/$rid/standalone"
  tar -xzf "$standalone" -C "$evidence/work/$rid/standalone"
  binary="$evidence/work/$rid/standalone/dotnet-binary"
  printf '%s extracted executable: ' "$rid" >> "$evidence/checksums.txt"
  shasum -a 256 "$binary" >> "$evidence/checksums.txt"
  if [ ! -x "$binary" ]; then
    echo "FAIL: $rid product binary is not executable." | tee "$evidence/summary.txt" >&2
    exit 1
  fi
  if [ "$rid" = osx-x64 ]; then
    actual_binary=$(shasum -a 256 "$binary" | awk '{print $1}')
    printf 'osx-x64 executable expected: %s\nosx-x64 executable actual:   %s\n' \
      "$expected_x64_binary" "$actual_binary" >> "$evidence/checksums.txt"
    if [ "$actual_binary" != "$expected_x64_binary" ]; then
      echo 'FAIL: osx-x64 extracted executable checksum mismatch.' | tee "$evidence/summary.txt" >&2
      exit 1
    fi
  fi
}

verify_and_extract osx-arm64 "$arm_artifact" "$expected_arm_zip"
verify_and_extract osx-x64 "$x64_artifact" "$expected_x64_zip"

mkdir -p "$evidence/work/fixtures"
printf 'Inert, deliberately non-PE test fixture for static inspection.\n' \
  > "$evidence/work/fixtures/synthetic.exe"

# Record whether a translated x86_64 process can start in this guest.
# The probe is not a substitute for running the actual product executable.
printf '%s\nUTC start: %s\n' \
  "arch -x86_64 /bin/sh -c 'sysctl -n sysctl.proc_translated'" \
  "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > "$evidence/rosetta-command.txt"
set +e
arch -x86_64 /bin/sh -c 'sysctl -n sysctl.proc_translated' \
  > "$evidence/rosetta-stdout.txt" 2> "$evidence/rosetta-stderr.txt"
rosetta_exit=$?
set -e
printf '%s\n' "$rosetta_exit" > "$evidence/rosetta-exit-code.txt"
printf 'UTC end: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> "$evidence/rosetta-command.txt"

failures=0
printf 'Issue #4 supplemental macOS ARM64 and Rosetta x64 cases (NOT Intel-native), UTC %s\n' \
  "$(date -u +%Y-%m-%dT%H:%M:%SZ)" > "$evidence/summary.txt"
if [ "$rosetta_exit" -ne 0 ] || [ "$(tr -d '\r\n' < "$evidence/rosetta-stdout.txt")" != 1 ]; then
  echo 'FAIL: Rosetta translation probe did not report translated process=1.' >> "$evidence/summary.txt"
  failures=$((failures + 1))
else
  echo 'PASS: Rosetta translation probe reported translated process=1.' >> "$evidence/summary.txt"
fi

run_case() {
  case_name=$1
  expected_exit=$2
  shift 2
  case_dir="$evidence/cases/$case_name"
  mkdir -p "$case_dir"
  {
    printf 'Exact argument vector: '
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
    printf 'PASS %-25s expected=%s actual=%s\n' "$case_name" "$expected_exit" "$actual_exit" >> "$evidence/summary.txt"
  else
    printf 'FAIL %-25s expected=%s actual=%s\n' "$case_name" "$expected_exit" "$actual_exit" >> "$evidence/summary.txt"
    failures=$((failures + 1))
  fi
}

for rid in osx-arm64 osx-x64; do
  binary="$evidence/work/$rid/standalone/dotnet-binary"
  case "$rid" in
    osx-arm64) cpu=arm64 ;;
    osx-x64) cpu=x86_64 ;;
  esac
  run_case "$rid-help" 0 arch -"$cpu" "$binary" --help
  run_case "$rid-version" 0 arch -"$cpu" "$binary" --version
  run_case "$rid-inspect" 0 arch -"$cpu" "$binary" inspect "$evidence/work/fixtures/synthetic.exe" --format json --sha256
  run_case "$rid-scan" 0 arch -"$cpu" "$binary" scan "$evidence/work/fixtures" --format json --max-candidates 1
  run_case "$rid-invalid-arity" 2 arch -"$cpu" "$binary" inspect
  if ! grep -Fq "0.1.0-preview.1+$source_sha" "$evidence/cases/$rid-version/stdout.txt"; then
    printf 'FAIL %s version string differs from frozen source SHA\n' "$rid" >> "$evidence/summary.txt"
    failures=$((failures + 1))
  fi
  if ! grep -Eiq 'usage|commands|options' "$evidence/cases/$rid-help/stdout.txt"; then
    printf 'FAIL %s help output lacks recognizable command description\n' "$rid" >> "$evidence/summary.txt"
    failures=$((failures + 1))
  fi
  for operation in inspect scan; do
    report="$evidence/cases/$rid-$operation/stdout.txt"
    if ! grep -Eq '"schemaVersion"[[:space:]]*:[[:space:]]*"1\.0\.0"' "$report" ||
       ! grep -Eq '"command"[[:space:]]*:[[:space:]]*"'"$operation"'"' "$report"; then
      printf 'FAIL %s %s output lacks expected v1 JSON report fields\n' "$rid" "$operation" >> "$evidence/summary.txt"
      failures=$((failures + 1))
    fi
  done
  if [ -s "$evidence/cases/$rid-invalid-arity/stdout.txt" ] ||
     ! grep -iq 'exactly one' "$evidence/cases/$rid-invalid-arity/stderr.txt"; then
    printf 'FAIL %s invalid-arity output differs from CLI contract\n' "$rid" >> "$evidence/summary.txt"
    failures=$((failures + 1))
  fi
done

printf 'UTC finish: %s\nFailed checks: %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$failures" >> "$evidence/summary.txt"
if [ "$failures" -ne 0 ]; then
  echo "FAIL: $failures checks failed. Preserve and inspect $evidence" >&2
  exit 1
fi
echo "PASS: Apple Silicon native and Rosetta cases complete; preserve $evidence"
