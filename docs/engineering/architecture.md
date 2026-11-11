# Minimal metadata-inspection architecture

**Status:** Proposed architecture for the engineering foundation. See [ADR 0001](decisions/0001-core-cli-boundary.md).

```text
dotnet-framework / dotnet framework
           |
  Infoconex.DotNetFramework.Cli
      (arguments, output, JSON)
           |
           v
  Infoconex.DotNetFramework.Core
      (pure inspection contracts,
       PE / metadata reading)
```

## Project and test boundaries

- `src/Infoconex.DotNetFramework.Core`: metadata-only managed assembly detection, declared target moniker and normalized result. Uses Microsoft `System.Reflection.Metadata` and `PEReader` once FR implementation starts. No dependency on CLI.
- `src/Infoconex.DotNetFramework.Cli`: command routing, filesystem scanning, exit codes, console/JSON output, .NET Tool packaging; depends on Core. In this baseline only help and version operate.
- `tests/Infoconex.DotNetFramework.Core.Tests`: unit and boundary tests without CLI references.
- `tests/Infoconex.DotNetFramework.Cli.Tests`: CLI behavior and future integration/snapshot tests, referencing CLI and Core transitively.

Use readable, immutable result contracts and explicit status values. Do not force a full Domain/Application/Infrastructure assembly split unless requirements justify it.

## Security and Native AOT constraints

- Inspected binaries are data, never executable code; forbid `Assembly.Load`, `Assembly.LoadFrom`, `AssemblyLoadContext` for candidate binaries.
- Validate the PE container and metadata bounds before extracting attributes. Do not infer target framework from CLR runtime version.
- Preserve raw declared monikers and distinguish unknown from not-managed/corrupt/unreadable.
- Provide bounded scanning, explicit symlink policy, controlled output and safe diagnostic messages.
- Use source-generated JSON serializers for Native AOT compatibility once JSON output exists.
- A release is not portable merely because `dotnet build` passes. Each supported native RID needs observed AOT build and execution on its target OS.
