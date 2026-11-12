# .NET Binary Tools

**A planned cross-platform CLI for static inspection of compiled .NET binaries, declared targets, requested deployment runtimes and informational framework lifecycle findings.**

The planned inspection covers **.NET Framework, .NET Core, modern .NET, and .NET Standard metadata**; the name refers to compiled binaries, not to any one .NET runtime family.

**Implementation status:** .NET 11 engineering scaffold only. CLI help/version and a small domain value object are implemented. **`inspect`, `scan`, configuration inspection, lifecycle assessment and JSON reporting are not implemented or released.** Commands below describe the approved v0.1 design, not working behavior.

## Approved specification

- [CAP-001–CAP-008: product capabilities](docs/capabilities/README.md).
- [ARCH-001–ARCH-008: mandatory SOLID and hexagonal architecture](docs/requirements/architecture.md).
- [Functional requirements index (71 approved)](docs/requirements/functional.md), [102 focused Given/When/Then scenarios](docs/scenarios/README.md), and [scenario verification status](docs/testing/traceability.md).
- [Engineering and development documentation](docs/README.md).

### Intended CLI (not implemented)

```shell
dotnet binary inspect ./bin/MyLibrary.dll
dotnet binary inspect ./bin/MyApplication.exe --details
dotnet binary scan ./publish ./legacy --recursive
dotnet binary scan ./publish --format json --output inventory.json
dotnet binary inspect ./bin/MyLibrary.dll --format json --fields name,version,targetFramework
```

`inspect` will accept one explicit regular file regardless of extension; `scan` will accept one or more explicitly selected directories and inventory DLL/EXE candidates. Invalid explicitly selected roots will reject the entire command. The app will not expand wildcard patterns. Explicit valid links are accepted, while directory links found recursively are skipped by default (`--follow-links` is opt-in).

Results will separate declared assembly target, requested deployment runtime, and actual observed runtime (not measured in v0.1), report non-managed binaries and unknown targets without guessing, and provide offline, dated lifecycle findings. Scans will preserve per-file errors and partial/incomplete coverage, including when resource limits are reached.

Both commands will share one versioned JSON report envelope; `--fields` will filter only per-assembly properties. Results will go to stdout by default or `--output`, using non-overwriting UTC timestamped collision-safe filenames. Diagnostics go to stderr.

Initial release goal: both standalone Native AOT executables and NuGet .NET Tool (`Infoconex.DotNetBinary.Tools`) on **Windows x64, Linux x64, macOS x64**; ARM64 is deferred. No NuGet package or standalone executable has been published.

## Engineering scaffold

The architecture requires distinct compiled **Domain, Application, Infrastructure and CLI** layers, with compile-time project references pointing inward. The CLI is an outer adapter and composition root. No implementation of an inspector is claimed by creating the projects.

```shell
dotnet restore DotNetBinary.slnx --configfile nuget.config
dotnet build DotNetBinary.slnx -c Release --no-restore
dotnet test DotNetBinary.slnx -c Release --no-build
dotnet run --project src/Infoconex.DotNetBinary.Cli -- --help
```

Commands above are build instructions, not verified run evidence in this document. Source files must be safe for inspection: do not load or execute selected binaries.

## Sources

- [Microsoft TargetFrameworkAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.versioning.targetframeworkattribute)
- [System.Reflection.Metadata](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.metadata)
- [Native AOT .NET tools](https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools)
