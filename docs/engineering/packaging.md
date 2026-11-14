# Packaging and distribution plan

**CAP-008 approved; no public release.** The proposed NuGet ID
`Infoconex.DotNetBinary.Tools` is not reserved, and the project version
`0.1.0-preview.1` is not an actual published release.

Initial delivery includes a standalone Native AOT executable and a NuGet
.NET Tool, supporting **linux-x64, win-x64 and osx-x64** (not ARM64). The tool
exposes `dotnet-binary` and, after tool installation, `dotnet binary`.

## Reproducible local pre-release checks

Install .NET 11, PowerShell 7, NuGet access and the native compiler prerequisites
for the matching x64 host. In the repository root:

```shell
pwsh ./scripts/validate-aot.ps1 -RuntimeIdentifier linux-x64
pwsh ./scripts/validate-tool-package.ps1 -RuntimeIdentifier linux-x64
```

Use `win-x64` on Windows x64 or `osx-x64` on Intel macOS. The first script
publishes an AOT executable and smoke-tests `--help`/`--version`. The second
builds both the matching Native AOT RID package and the top-level pointer
package, then installs the latter into a fresh private `--tool-path` from the
local package source. It tests the installed shim and `dotnet binary`.
Neither script pushes artifacts or updates a global tool installation.
`scripts/write-artifact-checksums.ps1` writes SHA-256 manifests for generated
artifact directories. On Linux, provision the `ubuntu:24.04` container image
separately, then run `scripts/validate-linux-runtime-free.ps1` to verify the
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

## Future installation (not published)

```shell
dotnet tool install --global Infoconex.DotNetBinary.Tools
dotnet binary --help
```

Do not use the global install command as release evidence until all packages
have been published with owner authorization and the package ID is reserved.
PR CI is validation only, **never** a publication workflow.
