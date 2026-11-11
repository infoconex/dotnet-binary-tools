# dotnet-framework

**Inspect compiled .NET assemblies to identify their declared target frameworks, without source code or executing the assemblies.**

`dotnet-framework` is a planned cross-platform .NET CLI extension for inspecting managed DLL and EXE files. Its initial purpose is to answer a simple question:

> **Which .NET framework was this assembly built to target?**

This repository includes a **.NET 11 engineering foundation** and a help-only CLI. **Assembly inspection, directory scanning, and JSON reporting have not been implemented yet.** Commands and example results below describe intended behavior, not currently available functionality.

## Current engineering foundation

- **Target:** .NET 11, modern SDK-style C# projects, NUnit and cross-platform CI.
- **Distribution goal:** Native AOT executable and RID-specific NuGet Tool packages that do not require installing the .NET 11 runtime.
- **Proposed package ID:** `Infoconex.DotNetFramework.Tools` (availability not reserved).
- **CLI command:** `dotnet-framework` / `dotnet framework` when installed as a .NET Tool.
- **Implementation status:** `--help` and `--version` work in the scaffold. `inspect` and `scan` deliberately return an unimplemented error.

Engineering standards and the normative requirements are in [docs/README.md](docs/README.md), [the FR/NFR catalogs](docs/requirements/functional.md), [acceptance scenarios](docs/scenarios/README.md), and [traceability](docs/testing/traceability.md). The original twelve `REQ-###` identifiers below are maintained through an explicit mapping.

To build this **source-only** branch after installing the .NET 11 SDK:

```shell
dotnet restore DotNetFramework.sln
dotnet build DotNetFramework.sln -c Release --no-restore
dotnet test DotNetFramework.sln -c Release --no-build
dotnet run --project src/Infoconex.DotNetFramework.Cli -- --help
```

No package has been published. See [packaging](docs/engineering/packaging.md) for the planned distribution and verification gates.

## Why this tool?

A developer may have a compiled DLL or EXE but no project file, source repository, or reliable build records. Before planning modernization or an upgrade, they need to know whether an assembly declares a target such as:

- .NET Framework 4.8
- .NET Standard 2.0
- .NET 8, .NET 10, or .NET 11

The information is often available in the assembly's `System.Runtime.Versioning.TargetFrameworkAttribute`. Microsoft's IL Disassembler (`ildasm.exe`) can expose it, but the goal here is a simple, scriptable CLI that can inspect one file or inventory many files.

**Scope matters:** an assembly's *target framework* is not necessarily the runtime version actually executing an application. It is also not the SDK version used to compile the assembly. This tool must not conflate those concepts.

## Proposed CLI experience

The intended command name is `dotnet-framework`, which supports invocation through the .NET CLI as `dotnet framework` when installed as a .NET Tool.

```shell
dotnet framework inspect ./bin/MyLibrary.dll
dotnet framework inspect ./bin/MyApplication.exe
dotnet framework scan ./publish --recursive
dotnet framework scan ./publish --recursive --format json
dotnet framework --help
```

The command name and options are **proposed for v0.1** and should be verified before a public package is published.

Illustrative output for a single assembly:

```text
File:              MyLibrary.dll
Managed assembly:  Yes
Target framework:  .NET Framework 4.8
Framework moniker: .NETFramework,Version=v4.8
Detection source:  TargetFrameworkAttribute
```

Illustrative JSON output:

```json
{
  "file": "MyLibrary.dll",
  "managed": true,
  "targetFramework": {
    "moniker": ".NETFramework,Version=v4.8",
    "displayName": ".NET Framework 4.8",
    "source": "TargetFrameworkAttribute"
  }
}
```

Outputs must clearly distinguish **detected metadata** from an **unknown** or **inferred** result. The JSON shape is illustrative and will be finalized with automated contract tests.

## Original requirements: v0.1 (mapped to FR/NFR catalog)

The following original IDs remain for historical context. The authoritative specifications now use stable FR/NFR IDs in [the engineering catalog](docs/README.md).

| ID | Requirement |
| --- | --- |
| REQ-001 → FR-001 | Inspect a specified DLL or EXE file and determine whether it contains a readable managed .NET assembly. |
| REQ-002 → FR-002 | Read the assembly's declared `TargetFrameworkAttribute`, when present, **without loading or executing the assembly**. |
| REQ-003 → FR-003 | Preserve the original framework moniker and produce a human-readable framework name when it can be mapped reliably. |
| REQ-004 → FR-004 | Recognize common declared targets across .NET Framework, .NET Standard, and modern .NET (`.NETCoreApp`). Do not hard-code an upper major-version limit. |
| REQ-005 → FR-005 | Report a clear `unknown`, `not managed`, or `invalid/unreadable` result when the target cannot be established. Do not manufacture a target framework. |
| REQ-006 → FR-006 | Support one-file inspection through `dotnet framework inspect <path>`. |
| REQ-007 → FR-007 | Support directory inventory through `dotnet framework scan <directory>`, with optional recursive traversal of DLL and EXE files. |
| REQ-008 → FR-008 | Provide readable console output and machine-readable JSON output suitable for scripting or CI use. |
| REQ-009 → NFR-001 | Handle missing files, invalid binaries, permission failures, and malformed metadata with actionable errors, without terminating an entire directory scan because of one bad file. |
| REQ-010 → NFR-002 | Work on Windows, Linux, and macOS for supported managed assembly formats, without requiring the inspected application's runtime or source code. |
| REQ-011 → NFR-003 | Be packaged as a standard .NET Tool using the command name `dotnet-framework`, with help and version information. |
| REQ-012 → NFR-004 | Include automated tests covering successful detection, unknown targets, invalid inputs, CLI output, and directory scans. |

### Detection rules and limitations

1. **Prefer explicit assembly metadata.** The primary evidence is the assembly-level `TargetFrameworkAttribute`. Report the raw moniker so callers can see exactly what was declared.
2. **Do not infer a target from the CLR metadata version.** For example, `v4.0.30319` does not mean that an assembly targets .NET Framework 4.0; multiple .NET Framework releases use that CLR version.
3. **Handle missing attributes honestly.** Some legacy or specially compiled managed assemblies do not include `TargetFrameworkAttribute`. The initial tool reports the target as unknown rather than guessing from assembly references or API usage.
4. **Differentiate managed assemblies from native launchers.** A modern .NET `.exe` may be a native apphost rather than a managed assembly, and Native AOT output does not necessarily contain inspectable managed assembly metadata. The tool must not claim a target it cannot read. Investigation of adjacent `.dll` and `.runtimeconfig.json` files is a possible later enhancement.
5. **Treat declared metadata as evidence, not proof of build history.** The attribute can be customized, omitted, or inconsistent with packaging. Report what is actually present.
6. **Never execute an inspected artifact.** Inspection must remain read-only. Avoid `Assembly.Load`, `Assembly.LoadFrom`, or any approach requiring arbitrary application code to run.

A directory scan should not follow symbolic-link cycles. File count, file size, and other resource limits should be evaluated during implementation to keep scans reliable on large or untrusted directories.

## Acceptance scenarios for v0.1

| Scenario | Expected result |
| --- | --- |
| Inspect a managed assembly declaring `.NETFramework,Version=v4.8` | Reports .NET Framework 4.8 and preserves the original moniker. |
| Inspect a managed assembly declaring `.NETStandard,Version=v2.0` | Reports .NET Standard 2.0 and preserves the original moniker. |
| Inspect a managed assembly declaring `.NETCoreApp,Version=v10.0` | Reports .NET 10 and preserves the original moniker. |
| Inspect a managed assembly with no target-framework attribute | Reports target framework as unknown; does not use `v4.0.30319` as a substitute. |
| Inspect a native executable or an invalid file | Identifies it as non-managed or unreadable, as applicable, without executing it. |
| Recursively scan a directory containing mixed binaries | Produces one result per candidate file, including failures, and does not stop after one unreadable file. |
| Request JSON output | Produces parseable, deterministic fields with a documented schema; diagnostics do not corrupt standard output. |
| Run the same fixtures on supported operating systems | Produces equivalent detection results. |

Automated tests should use known fixture assemblies with explicitly documented targets. Tests must verify **the tool's output and its failure behavior**, not only that parsing completes.

## Engineering approach

- **Language and platform:** C# and a modern, supported .NET SDK. The tool's own target framework will be selected during implementation.
- **Metadata reader:** Prefer Microsoft's `System.Reflection.Metadata` and `System.Reflection.PortableExecutable` APIs to inspect binary metadata directly.
- **Architecture:** Keep binary inspection separate from CLI argument parsing, formatting, and directory traversal so the detection logic can be tested independently.
- **Portability:** Use cross-platform APIs and paths. Do not depend on Visual Studio or Windows-only SDK utilities for normal operation.
- **Safety:** Read input files without running or dynamically loading their code. Avoid changing or writing to inspected files.
- **Quality:** Use test-driven development, automated validation, clear errors, and GitHub Actions across supported environments.
- **Releases:** Version the CLI and JSON contract; document any breaking behavior before publishing packages.

## Out of scope for v0.1

These are **not** the same as target-framework detection and should not delay the first implementation:

- Determining the exact runtime patch currently executing an application.
- Detecting the SDK or C# compiler version originally used to build a binary.
- Claiming that an assembly is compatible with a particular runtime based only on its target moniker.
- Finding the transitive dependency graph or checking package vulnerabilities.
- Reading every possible deployment format, including Native AOT and bundled single-file executables.
- Making changes to assemblies, projects, or installed SDKs.
- Upgrading, retargeting, rebuilding, or migrating applications.

## Potential follow-on capabilities

Once v0.1 is accurate and tested, evaluate these separately:

- **Deployment context:** Read adjacent `*.runtimeconfig.json` and `*.deps.json` to report requested runtimes and dependencies, while distinguishing them from the assembly's declared target.
- **Support lifecycle:** Compare detected framework targets with Microsoft's current support metadata; display the evaluation date and source, and distinguish unsupported *targets* from the actual deployed runtime.
- **Inventory reporting:** Add CSV, richer JSON summaries, filtering, and optional CI policies.
- **Dependency analysis:** Identify referenced assemblies and incompatible or uncertain target combinations.
- **Native hosts and single-file deployments:** Investigate what can be determined reliably without executing the file.

These features are proposals, not commitments.

## Proposed implementation milestones

1. **Baseline:** Establish the .NET solution, CLI project, test project, fixtures, and GitHub Actions validation.
2. **Single-file inspection:** Implement metadata-only target-framework detection through tests for `inspect`.
3. **Inventory and reporting:** Implement `scan`, recursive traversal, robust errors, and structured JSON output.
4. **Package and document:** Publish a versioned .NET Tool package, command reference, and tested usage examples.

## Microsoft references

- [TargetFrameworkAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.versioning.targetframeworkattribute)
- [System.Reflection.Metadata](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata)
- [System.Reflection.PortableExecutable](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.portableexecutable)
- [IL Disassembler (Ildasm.exe)](https://learn.microsoft.com/en-us/dotnet/framework/tools/ildasm-exe-il-disassembler)
- [Create a .NET Tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create)
- [Manage .NET tools](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools)

---

**Project status:** .NET 11 engineering scaffold available for review; no functioning assembly inspector, standalone binaries or NuGet package has been released.
