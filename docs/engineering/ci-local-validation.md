# Local validation and GitHub Actions parity

## Adopted policy

**Repository-controlled scripts implement reproducible validation; GitHub
Actions orchestrates them.** Where practical, the same implementation must be
executable locally and in CI. Workflows own event triggers, change detection,
matrix selection, checkout, tool provisioning, caching, permissions, and
artifact publication. GitHub-managed features such as Dependabot, CodeQL
default setup, and secret scanning are exceptions because they run as services.

Prefer native `dotnet` commands for individual operations and focused,
cross-platform PowerShell 7 scripts when orchestration is useful. Preserve the
existing Python 3 validators. Validation must return a meaningful nonzero exit
code and actionable diagnostics without depending on GitHub-only variables;
keep installing prerequisites separate from checking the repository.

**Current state:** Existing Python validators already run from both local
shells and CI. Managed build/test/style and Native AOT validation currently
use `dotnet` commands embedded directly in workflow YAML. Moving that logic to
small reusable scripts, with tests for nontrivial script decisions, belongs to
[Issue #2's testing and developer-tooling workstream](https://github.com/infoconex/dotnet-binary-tools/issues/2).
This policy does **not** claim full local/CI parity has already been achieved.

## Prerequisites and environment evidence

- Git and Python 3 for the existing validators (Windows may use `py -3` in
  place of `python3`).
- The .NET 11 SDK selected by [global.json](../../global.json), plus NuGet
  access, for managed restore, build, test, and formatting.
- PowerShell 7 for cross-platform orchestration as it is introduced.
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
dotnet restore DotNetBinary.slnx --configfile nuget.config --force-evaluate
dotnet build DotNetBinary.slnx --configuration Release --no-restore
dotnet test DotNetBinary.slnx --configuration Release --no-build --no-restore --logger trx
dotnet format style DotNetBinary.slnx --no-restore --verify-no-changes --diagnostics IDE1006 --severity error --verbosity normal
```

The format-style check runs on Linux in the current workflow. Managed
restore/build/NUnit are matrixed over `ubuntu-latest`, `windows-latest`, and
`macos-latest`. Successful local execution on one OS cannot establish the
other two OS results.

### Native AOT CLI smoke test

Run on the operating system matching one of `linux-x64`, `win-x64`, or
`osx-x64`, after installing its native AOT prerequisites. Replace `<rid>`
with the matching RID (no angle brackets in the actual command):

```shell
dotnet restore src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj -r <rid> --configfile nuget.config
dotnet publish src/Infoconex.DotNetBinary.Cli/Infoconex.DotNetBinary.Cli.csproj -c Release -r <rid> --configfile nuget.config --no-restore -p:PublishAot=true -o artifacts/aot/<rid>
```

Execute the resulting `dotnet-binary` (or `dotnet-binary.exe` on Windows)
with `--help` and `--version` and check both exit codes. Current CI checks
only these scaffold commands; this is **not** proof of full inspection,
runtime-free clean installation, or release readiness.

## What CI does and does not run

| Workflow | Purpose | Current change behavior |
| --- | --- | --- |
| [Validate engineering documentation](../../.github/workflows/validate-engineering.yml) | Python unit tests, line endings, docs, architecture, and test metadata | Runs Python unit tests on every invocation; relevant validators are selected by changed paths. |
| [.NET 11 build, test and style](../../.github/workflows/build-test.yml) | Managed restore/audit, build, NUnit across three OSes; Linux style | The managed matrix is skipped for documentation-only changes. |
| [.NET 11 Native AOT smoke test](../../.github/workflows/native-aot-smoke.yml) | RID-specific restore, publish, help and version on three OSes | The AOT matrix is skipped for documentation-only changes. |

All workflows trigger on PRs targeting `main`, pushes to `main`, and manual
dispatch. Manual dispatch forces every applicable job in the full matrix.
Change detection fails rather than silently skipping checks when it cannot
classify a change. **A skipped matrix is not validation evidence.** NuGet
caching never replaces restore or auditing. Compare exact SHA and observed
check results before approving a PR, and run the full matrix when preparing a
release candidate.
