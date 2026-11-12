# v0.1 product capabilities

**Specification status:** All eight capabilities are approved for v0.1. **Implementation status:** The inspection features and functional acceptance remain Planned, not yet verified.

This README is the index for the individual capability specifications. Each capability document explains its purpose, approved behavior, inputs and outputs, exclusions, failure/completeness semantics, relationships to other capabilities, and linked requirements. It is a behavioral boundary, **not** a requirement to create one class or one implementation project per capability.

| ID | Capability | Responsibility |
| --- | --- | --- |
| [CAP-001](CAP-001.md) | Input Selection | Validate and resolve explicitly selected files and directories. |
| [CAP-002](CAP-002.md) | Assembly Inspection | Statically inspect compiled binaries and report managed/non-managed classification, declared framework and available metadata without execution or unsupported inference. |
| [CAP-003](CAP-003.md) | Deployment Configuration | Locate directly associated or explicitly selected runtime configuration and interpret requested runtimes separately from assembly targets. |
| [CAP-004](CAP-004.md) | Lifecycle Assessment | Provide dated, source-backed, advisory lifecycle findings from bundled offline data with explicit refresh. |
| [CAP-005](CAP-005.md) | Inspection Inventory | Coordinate single-file and directory inspections, safely traverse directories, aggregate findings and record completeness. |
| [CAP-006](CAP-006.md) | Results and Reporting | Serialize a common structured report, present human output and safely persist reports. |
| [CAP-007](CAP-007.md) | Command-Line Interface | Interpret commands/options, present help and diagnostics, and apply exit semantics without owning inspection behavior. |
| [CAP-008](CAP-008.md) | Distribution | Deliver verified x64 standalone Native AOT executables and a NuGet .NET Tool on Windows, Linux and macOS. |

## Shared product boundaries

- The tool inspects selected files **without loading or executing them**. `inspect` requires one explicit regular file of any extension; `scan` requires one or more directories and discovers DLL/EXE candidates. Scanning is non-recursive unless opted into.
- Declared assembly targets, configured deployment runtime requests, and actual observed running runtimes are different facts. v0.1 does not observe installed or running runtimes; it reports unknowns rather than guessing.
- Directory inventories preserve individual failures, skipped paths, resource limits, and whether coverage is complete. A successful report can still describe partial coverage; invalid explicitly supplied inputs fail the request.
- Results share an evidence-preserving, versioned report contract. Lifecycle advice uses dated, source-backed data and is offline by default.
- The initial distribution goal is Windows, Linux, and macOS x64; ARM64, package publishing, and release acceptance require separate evidence and authorization.

## Specification authority and verification

The **Requirement coverage** tables in these eight capability files define all 71 approved functional requirements; the [functional requirements index](../requirements/functional.md) links to them. Their linked [102 Given/When/Then acceptance scenarios](../scenarios/README.md) define acceptance criteria; individual scenarios remain **Planned** until evidence appears in [traceability](../testing/traceability.md). The approved [architectural constraints](../requirements/architecture.md) govern implementation independently of the capabilities. Open numerical and contract choices are tracked in [open decisions](../requirements/open-decisions.md).
