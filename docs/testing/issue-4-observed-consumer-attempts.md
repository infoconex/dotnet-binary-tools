# Issue #4 — observed physical consumer attempts

**Assessment as of 2026-11-19 UTC: BLOCKED; no independent x64 consumer pass.**
This is a diagnostic supplement to the [immutable candidate and operator
procedure](issue-4-consumer-acceptance.md).
The observations below concern the frozen, **unpublished**
`0.1.0-preview.1` source commit
`6dffde08e6a9853c68555c1e751bcd63f082e0bd`, produced by
[Native AOT workflow 44360433040](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44360433040).
The owner tested the downloaded packages on two physical machines; do not
confuse these attempts with independent Windows **runtime-free** or physical
Intel macOS **x64** acceptance.

## Windows x64 — physical desktop, SDK-equipped negative result

- **Host:** Windows 11 Pro, version `10.0.26300`, build `26300`, x64;
  PowerShell `7.7.0`. This OS identity is from the actual captured CIM
  inventory; the earlier `Get-ComputerInfo` product-name field misleadingly
  reported Windows 10.
- **.NET inventory:** SDKs `8.0.428`, `9.0.321`, `10.0.404`, **`11.0.100`**;
  modern shared runtimes from .NET 6 through 11 are present. This host **does
  not meet** Issue #4's SDK/runtime-absence requirement. The SDK-equipped
  desktop test was supplemental compatibility diagnostics only.
- **Artifacts:** The downloaded GitHub workflow ZIP, standalone ZIP and
  executable SHA-256 values matched the frozen manifests. The five native
  build manifest entries passed the operator's individual checks.

| Exact object | Observed SHA-256 |
| --- | --- |
| `pre-release-win-x64.zip` | `7b83dd06cc7bc513b13ca0dbd92ccaf2425c087c0d7e8c347b7769c9e1e9af7a` |
| `dotnet-binary-0.1.0-preview.1-win-x64.zip` | `0b8204ec74f7c1120c51cc2472c246e5827a8ef60c9987e0086a1728c45fea49` |
| `dotnet-binary.exe` | `972c77903a325c0925c2a0556ecddb329a5410e733373dd5a5d70ea24dee0fd0` |

| Invocation (standalone `dotnet-binary.exe`) | Start UTC | End UTC | Result |
| --- | --- | --- | --- |
| `--help` | `2026-11-19T06:05:24.2185289Z` | `2026-11-19T06:05:25.5930401Z` | `launch-failed`; no application exit code |
| `--version` | `2026-11-19T06:05:25.6061791Z` | `2026-11-19T06:05:26.6480641Z` | `launch-failed`; no application exit code |
| `inspect`, `scan`, malformed invocation | — | — | **Not attempted** after the execution-policy block |

Both launch attempts captured **empty** raw `stdout.txt` and `stderr.txt`.
The `launch-error.txt` for each case contains:

> This command cannot be run due to the error: An Application Control policy has blocked this file.

`Get-AuthenticodeSignature` reported `NotSigned`. Two Code Integrity
**enforcement** events (`3077`) at `2026-11-18 22:05:25` and `22:05:26`
Pacific identified the blocked executable and policy
`{0283ac0f-fff1-49ae-ada1-8a933130cad6}`. Microsoft identifies that
policy as **VerifiedAndReputableDesktop**, the [Windows 11 Smart App Control
enforcement policy](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/windows-defender-application-control/operations/inbox-wdac-policies).
The [event documentation](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/app-control-for-business/operations/event-id-explanations)
confirms `3077` means an enforced block. **Diagnosis: signed-distribution
compatibility blocker, not observed application logic failure.**

**Preserved evidence:** Operator-created
`issue4-windows-desktop-failed-evidence.zip` with raw per-command
`command.txt`, `stdout.txt`, `stderr.txt`, `launch-error.txt`, `result.txt`,
environment inventory, signature, SHA-256 and Code Integrity events.
SHA-256 **`c467faa663aa77be0b37cac3cd810e776f9793f0ae0f743c653fd4510b526707`**
was independently rechecked against the uploaded ZIP. The owner retains the
original; it is **not** a published GitHub Actions artifact.

## macOS ARM64 — physical clean Mac mini M4, supplemental failed attempt

- **Host:** macOS `26.4.1`, build `25E253`, Apple Silicon `arm64` on a
  physical Mac mini M4. The owner attested no previous .NET SDK or shared
  runtime installation. Standard install paths, `dotnet` on PATH, macOS
  receipts and Homebrew package checks found none. This is a credible
  **ARM64** clean consumer, not an Intel x64 host.
- **Immutable outer ZIPs:** `pre-release-osx-arm64.zip` SHA-256
  `085074157d2b5657ad866decb755b24eabf43f20d31554de2a69a2f47db16e45`;
  `pre-release-osx-x64.zip` SHA-256
  `591d4fb3bf4327051f18da7d3f2a56bda43ead509da8a444b534b7bee02f5b6f`.
  Both matched when the owner checked them locally.
- **Native ARM64:** All five launched scenarios exited `137` (`SIGKILL`),
  with empty case stderr; this prevented meaningful `--help`, `--version`,
  `inspect`, `scan` and malformed-invocation assertions. macOS displayed a
  Gatekeeper verification dialog. Read-only inspection confirmed Mach-O
  ARM64 executable, Chrome quarantine attribute, **ad hoc linker signature**
  with no Team ID, and `spctl --assess --type execute` **rejected**. Gatekeeper
  is a demonstrated distribution-trust blocker, although the exact source
  of `SIGKILL` has not been independently proven.
- **Rosetta x64:** The Rosetta probe and x64 executable launch both returned
  `Bad CPU type in executable`. Rosetta translation is unavailable on this
  host at the time of testing, so the five x64 cases did not run.
- **Summary:** The owner reported `21` failed checks, mostly cascading
  output/exit-code assertions caused by these two launch blockers. This
  does **not** establish an application logic defect or a successful test.

**Preserved evidence:** Owner-reported
`evidence-mac-physical.tar.gz`, SHA-256
`e02fee3992b011ab29fd4943d0480d6c4e431062a054ef09b0b7dfba0dc9fb9e`.
The archive has **not** been supplied for independent review or attached to
the repository, so macOS findings above remain operator-reported.

## Acceptance and release decision matrix

| Issue #4 acceptance criterion | Status | Explanation |
| --- | --- | --- |
| Genuine Windows x64 host without SDK/shared runtime | **Pending** | Desktop is x64 but has .NET SDK 8–11 and shared runtimes; no clean consumer executed. |
| Genuine Intel macOS x64 host without SDK/shared runtime | **Pending** | Intel testing deferred by owner; ARM64/Rosetta are not Intel-native substitutes. |
| Immutable candidate SHA and manifest verification on both required consumers | **Pending** | Windows desktop hashes and Mac supplemental ZIP hashes matched, but neither required clean x64 host qualified. |
| `--help`, `--version`, representative inspection and malformed CLI invocation pass on both | **Blocked** | Windows Smart App Control, macOS Gatekeeper; no meaningful program output. |
| OS dependencies, security restrictions and packaging exceptions reviewed | **Partial / blocker identified** | Windows unsigned executable and macOS ad hoc signing; Rosetta unavailable on ARM64 host. Remediation not yet tested. |
| v0.1 readiness and scenario traceability supported by actual acceptance | **Blocked** | `SC-DST-005` remains blocked; no distribution or release authorization. |

## Engineering disposition — plan only, not authorization to sign or release

1. **Windows:** Design an Authenticode signing stage using an appropriately
   trusted code-signing certificate and verified timestamping. Confirm with
   `Get-AuthenticodeSignature` and retest the **new** signed candidate with
   Smart App Control enforced. Microsoft documents [Smart App Control's
   signing model](https://learn.microsoft.com/en-us/windows/apps/develop/smart-app-control/overview).
2. **macOS:** Design Developer ID Application signing and Apple notarization,
   including review of the distribution format and ticket delivery for a
   standalone CLI. Verify `codesign`, `spctl` and a consumer download without
   disabling Gatekeeper. Follow [Apple notarization
   guidance](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution/).
3. **Security and approvals:** Obtain explicit owner approval for certificate
   procurement, developer accounts, signing identities and protected CI secret
   use before implementation. Never commit keys, notarization credentials or
   signed secrets to the repository or expose them in build logs. Keep normal
   PR builds unprivileged.
4. **New candidate and acceptance:** Signing changes executable bytes and
   may change packaging. Build an immutable new candidate, regenerate and
   verify all SHA-256 manifests, and rerun both genuine runtime-free Windows
   x64 and physical Intel macOS x64 acceptances. The ARM64 Mac mini is useful
   supplemental coverage after trust remediation. Preserve failed attempts
   unchanged, and investigate any new functional error separately.

**STOP:** Do not close Issue #4, promote `SC-DST-005`, merge PR #30, tag,
publish, or release on the basis of these failures. Source/PR review and
publication require separate explicit owner approval.
