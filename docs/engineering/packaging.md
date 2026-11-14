# Packaging and distribution plan

**CAP-008 approved; no public release.** The proposed NuGet ID `Infoconex.DotNetBinary.Tools` is not reserved.

Initial delivery includes both a standalone Native AOT executable and a NuGet .NET Tool with the same commands (`dotnet-binary` and, after tool installation, `dotnet binary`). Advertised platforms in v0.1 are **win-x64, linux-x64 and osx-x64**, not ARM64.

Future installation example (package not currently published):

```shell
dotnet tool install --global Infoconex.DotNetBinary.Tools
dotnet binary --help
```

Release acceptance needs native build, execution and clean consumer verification without installed .NET, independent tool installation on each supported OS, checksums and provenance, dependency audit, and explicit owner GO. RID-specific packages must have matching versions and be published before the top-level package. Do not publish from PR CI.

See [ADR 0002](decisions/0002-dotnet-11-aot-tools.md) and [Microsoft RID-specific tools](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools).
