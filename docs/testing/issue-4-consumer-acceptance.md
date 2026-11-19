# Issue #4 — independent Native AOT consumer acceptance

**Status (2026-11-19 UTC): BLOCKED; not release-ready.** No independently provisioned,
SDK/runtime-free Windows x64 or physical Intel macOS x64 host was available to
this assessment. The owner deferred Intel-hardware rental for now and approved
preparing supplementary testing on an existing Apple Silicon Mac; this is
**not** a waiver of Intel x64 acceptance. Neither these scripts nor a successful
SDK-equipped CI runner establishes those two acceptance results. Do not close
[Issue #4](https://github.com/infoconex/dotnet-binary-tools/issues/4),
promote artifacts, tag, release, or publish without the actual consumer logs and
separate owner authorization.

## Immutable candidate and integrity preparation

- Source: `main` at `6dffde08e6a9853c68555c1e751bcd63f082e0bd`.
- Version: `0.1.0-preview.1`.
- [Complete Native AOT/distribution workflow](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433040),
  with six native builds and all thirteen distribution inputs. Artifacts expire
  on December 2, 2026 (UTC); retain the extracted evidence or acquire a new
  frozen candidate and regenerate these pinned hashes before expiration.
- [Observed physical Windows/macOS launch failures, signing diagnosis and acceptance matrix](issue-4-observed-consumer-attempts.md). The Windows evidence ZIP was independently checked; macOS archive findings remain operator-reported pending independent file review.
- [Managed/NUnit](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433068),
  [CodeQL](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360432870),
  [Documentation](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360437476).

| Object | Artifact ID | SHA-256 (actual equals expected) |
| --- | --- | --- |
| GitHub workflow ZIP `pre-release-win-x64` | `14453944887` | `7b83dd06cc7bc513b13ca0dbd92ccaf2425c087c0d7e8c347b7769c9e1e9af7a` |
| GitHub workflow ZIP `pre-release-osx-x64` | `14454039718` | `591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f` |
| GitHub workflow ZIP `pre-release-osx-arm64` | `14453764676` | `085074157d2b5657ad866decb755b24eabf43f20d31554de2a69a2f47db16e45` (GitHub artifact metadata; not yet rechecked on a consumer) |
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
| Genuine clean Windows x64 host, SDK/runtime absent | **Pending** | The SDK-equipped physical Windows 11 desktop failed to launch under Smart App Control; a genuinely runtime-free host is still required. |
| Genuine clean Intel macOS x64 host, SDK/runtime absent | Pending | Genuine Intel machine/legitimate macOS VM provenance, inventory, native execution log |
| Candidate SHA, immutable artifacts and per-platform hashes | **Partial** | Windows desktop outer ZIP, standalone ZIP, executable and five native manifest checks passed; supplemental Mac ZIP hashes matched. Repeat verification on **both required clean x64 consumers**. |
| Native `--help`, `--version`, `inspect`, `scan` and malformed invocation | **Blocked** | Windows launch denied by Smart App Control; supplemental macOS ARM64 was killed and Rosetta x64 was unavailable. No application behavior has passed on required consumers. |
| Native OS dependencies/signing/launch restrictions reviewed | **Partially diagnosed** | Windows unsigned executable blocked by Code Integrity policy; macOS Gatekeeper rejects an ad hoc-signed/quarantined ARM64 binary; see [evidence report](issue-4-observed-consumer-attempts.md). |
| Release readiness and traceability updated | **Blocked; documented** | `SC-DST-005` stays Blocked until both clean x64 hosts actually pass; signing/notarization and ARM64 gates remain separate. |

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

### Supplemental: Apple Silicon Mac mini M4 / Mac Studio M3 Ultra

The owner has access to both machines, but **neither contains an Intel CPU**.
Use this path to collect independent macOS ARM64 consumer execution and
**Rosetta-translated** x64 execution. The two execution modes must be reported
separately; **Rosetta is not physical Intel macOS x64 acceptance**. Do not change
the approved RID support or promote `SC-DST-005` on these results alone.

#### Preferred when the physical Mac has never had .NET installed

The owner reported macOS **26.4.1 (25E253)** on a physical Mac mini M4,
`arm64`, no `dotnet` command on PATH, all seven checked standard installation
locations absent, no .NET package receipts and no Homebrew .NET packages. The
owner independently confirmed that **no .NET SDK or shared runtime was ever
installed on that Mac**. These are operator-reported screening observations,
**captured in the failed first test attempt**. The full macOS evidence archive has not yet been supplied for independent review. They make the physical
Mac a credible clean ARM64 consumer candidate without creating a VM.

The original `pre-release-osx-arm64.zip` and `pre-release-osx-x64.zip` were
downloaded onto that Mac and their pinned workflow SHA-256 digests independently
matched the values above, as reported by the owner. The real execution step was attempted and **failed** due to Gatekeeper rejection
(ARM64) and unavailable Rosetta translation (x64). The original attempt and
reported archive SHA-256 are recorded in the [observed-attempts report](issue-4-observed-consumer-attempts.md).

Download both `run-osx-apple-silicon-physical.sh` and
`osx-apple-silicon.sh` from **PR #30's branch** into one directory. The physical
entry point asks for an explicit installation-history attestation, checks the
exact ZIP hashes again, and then runs the platform-neutral consumer script.
From that directory, use a **new** output path:

```bash
bash ./run-osx-apple-silicon-physical.sh \
  "$HOME/Downloads/pre-release-osx-arm64.zip" \
  "$HOME/Downloads/pre-release-osx-x64.zip" \
  "$HOME/Downloads/issue4-physical-mac-evidence"
```

Expected output under `issue4-physical-mac-evidence/` includes
`provenance.txt`, `operator-run.log`, `test-exit-code.txt`,
`evidence/test-summary.md`, `evidence/environment.txt`,
`evidence/checksums.txt`, `evidence/summary.txt`, separate raw case logs
for each ARM64 and Rosetta x64 command, and `evidence-mac-physical.tar.gz` plus
`evidence-mac-physical.sha256.txt`. The owner preserved the failed first attempt; **preserve any failed first attempt** and
use a new results directory on retry. Review Gatekeeper/Rosetta prompts
without weakening security policies. The physical Mac proves nothing about
Intel-native execution, which remains deferred, or Windows x64, still pending.

This direct test requires no Homebrew, Tart, guest image or .NET installation.
The script does not scan unrelated personal files, although evidence may
contain paths identifying the local account; review before sharing.

#### Alternative: scripted fresh-VM setup and guest execution (Tart)

The repository provides **two cooperating scripts**: a host provisioner
(`provision-osx-apple-silicon-vm.sh`) and a guest runner
(`run-osx-apple-silicon-guest.sh`). The guest runner calls the already-reviewed
`osx-apple-silicon.sh` scenario suite. Tart is an open-source CLI built on
[Apple's Virtualization framework](https://developer.apple.com/documentation/virtualization);
the [Tart quick start](https://tart.run/quick-start/) documents creation of a
fresh VM from an Apple IPSW, VM launching and read-only directory sharing.
This avoids installing any SDK, .NET runtime, SSH server, third-party guest
agent or developer tool inside the **test consumer**.

On the **Apple Silicon host**, install the official Tart CLI if not already
installed (this changes the *host*, not the clean guest):

```bash
brew install openai/tools/tart
```

Alternatively, follow Tart's official release-installation instructions when
Homebrew is unavailable. Review that installation as you would any third-party
host tooling. Allow space for a fresh macOS VM and the Apple IPSW download;
approximately **80 GB of free host storage** is a prudent working allowance,
not a guaranteed minimum for all macOS releases. The host needs internet access
to download macOS; the standalone product tests themselves do not use it.

Download the **unaltered workflow ZIPs** `pre-release-osx-arm64` and
`pre-release-osx-x64` from [frozen run 44360433040](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433040).
The ZIPs must have the SHA-256 values in the table above. GitHub CLI's
`gh run download` **extracts** these ZIPs and must not be used as the input to
this provisioner. Download the host and guest scripts plus
`osx-apple-silicon.sh` together from **PR #30's branch**, and invoke:

```bash
bash ./provision-osx-apple-silicon-vm.sh \
  "$HOME/Downloads/pre-release-osx-arm64.zip" \
  "$HOME/Downloads/pre-release-osx-x64.zip" \
  "$HOME/Downloads/issue4-fresh-macos-vm"
```

The third argument **must not already exist**. The host script does the
following in order:

1. Refuses non-Apple-Silicon hosts, missing files, preexisting output or a
   wrong ZIP SHA **before creating a VM**.
2. Makes an evidence workspace with an input-only mount and a separate
   writable results mount. Records host/Tart versions, candidate identity,
   pinned digests and the exact Apple IPSW provisioning method.
3. Creates a uniquely named new VM using `tart create --from-ipsw=latest`,
   **not** a cached development VM or a third-party SDK-equipped image.
4. Launches the GUI with both mounts and displays the single guest command.
   **First boot is intentionally interactive:** complete Apple's macOS Setup
   Assistant, create a disposable local account with no personal data and
   do not install developer tools or sign into a personal Apple ID.
5. Inside the guest, open Terminal and execute:

   ```bash
   bash "/Volumes/My Shared Files/acceptance-input/run-osx-apple-silicon-guest.sh"
   ```

   After attesting the guest was freshly installed and no .NET was installed,
   the guest script verifies the ZIPs/manifests, extracts the exact standalone
   binaries and runs five ARM64-native plus five Rosetta x64 cases. It produces
   per-case raw stdout/stderr, exit codes, UTC timestamps, a concise
   `test-summary.md` with explicit deferred criteria, and an evidence tar.gz
   + SHA-256 in the `results/` share.
6. Shut down the guest normally. The host prints all recorded guest attempt
   exit codes, archive digests, VM name and evidence directory. Failed attempts
   and the VM are **preserved**. Use the generated `resume-vm.sh` to reopen the
   same VM, install Rosetta through macOS's ordinary prompt if required, and
   rerun the guest script. Each attempt receives a new evidence folder; no
   prior logs are overwritten.

**Automation limit:** Apple macOS Setup Assistant and the explicit guest
attestation require a person. Full unattended first-boot provisioning would
require different macOS-version prerequisites and/or guest management
capabilities; do not silently install a guest agent, enable SSH, turn off
Gatekeeper or disable macOS authentication to skip them. The Tart VM is left
intact until the operator confirms the evidence is retained elsewhere. Do not
delete it automatically. The host script's unit tests use a **mock Tart** on
Linux and do not constitute actual VM provisioning or consumer acceptance.

#### Alternative: manually provisioned macOS ARM64 VM

1. On the Mac mini M4, use an Apple-supported macOS virtualization application
   and a **new macOS ARM64 guest installed from Apple-provided media**.
   [Apple's Virtualization framework](https://developer.apple.com/documentation/virtualization/virtualize-macos-on-a-mac)
   supports macOS guests on Apple Silicon. Record the macOS image origin, guest
   creation time, VM product/version and settings in the provenance note.
   No SDK, shared .NET runtime or developer toolchain should be installed in
   the guest. Do not use a snapshot of a development VM or a host with .NET
   merely hidden from `PATH`.
2. For the frozen [run 44360433040](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433040),
   download both **`pre-release-osx-arm64`** and **`pre-release-osx-x64`**
   workflow ZIPs into the guest (or transfer the unmodified ZIPs via a VM share).
   The runner's build toolchain is *not* transferred. Download
   `scripts/consumer-acceptance/osx-apple-silicon.sh` from **PR #30's branch**.
   Do not replace the ZIPs with another workflow run or recompress them.
3. Record a non-sensitive `provenance.txt` in the guest with operator, host model,
   guest/VM software version, Apple macOS installer source, fresh-install date,
   and confirmation that .NET was not installed. Do not include an Apple ID,
   hostname, account name or other unrelated personal information.
4. Review the script, then in a new test directory run:

   ```bash
   bash ./osx-apple-silicon.sh \
     ./pre-release-osx-arm64.zip \
     ./pre-release-osx-x64.zip \
     ./evidence-mac-arm64-rosetta \
     ./provenance.txt
   tar -czf ./evidence-mac-arm64-rosetta.tar.gz ./evidence-mac-arm64-rosetta
   shasum -a 256 ./evidence-mac-arm64-rosetta.tar.gz
   ```

5. The script requires a genuine macOS ARM64 guest and checks .NET installation
   locations, package receipts and PATH. It compares **both outer workflow ZIPs**
   against GitHub's pinned SHA-256 digests, validates each `SHA256SUMS.txt`,
   extracts the standalone archives and records their executable digests.
   It invokes native ARM64 commands with `arch -arm64` and x64 commands with
   `arch -x86_64`, recording a translated-process Rosetta probe, five cases for
   each architecture, raw stdout/stderr, exact commands, exit codes and UTC times.
   Fresh-image provenance is necessary; these inventory checks alone cannot
   exclude undisclosed nonstandard installations.
6. If Rosetta is not installed, record the original failure first. Apple's
   [Rosetta instructions](https://support.apple.com/en-us/102527) describe its
   normal installation prompt for Intel-only apps. Installing Rosetta is not
   installing .NET; note the installation and rerun with a **new evidence
   directory**, preserving the original failure logs. Do not disable security
   controls or alter the x64 product binary to make it launch. Apple's
   [translation documentation](https://developer.apple.com/documentation/Apple-Silicon/about-the-rosetta-translation-environment)
   explains why this does not substitute for actual Intel execution.
7. Inspect and share the evidence archive after checking for personal data.
   Include its SHA-256 in the Issue #4 discussion or PR evidence. No success
   should be recorded until the **actual Mac VM output** is independently
   reviewed, including the report semantics, not just exit codes.

**Operator results: PENDING.** This supplemental script has not yet been run in
a real Apple Silicon macOS guest. It must not be represented as an observed
runtime-free acceptance pass based on Linux syntax checks or CI.

## Evidence outputs and interpretation

All three scripts refuse to reuse an existing evidence directory. The directory
contains:

- `provenance.txt`, `environment.txt`, `context.txt`, `checksums.txt`;
- `summary.txt` with per-scenario expected/actual exit status and UTC times;
- `cases/help`, `cases/version`, `cases/inspect`, `cases/scan`,
  `cases/invalid-arity`: exact argument vector, raw `stdout.txt`, raw
  `stderr.txt`, and `exit-code.txt`;
- `work/`: verified artifacts and an inert `synthetic.exe` text fixture.

The Apple Silicon supplemental script places each RID's five cases under
`cases/osx-arm64-*` and `cases/osx-x64-*` and additionally records
`rosetta-command.txt`, `rosetta-stdout.txt`, `rosetta-stderr.txt` and
`rosetta-exit-code.txt`. Its two architecture-specific results are independent
and must never be collapsed into a claim of Intel-native success.

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
