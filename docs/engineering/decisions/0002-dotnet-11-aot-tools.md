# ADR 0002: .NET 11 and Native AOT as the distribution goal

**Status:** Proposed for review on the engineering-foundation branch.

## Context

Users should be able to inspect a legacy managed assembly without installing the targeted application's .NET runtime or the inspector's .NET 11 runtime.

## Decision

- Target `net11.0` for development and use `global.json` to constrain SDK selection to .NET 11.
- Configure the CLI as a .NET Tool with proposed NuGet package ID `Infoconex.DotNetFramework.Tools`, command `dotnet-framework`, and `dotnet framework` dispatch.
- Prefer **Native AOT** for standalone applications and RID-specific .NET Tool packages.
- Plan RIDs: `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`.
- Build AOT per host OS, test executable behavior and package selection, and prove runtime independence on clean machines before release.
- Use metadata APIs and source-generated JSON to maintain AOT compatibility.

## Tradeoffs and release constraint

AOT increases OS-specific compiler and packaging complexity. Microsoft requires `dotnet pack -r <RID>` for each AOT tool RID and publishing all RID packages before their top-level pointer package. Do not run package publishing in PR workflows.

The configuration declares six intended RIDs; **CI should not claim every one is verified** until dedicated runners and standalone consumer tests cover them. A managed `dotnet test` pass does not establish AOT compatibility.

Reference: [Microsoft RID-specific and Native AOT .NET Tools](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools).
