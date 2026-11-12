# Microsoft-aligned C# coding standard

Project coding conventions for .NET 11, aligned with Microsoft's C# coding-style and identifier guidelines.

## Language and formatting

- Use SDK-style projects, C# supported by the selected .NET 11 SDK, nullable reference types and implicit usings.
- Use UTF-8, LF endings, four-space C# indentation and final newlines.
- Prefer file-scoped namespaces; one cohesive type per file; using directives outside namespaces.
- Require braces for control flow. Use `var` only when the type remains clear.
- Prefer explicit, immutable records and results when appropriate; do not introduce abstractions without a real seam or test benefit.
- Use culture-invariant machine output and predictable encoding when specifying on-disk formats.

## Naming

- Public type and member names: PascalCase.
- Interfaces: `I` + PascalCase.
- Parameters and local variables: camelCase.
- Private instance fields: `_camelCase`.
- Private static nonconstant fields: `s_camelCase`.
- All constants: PascalCase, regardless of accessibility.
- Namespaces reflect the compiled layers: `Infoconex.DotNetBinary.Domain`, `.Application`, `.Infrastructure`, `.Cli`, and their subnamespaces.

The root `.editorconfig` declares Roslyn naming rules. CI runs `dotnet format style --diagnostics IDE1006 --verify-no-changes`, not merely editor suggestions.

## APIs, robustness and security

- Document public APIs with useful XML comments, including inputs, errors and ownership.
- Use `nameof` for argument exceptions; preserve stable diagnostic codes and JSON field names.
- Keep Domain independent of Application, Infrastructure, CLI and NuGet packaging; define consumed ports in Application and prevent external metadata-reader types from leaking into inner contracts.
- Never load or execute inspected assemblies. Read-only metadata access is the security boundary.
- Treat PE files, metadata lengths and directory traversal as hostile; bound work and handle errors explicitly.
- Avoid ambient culture, random iteration order and nondeterministic path formatting in machine-readable output.
- Do not log binary contents, sensitive source data or unhandled vendor exception details.
- Maintain tests before altering public API/JSON output contracts.

## Verification

Builds treat warnings as errors. NUnit tests cover contract, boundary and error semantics. Follow [testing strategy](../testing/strategy.md) and [architecture](architecture.md). Comment why code is non-obvious rather than narrating each statement.
