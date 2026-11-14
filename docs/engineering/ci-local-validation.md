# Local validation and GitHub Actions parity

## Adopted policy

**Repository-controlled scripts implement reproducible validation; GitHub
Actions orchestrates them.** Where practical, the same implementation must be
executable locally and in CI. Workflows own event triggers, change detection,
matrix selection, checkout, tool provisioning, caching, permissions, and
artifact publication. GitHub-managed features such as Dependabot and secret
scanning are exceptions because they run as services. CodeQL's configuration
is documented separately.

Prefer native `dotnet` commands for individual operations and focused,
cross-platform PowerShell 7 scripts when orchestration is useful. Preserve the
existing Python 3 validators. Validation must return a meaningful nonzero exit
code and actionable diagnostics without depending on GitHub-only variables;
keep installing prerequisites separate from checking the repository.

The Python validators, managed Release build/NUnit/style commands, and Native
AOT publish/smoke checks have reusable entry points. Workflows retain SDK and
native compiler provisioning, OS selection, reporting and artifact uploads.
Style verification is enabled on Linux CI. Examine CI results for the exact
candidate SHA on each platform; an SDK-equipped native smoke test is not
independent evidence of a runtime-free consumer environment.

## Prerequisites and environment evidence

- Git and Python 3 for the existing validators (Windows may use `py -3` in
  place of `python3`).
- The .NET 11 SDK selected by [global.json](../../global.json), plus NuGet
  access, for managed restore, build, test, and formatting.
- PowerShell 7 for the cross-platform validation scripts.
- A matching operating system and native compiler/linker prerequisites for
  Native AOT. Linux CI installs or verifies Clang and `zlib1g-dev` separately.
  macOS x64 verification uses an Intel runner; Windows uses an x64 runner.

Record the *actual* tool and OS environment and the exact source revision with
verification evidence. For example:

```shell
git rev-parse HEAD
git status --short
python3 --version
dotnet --info
```

If PowerShell is used, also capture `pwsh --version` and
`[System.Runtime.InteropServices.RuntimeInformation]::OSDescription`.
Distinguish a clean commit from a working tree with uncommitted changes. CI
job URLs, conclusions and exact candidate SHAs belong in the relevant PR;
scenario acceptance state belongs only in the
[traceability matrix](../testing/traceability.md).

## Run the current checks locally

Run from the repository root. These commands reflect the existing workflows;
the manual examples are **instructions**, not a claim they were run on every
platform.

### Documentation, architecture, and test metadata

```shell
python3 -m unittest discover -s scripts/tests -p 'test_*.py'
python3 scripts/validate_line_endings.py
python3 scripts/validate_docs.py
python3 scripts/validate_architecture.py
python3 scripts/validate_test_metadata.py
```

These require no extra Python packages. The line-ending check inspects Git's
index and checkout; stage intended files before checking tracked text. The
change-scope classifier (`scripts/ci_change_scope.py`) is an orchestration
component, **not** a local validation command: it intentionally needs
`GITHUB_EVENT_NAME`, `BASE_SHA`, `HEAD_SHA`, and `GITHUB_OUTPUT` when invoked by
Actions. Its decision logic is covered by the Python unit tests above.

### Managed .NET and NUnit

```shell
pwsh ./scripts/validate-managed.ps1 -VerifyStyle
pwsh ./scripts/collect-coverage.ps1
python3 scripts/coverage_report.py --input artifacts/coverage/raw --output artifacts/coverage/report --artifact-name coverage-local
```

Omit `-VerifyStyle` to match Windows/macOS CI checks. The managed script records
the actual SDK, OS, PowerShell version and commit, and fails on any unsuccessful
restore/build/NUnit/style command. The format-style check runs on Linux CI. Managed
restore/build/NUnit are matrixed over `ubuntu-latest`, `windows-latest`, and
`macos-latest`. Successful local execution on one OS cannot establish the
other two OS results. The optional coverage script runs NUnit again after a
Release build using Microsoft's built-in collector and the shared
[coverage settings](../../coverage.runsettings). It writes Cobertura XML and
TRX files under ignored `artifacts/coverage/raw`, with separate CI artifacts
per OS. The Python renderer produces annotated source-level HTML,
method/unexecuted-method details, JSON metrics and a Markdown summary. Open
`report/index.html` from the downloadable GitHub Actions artifact ZIP.

### Native AOT CLI smoke test

Run on an x64 operating system matching one of `linux-x64`, `win-x64`, or
`osx-x64`, after installing native AOT prerequisites:

```shell
pwsh ./scripts/validate-aot.ps1 -RuntimeIdentifier linux-x64
# Windows x64: pwsh ./scripts/validate-aot.ps1 -RuntimeIdentifier win-x64
# macOS Intel: pwsh ./scripts/validate-aot.ps1 -RuntimeIdentifier osx-x64
```

The script rejects a mismatched host or architecture and executes the resulting
`dotnet-binary` (or `dotnet-binary.exe` on Windows) with `--help` and `--version`,
checking both exit codes. CI checks only these scaffold commands; this is **not** proof of full inspection,
runtime-free clean installation, or release readiness.

## What CI does and does not run

| Workflow | Purpose | Current change behavior |
| --- | --- | --- |
| [Validate engineering documentation](../../.github/workflows/validate-engineering.yml) | Python unit tests, line endings, docs, architecture, and test metadata | Runs Python unit tests on every invocation; relevant validators are selected by changed paths. |
| [.NET 11 build, test and style](../../.github/workflows/build-test.yml) | Managed restore/audit, build, NUnit and Microsoft Cobertura coverage across three OSes; Linux style | All three required job contexts remain present; validation steps are omitted for documentation-only changes. |
| [.NET 11 Native AOT smoke test](../../.github/workflows/native-aot-smoke.yml) | RID-specific restore, publish, help and version on three OSes | All three required job contexts remain present; validation steps are omitted for documentation-only changes. |
| [Dependency review](../../.github/workflows/dependency-review.yml) | GitHub dependency-graph diff and high/critical vulnerability gate | Runs on pull requests; requires GitHub's dependency graph and advisory data, so there is no equivalent entirely local check. |

All workflows trigger on PRs targeting `main`, pushes to `main`, and manual
dispatch. Manual dispatch forces every applicable job in the full matrix.
Change detection fails rather than silently skipping checks when it cannot
classify a change. A green matrix job that ran only the intentional-omission
notice is **not validation evidence**. Keeping matrix jobs present allows
the ruleset's OS-specific required contexts to be reported even for
documentation-only PRs. NuGet
caching never replaces restore or auditing. Compare exact SHA and observed
check results before approving a PR, and run the full matrix when preparing a
release candidate.
