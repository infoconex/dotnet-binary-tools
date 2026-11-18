# Issue #4 — independent Native AOT consumer acceptance

**Status (2026-11-18 UTC): BLOCKED; not release-ready.** No independently provisioned,
SDK/runtime-free Windows x64 or physical Intel macOS x64 host was available to
this assessment. Neither the scripts below nor a successful SDK-equipped CI
runner establishes those two acceptance results. Do not close [Issue #4](https://github.com/infoconex/dotnet-binary-tools/issues/4),
promote artifacts, tag, release, or publish without the actual consumer logs and
separate owner authorization.

## Immutable candidate and integrity preparation

- Source: `main` at `6dffde08e6a9853c68555c1e751bcd63f082e0bd`.
- Version: `0.1.0-preview.1`.
- [Complete Native AOT/distribution workflow](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433040),
  with six native builds and all thirteen distribution inputs. Artifacts expire
  on December 2, 2026 (UTC); retain the extracted evidence or acquire a new
  frozen candidate and regenerate these pinned hashes before expiration.
- [Managed/NUnit](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433068),
  [CodeQL](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360432870),
  [Documentation](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360437476).

| Object | Artifact ID | SHA-256 (actual equals expected) |
| --- | --- | --- |
| GitHub workflow ZIP `pre-release-win-x64` | `14453944887` | `7b83dd06cc7bc513b13ca0dbd92ccaf2425c087c0d7e8c347b7769c9e1e9af7a` |
| GitHub workflow ZIP `pre-release-osx-x64` | `14454039718` | `591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f` |
| GitHub workflow ZIP `pre-release-distribution` | `14454665812` | `5a9edfd9a128ad1ebd810d35a1fd8751c36d60624e8959eb244fc93429ca75c0` |
| Windows standalone ZIP | in Windows artifact | `0b8204ec74f7c1120c51cc2472c246e5827a8ef60c9987e0086a1728c45fea49` |
| macOS standalone tar.gz | in macOS artifact | `1126739ffb5c75accd01de2827e2555c20b59ef1c57ede11c84fb065ea140f49` |
| Windows x64 `dotnet-binary.exe` | in Windows standalone | `972c77903a325c0925c2a0556ecddb329a5410e733373dd5a5d70ea24dee0fd0` |
| macOS x64 `dotnet-binary` | in macOS standalone | `330fb27234b739a8571f52bd79239573469642c89e8a0ac570199e88c08c3f9a` |

On a separate Linux x64 preparation host, the actual downloaded workflow ZIPs
matched GitHub's SHA-256 digests. Checks with `sha256sum -c SHA256SUMS.txt` from
each native artifact directory passed **5/5 Windows** and **7/7 macOS** items.
The complete distribution manifest passed **6/6 standalone** and **7/7 NuGet**
checks. The standalone archives contained the same executable bytes as their
native job outputs. `file` identified a PE32+ Windows x86-64 console executable
and a Mach-O x86_64 executable. This preparation host did **not** execute them.
The initial checksum invocation from the ZIP root failed because each manifest
uses names relative to its *own* directory; rerunning in those directories
passed. This was a verification-command path error, not a bad artifact.

## What remains unverified

| Issue #4 acceptance criterion | Assessment | Evidence required to pass |
| --- | --- | --- |
| Genuine clean Windows x64 host, SDK/runtime absent | Pending | Fresh image provenance, inventory, native execution log |
| Genuine clean Intel macOS x64 host, SDK/runtime absent | Pending | Genuine Intel machine/legitimate macOS VM provenance, inventory, native execution log |
| Candidate SHA, immutable artifacts and per-platform hashes | Prepared, not complete on hosts | Repeat pinned SHA-256 verification **on both consumers** |
| Native `--help`, `--version`, `inspect`, `scan` and malformed invocation | Pending | Raw stdout/stderr, exit codes, OS/CPU and UTC time per host |
| Native OS dependencies/signing/launch restrictions reviewed | Pending | Actual host observations; diagnose each failure with OS build and error text |
| Release readiness and traceability updated | Partial | Promote `SC-DST-005` only after both hosts genuinely pass; ARM64 gates remain independent |

The latest product scope also includes `win-arm64` and `osx-arm64`, as approved
in [Issue #28](https://github.com/infoconex/dotnet-binary-tools/issues/28).
Completing just this x64 procedure must not be represented as satisfying their
separate clean-consumer release gates.

## Consumer environment acquisition

1. **Windows x64:** Use an independently provisioned Windows 11 or supported
   Windows Server x64 VM/machine from a clean base image with no modern .NET SDK
   or shared .NET runtime. Verify image origin and OS installation state.
   **Do not uninstall .NET from a developer machine or hide it by modifying PATH.**
2. **Intel macOS x64:** Use a physical Intel Mac with a clean installation, or
   a legitimately hosted **Intel Mac** virtual machine with a fresh macOS
   image. Do not use an ARM Mac running the binary under Rosetta, a Linux
   emulator, or an SDK-equipped GitHub Actions runner as acceptance evidence.
3. Record image provenance in a plain UTF-8 `provenance.txt` containing the
   operator, machine/VM provider (no credentials), base OS image/version,
   creation date and means of independently establishing no .NET installation.
   A script cannot prove the absence of an undisclosed custom installation;
   the fresh-image provenance and on-host inventory are both required.
4. Keep these hosts disposable or free of unrelated data. No internet access is
   required *during* execution once the ZIP and this script have been copied.
   The test never opens or invokes the inspected synthetic file as a program.

## Exact operator procedure

Download the two `pre-release-<rid>` ZIP artifacts from the linked **exact run
44360433040**. Preserve their original contents; do not rebuild, rename binary
contents, or substitute a later workflow run. Download/copy the corresponding
script from this Issue #4 branch. In a new, disposable working directory, create
the `provenance.txt` note and run the appropriate command.

### Windows x64 (inbox Windows PowerShell 5.1, 64-bit)

```powershell
& 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -NoProfile -File '.\win-x64.ps1' `
  -ArtifactZip '.\pre-release-win-x64.zip' `
  -EvidenceDirectory '.\evidence-win-x64' `
  -ProvenanceFile '.\provenance.txt'

Compress-Archive -LiteralPath '.\evidence-win-x64' -DestinationPath '.\evidence-win-x64.zip'
Get-FileHash '.\evidence-win-x64.zip' -Algorithm SHA256
```

Review the script before invoking it; if an organizational execution policy
blocks it, record that result and follow the approved administrative process.
Do not bypass the policy for this test. The script uses **Windows PowerShell 5.1** (the Windows .NET Framework
component), not PowerShell 7 or the separately installed modern .NET runtime.
It checks both Program Files trees, per-user defaults, the installed-version
registry keys and PATH without deliberately concealing installations.

### Intel macOS x64 (system Bash)

```bash
uname -m # must be x86_64 on an Intel host
bash ./osx-x64.sh ./pre-release-osx-x64.zip ./evidence-osx-x64 ./provenance.txt
tar -czf ./evidence-osx-x64.tar.gz ./evidence-osx-x64
shasum -a 256 ./evidence-osx-x64.tar.gz
```

The script checks `sw_vers`, the Intel CPU identity, standard runtime directories,
installed package receipts and PATH. An absent `dotnet` command *alone* is never
considered sufficient. Mac download quarantine/Gatekeeper behavior should be
recorded accurately; **do not disable Gatekeeper, bypass policy, or remove
quarantine as an undocumented workaround.**

## Evidence outputs and interpretation

Both scripts refuse to reuse an existing evidence directory. The directory
contains:

- `provenance.txt`, `environment.txt`, `context.txt`, `checksums.txt`;
- `summary.txt` with per-scenario expected/actual exit status and UTC times;
- `cases/help`, `cases/version`, `cases/inspect`, `cases/scan`,
  `cases/invalid-arity`: exact argument vector, raw `stdout.txt`, raw
  `stderr.txt`, and `exit-code.txt`;
- `work/`: verified artifacts and an inert `synthetic.exe` text fixture.

Required exit codes are `0` for help/version/inspect/scan and `2` for an
`inspect` command missing its path. `--version` must identify
`0.1.0-preview.1+6dffde08e6a9853c68555c1e751bcd63f082e0bd`.
Check the output JSON semantics and warning text as part of the owner review;
exit codes alone do not prove meaningful output. Preserve raw reports and
investigate any unexpected classification, missing file, unavailable system
library, security prompt, architecture mismatch, or process launch error.

If a case fails, attach the unmodified evidence ZIP/tar.gz privately or to
an appropriate repository artifact location only after reviewing for user data,
then provide its SHA-256, the OS and candidate identity, and a concise failure
analysis. Do not silently retry on a different host or change the product
binary. A tool defect requires a new reviewed candidate, new hashes and a
repeat of both platform acceptances; an OS prerequisite or policy issue needs
separate documented review.

## Sources and restrictions

- [Microsoft Native AOT deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/):
  Native AOT requires no separately installed .NET runtime, but the target OS
  still supplies native dependencies; documentation alone is not our test.
- [GitHub-hosted runners](https://docs.github.com/en/actions/concepts/runners/github-hosted-runners):
  normal hosted Windows/macOS images contain development tooling and are not
  independently clean consumer environments.
- [Release promotion gates](../engineering/release-gates.md) and
  [authoritative scenario traceability](traceability.md).
