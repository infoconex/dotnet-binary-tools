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

In .NET 10 and later, a RID-specific Native AOT tool uses **two package
steps**: `dotnet pack -r <rid>` creates the native subpackage on the matching
OS; `dotnet pack` without a RID creates the cross-RID pointer package.
All RID subpackages must use the same version as the pointer package. The
pointer package must be published **after** all required subpackages, not
before. See [Microsoft's RID-specific tooling documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools)
and [ADR 0002](decisions/0002-dotnet-11-aot-tools.md).

CI retains the unpublished artifacts for review; a successful SDK-equipped
runner smoke test does **not** establish runtime independence. Consumer
verification must separately execute the standalone binary where no .NET SDK
or runtime is installed, on each advertised OS, and test the NuGet tool from a
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
