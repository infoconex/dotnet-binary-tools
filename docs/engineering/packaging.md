# Packaging and distribution plan

**Not released. NuGet package name has not been reserved or verified as publishable.**

- Proposed NuGet package ID: `Infoconex.DotNetFramework.Tools`.
- Installed executable: `dotnet-framework`.
- Invocations: `dotnet-framework --help` or `dotnet framework --help` after installation through the .NET CLI.
- Planned distribution: matching versioned RID-specific Native AOT NuGet tool packages and GitHub Releases standalone binaries.

Example **future** installation (not currently available):

```shell
dotnet tool install --global Infoconex.DotNetFramework.Tools
dotnet framework inspect MyLibrary.dll
```

Before publishing:
1. Confirm package-name availability and NuGet ownership and prefix policy.
2. Pass all FR/NFR acceptance and CI gates.
3. Build, inspect, and execute AOT artifacts on each advertised RID. Include a machine/container without the .NET 11 runtime in release acceptance.
4. Generate checksums and a Software Bill of Materials or dependency list as applicable; audit advisories and licenses.
5. Verify `dotnet tool install` and `dotnet framework` on clean consumers.
6. Publish all RID packages of the **same version** first, then the NuGet pointer package; publish only after explicit owner GO.

See [Microsoft RID-specific tool documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools).
