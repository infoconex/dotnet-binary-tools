# ADR 0002: .NET 11 and Native AOT distribution

**Status: Approved distribution goal.**

The tool targets `net11.0` in source and supports **both** standalone Native AOT executables and the NuGet .NET Tool (`Infoconex.DotNetBinary.Tools`, executable `dotnet-binary`, `dotnet binary` invocation).

**Initial release RIDs:** `win-x64`, `linux-x64`, `osx-x64`. ARM64 is deferred pending separate evidence. The same feature contract must be available through both distributions.

Each advertised RID requires real native publication, execution, and NuGet package installation tests on its corresponding OS, plus a clean consumer test proving the standalone tool does not need the .NET runtime. A managed build, declared RID or AOT help smoke test on a runner with .NET installed is not proof of runtime independence.

Package publishing and public releases require separate owner approval. See [Microsoft RID-specific .NET Tools](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools).
