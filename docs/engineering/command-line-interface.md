# CAP-007: CLI grammar and exit-code policy

[Approved capability](../capabilities/CAP-007.md) · [Acceptance evidence](../testing/traceability.md) · [Implementation Issue #24](https://github.com/infoconex/dotnet-binary-tools/issues/24)

This document records the technical selections that complete the approved CAP-007 command surface. The eight [approved requirement wordings](../capabilities/CAP-007.md#requirement-coverage) and eleven Given/When/Then scenarios remain authoritative; test evidence belongs solely in [traceability](../testing/traceability.md).

## Commands and invocation

The executable is `dotnet-binary`; the .NET Tool command is `dotnet binary` after installation. The CLI project already sets `PackAsTool=true`, `ToolCommandName=dotnet-binary`, and `AssemblyName=dotnet-binary`. [Microsoft's tool creation guide](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create) documents tool command names, and [Microsoft's troubleshooting guide](https://learn.microsoft.com/en-us/dotnet/core/tools/troubleshoot-usage-issues) documents the `dotnet-<name>` to `dotnet <name>` invocation convention. These settings do not establish CAP-008 publication or independent clean-consumer acceptance.

```shell
dotnet-binary inspect "./publish/My Assembly.dll" --format json --sha256
dotnet binary scan ./publish ./legacy --recursive --max-candidates 500 --format json
dotnet binary inspect ./app.dll --config ./app.runtimeconfig.json --details
dotnet binary --help
dotnet binary --version
dotnet binary lifecycle refresh
```

The root with no arguments displays help. `--help`, `-h`, and `help` display root help; `inspect --help`, `inspect -h`, `scan --help`, and `scan -h` also display it without selecting inputs. Root `--version` reads the CLI assembly's actual informational-version metadata, falling back to the assembly version. No hardcoded version identifier is displayed.

`inspect` requires exactly one literal file of any extension; `scan` requires one or more literal directory roots, discovering DLL/EXE candidates. CAP-001 validates all selected roots (and the inspect-only explicit configuration) as one request; a missing selected root prevents inventory work. No glob expansion is performed. Command and option names are case-sensitive.

## Options and defaults

| Option | Commands | Meaning and default |
| --- | --- | --- |
| `--recursive` | scan | Recurse, default off |
| `--follow-links` | scan | Follow discovered directory links, default off; independent of recursion |
| `--config <file>` | inspect | CAP-001-validated explicit configuration; unavailable for scan |
| `--format human\|json` | inspect, scan | `human` default |
| `--details` | inspect, scan | Expanded human output only; conflicts with JSON and `--fields` |
| `--fields <names>` | inspect, scan | Case-sensitive comma-separated per-assembly fields in the [v1 JSON contract](results-reporting.md); no duplicate or unknown names |
| `--output <new-file>` | inspect, scan | Exclusive new report file, using CAP-006 collision-safe fallback; stdout otherwise |
| `--sha256` | inspect, scan | Opt-in bounded SHA-256 for examined candidates, default off |
| `--refresh-lifecycle` | inspect, scan | Explicit online refresh after syntax and input validation, before inspection; default offline |
| `--max-depth N` | inspect, scan | Default 32, range 0–128 |
| `--max-candidates N` | inspect, scan | Default 10,000, range 1–100,000 |
| `--max-file-mib N` | inspect, scan | Default 512 MiB, range 1–1,024 MiB |
| `--max-elapsed-seconds N` | inspect, scan | Default 300, range 1–3,600 |
| `--max-cpu-seconds N` | inspect, scan | Default 300, range 1–3,600 |

The [CAP-005 inventory policy](inspection-inventory.md) defines cooperative resource-limit semantics, distinct from hard process isolation. CAP-006 defines projection, diagnostics, file allocation and [the immutable v1 JSON property spelling](results-reporting.md). Neither contract is changed by CAP-007.

Arguments may precede or follow options; shell quoting preserves spaces and unusual characters. `--` ends option processing and makes subsequent tokens literal paths, including paths beginning with `-`. For option values that themselves begin with `-`, use an explicit relative or absolute path (such as `./--configuration.json`). Unknown option spellings, repeated options, values that are missing or mistaken for other switches, numeric values outside documented ranges, wrong arity, unsupported command combinations, and incompatible report modes are errors before any filesystem selection, traversal, hashing or network refresh. No equals-form options, bundled short options, or aliases beyond help are accepted.

## Outcomes and streams

| Exit code | Meaning |
| --- | --- |
| `0` | Help/version; successful explicit refresh; completed report, **including** per-candidate failures or incomplete inventory coverage |
| `2` | Invalid command syntax, option, combination, input count, or CAP-001-selected root/configuration |
| `1` | Unrecoverable inventory orchestration, explicit refresh, reporting or output failure |

Successful inspect/scan reports go to stdout unless `--output` directs the complete report to an exclusive new file; diagnostics always use stderr. JSON stdout is exactly one report document without progress lines, logging or ANSI formatting. A failed write may leave an incomplete **new** file; no success is claimed and existing files are never overwritten. User-facing unexpected-failure messages exclude exception details and sensitive input contents. An explicit failed refresh leaves the last valid local lifecycle data untouched, produces a diagnostic and returns code 1 without inspecting.

The tool never executes or loads selected assemblies and does not infer installed or observed runtimes. Normal lifecycle assessment is offline, using bundled or validated cached data. Only `lifecycle refresh` and `--refresh-lifecycle` initiate the [bounded, allowlisted Microsoft HTTPS refresh](lifecycle-assessment.md). Parsing/help/version/input-count checks do not contact the network. These restrictions do not guarantee safe parsing of arbitrarily hostile binaries: resource limits are cooperative, not an OS sandbox.

## Architectural selection and research

The existing compact parser was extended rather than adding `System.CommandLine`. Its documented [syntax and arity conventions](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax) and [design guidance](https://learn.microsoft.com/en-us/dotnet/standard/commandline/design-guidance) informed the command contract; adopting an additional library is not justified for this narrow grammar, because it would introduce dependency, lockfile and AOT qualification work without a demonstrated need. Add a parser library only if future grammar complexity materially justifies it.

The CLI parses and validates typed requests, presents help/version/errors, and composes Application use cases behind `IInputSelectionUseCase`, `IInventoryUseCase` and `IReportUseCase`; the explicit lifecycle refresh delegates to CAP-004. Domain and Application remain free of CLI, filesystem, PE parsing and output adapters. CAP-005 passes the opt-in hash selection to the existing CAP-002 assembly-inspection service; CAP-006 continues to own all report rendering and file persistence. Tests substitute Application ports and exercise real managed processes; CI separately exercises actual Native AOT commands on the three x64 OSes.

## Compatibility and release boundaries

Option aliases and numeric ranges above are documented technical implementation decisions made in [Issue #24](https://github.com/infoconex/dotnet-binary-tools/issues/24), not retrospective changes to approved functional requirements. Post-release grammar changes require explicit compatibility review. CAP-008 distribution and publication, ARM64, SDK-free Windows/macOS consumer acceptance, and release gate [#4](https://github.com/infoconex/dotnet-binary-tools/issues/4) remain separate.
