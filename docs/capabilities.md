# Version 0.1 approved product capabilities

**Status: Approved.** Capability identifiers are stable; implementation is not yet complete.

| ID | Capability | Responsibility |
| --- | --- | --- |
| CAP-001 | Input Selection | Validate and resolve explicitly selected files and directories. |
| CAP-002 | Assembly Inspection | Statically inspect compiled binaries and report managed/non-managed classification, declared framework and available metadata without execution or unsupported inference. |
| CAP-003 | Deployment Configuration | Locate directly associated or explicitly selected runtime configuration and interpret requested runtimes separately from assembly targets. |
| CAP-004 | Lifecycle Assessment | Provide dated, source-backed, advisory lifecycle findings from bundled offline data with explicit refresh. |
| CAP-005 | Inspection Inventory | Coordinate single-file and directory inspections, safely traverse directories, aggregate findings and record completeness. |
| CAP-006 | Results and Reporting | Serialize a common structured report, present human output and safely persist reports. |
| CAP-007 | Command-Line Interface | Interpret commands/options, present help and diagnostics, and apply exit semantics without owning inspection behavior. |
| CAP-008 | Distribution | Deliver verified x64 standalone Native AOT executables and a NuGet .NET Tool on Windows, Linux and macOS. |

## Approved product boundaries

- `inspect` selects **one** regular file with any extension. `scan` selects **one or more** directories and enumerates DLL/EXE candidates. No application-side wildcard expansion. Invalid explicitly requested roots reject the whole request before scanning.
- Explicitly selected valid file/directory symlinks and Windows junctions are accepted. Recursive traversal skips directory links by default, with optional `--follow-links`, cycle detection and limits.
- Distinguish a **declared target**, a **configured runtime requirement**, and an **observed running runtime** (not assessed in v0.1). Missing framework declarations remain unknown: never infer from CLR metadata, references or adjacent files.
- Report non-managed DLL/EXE candidates rather than silently omitting them; do not speculate about native apphost associations.
- Initially inspect only directly associated `*.exe.config` and `*.runtimeconfig.json` or an explicitly chosen `--config`. No installed-runtime inventory, running-process observation or speculative application grouping.
- Assess .NET Framework, .NET Core and modern .NET lifecycle separately from assembly inspection, with provenance, date and uncertainty. .NET Standard is not itself an installed runtime.
- Directory scans continue across individual file failures. A valid report may have incomplete coverage due to failures or resource limits and still return exit code 0; invalid command inputs return nonzero.
- `inspect` and `scan` share a versioned JSON report envelope with metadata, summaries, completeness and per-file findings. `--fields` filters only assembly fields; default human output is concise, `--details` expands it.
- Output defaults to stdout; `--output` creates a report safely without overwriting. Existing names receive a UTC timestamp and collision suffix. Diagnostics go to stderr.
- Initial distribution is **win-x64, linux-x64, osx-x64**; ARM64 is deferred.

The eight capabilities are **what** the product provides, not a mandated class-per-capability design. Approved [architectural constraints](requirements/architecture.md) govern **how** it is built.

## Specification state

[All 71 v0.1 functional requirements](requirements/functional.md) across CAP-001–CAP-008 are approved for specification; [all 102 focused scenarios](scenarios/README.md) are defined but remain Planned until verified. [Scenarios](scenarios/README.md) and [traceability](testing/traceability.md) distinguish normative future behavior from actual executable evidence.
