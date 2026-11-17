# Packaging and distribution plan

**CAP-008 approved; no public release.** The proposed NuGet ID
`Infoconex.DotNetBinary.Tools` is not reserved, and the project version
`0.1.0-preview.1` is not an actual published release.

Initial delivery includes a standalone Native AOT executable and a NuGet
.NET Tool, supporting **win-x64, win-arm64, linux-x64, linux-arm64, osx-x64, and osx-arm64**. The tool
exposes `dotnet-binary` and, after tool installation, `dotnet binary`.

## Reproducible local pre-release checks

Install .NET 11, PowerShell 7, NuGet access and the native compiler prerequisites
for a matching native x64/ARM64 host. In the repository root:

```shell
pwsh ./scripts/validate-aot.ps1 -RuntimeIdentifier linux-x64
pwsh ./scripts/validate-tool-package.ps1 -RuntimeIdentifier linux-x64
```

Use the corresponding RID on Windows or macOS, including Apple Silicon. Both scripts first
restore the CLI project's declared multi-RID dependency graph in NuGet
`--locked-mode`, without passing an individual `-r` to restore. The graph
and tool metadata both declare the same six approved native RIDs. The first script publishes for the host RID
with `--no-restore` and smoke-tests `--help`/`--version`. The second builds
both the matching Native AOT RID package and the top-level pointer package
without restoring again, then installs the latter into a fresh private `--tool-path` from the
local package source. It tests the installed shim (a `.cmd` launcher on the
observed Windows runner) and `dotnet binary`.
Neither script pushes artifacts or updates a global tool installation.
`scripts/write-artifact-checksums.ps1` writes SHA-256 manifests for generated
artifact directories. On Linux, provision the `ubuntu:24.04` container image
separately, then run `scripts/validate-linux-runtime-free.ps1 -RuntimeIdentifier linux-x64` (or `linux-arm64` on ARM64) to verify the
published executable works without an installed .NET runtime or network.

In .NET 10 and later, a RID-specific Native AOT tool uses **two package
steps**: `dotnet pack -r <rid>` creates the native subpackage on the matching
OS; `dotnet pack` without a RID creates the cross-RID pointer package.
All RID subpackages must use the same version as the pointer package. The
pointer package must be published **after** all required subpackages, not
before. See [Microsoft's RID-specific tooling documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools)
and [ADR 0002](decisions/0002-dotnet-11-aot-tools.md).

CI retains the unpublished artifacts for review; a successful SDK-equipped
runner smoke test does **not** establish runtime independence. The isolated
Linux container probes this on an Ubuntu 24.04/glibc-compatible image; equivalent
independent consumer testing is still needed for Windows and macOS before a
release. Consumer verification must execute the binary without .NET on each
advertised OS and test the NuGet tool from a
clean source and tool path. Native AOT still depends on OS ABI and system
libraries; Linux builds target a compatible glibc baseline.

## CAP-008 unpublished distribution acceptance

The [CAP-008 implementation issue](https://github.com/infoconex/dotnet-binary-tools/issues/26)
adds repository-controlled checks that package each previously tested native
executable with its license using `scripts/package-standalone.ps1`, and
assemble the six native-platform artifacts with `scripts/assemble-distribution.ps1` ([expansion Issue #28](https://github.com/infoconex/dotnet-binary-tools/issues/28)).
`scripts/distribution_inventory.py` validates exactly six standalone archives,
six RID-native NuGet packages, and one top-level NuGet pointer, including
versions, source-commit metadata, commands, executable paths, native executable
modes, safe archive members and SHA-256 digests. These are unpublished CI
artifacts, not a release candidate accepted for public distribution.

The .NET 11 SDK's actual **v2 RID-aware tool format** stores the pointer-to-RID
mapping in `tools/any/any/DotnetToolSettings.xml`, under
`RuntimeIdentifierPackages`; it does **not** expose those references as ordinary
NuGet nuspec dependencies. Each native subpackage has its own
`tools/any/<rid>/DotnetToolSettings.xml` command/entry point. This was observed
in the [first CAP-008 three-OS CI artifacts](https://github.com/infoconex/dotnet-binary-tools/actions/runs/44031362119)
and confirmed with a local, offline audit of the actual four package files and
three archives. The initial aggregator validator's nuspec-dependency assumption
failed and was corrected; this is a repository-specific verification of
[Microsoft's documented RID-aware packaging model](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools).

The installation check now supplies a dedicated local-feed `nuget.config`
and an isolated package cache. `--add-source` alone would permit existing feeds
to race to supply the same package ID/version. Per-platform CI compares actual
same-fixture `inspect` and `scan` JSON for the installed tool and the standalone
binary after excluding only their distinct generation timestamps. The CI
aggregation step verifies the *complete* seven-package set rather than assuming
that a one-host local installation proved all RID references are present.

These checks do **not** satisfy Issue #4: native SDK/runtime-free Windows and
macOS consumers on both architectures remain separately required for v0.1 publication. Ordinary PR
CI does not contain a NuGet push, tag, release or public upload action.

## Future installation (not published)

```shell
dotnet tool install --global Infoconex.DotNetBinary.Tools
dotnet binary --help
```

Do not use the global install command as release evidence until all packages
have been published with owner authorization and the package ID is reserved.
PR CI is validation only, **never** a publication workflow.
