# CAP-005: inspection inventory resource and traversal policy

[Approved capability](../capabilities/CAP-005.md) · [Acceptance evidence](../testing/traceability.md) · [Implementation Issue #20](https://github.com/infoconex/dotnet-binary-tools/issues/20)

## Implementation selections — November 15, 2026 (Pacific)

These numerical values and option names are technical implementation choices made for CAP-005; they are **not** retrospective approvals or changes to FR-INV-001–012.

| Resource | Default | Permitted override | Meaning |
| --- | ---: | --- | --- |
| Depth | 32 | `--max-depth` 0–128 | Selected root is depth zero; skipped descendants mark coverage incomplete |
| Unique candidates | 10,000 | `--max-candidates` 1–100,000 | Duplicates do not consume the candidate budget; the first additional unique candidate stops discovery and records the limit |
| Candidate size | 512 MiB | `--max-file-mib` 1–1,024 | An oversized candidate is recorded as resource-limited without invoking its parser |
| Elapsed budget | 300 seconds | `--max-elapsed-seconds` 1–3,600 | Cooperative checks between enumerated entries and candidates |
| CPU budget | 300 seconds | `--max-cpu-seconds` 1–3,600 | Cooperative checks using elapsed process CPU time |

CAP-002 independently enforces its **64 MiB metadata cap** and bounded streaming hash reads; CAP-005 passes its per-file byte limit through the existing assembly-inspection Application service. A scan also limits queued/visited directory work and recorded skip/error events (at most 100,000 combined events, with a smaller bound for lower candidate budgets). Inventory entries stay in a list bounded by the candidate limit. A `scan` is non-recursive by default; `--recursive` and `--follow-links` are independent opt-ins. `inspect` allows any explicitly selected file extension.

These are minimal CAP-005 CLI bindings, not the final CAP-007 command-language or CAP-006 report/schema contract.

## Rationale and tradeoffs

- [Microsoft's lazy filesystem enumeration](https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.enumeratefilesysteminfos) permits bounded discovery without materializing an entire directory before processing. [EnumerationOptions](https://learn.microsoft.com/en-us/dotnet/api/system.io.enumerationoptions) can ignore inaccessible entries, but doing so would conceal incomplete coverage. Individual directory traversals expose failures explicitly instead.
- [ResolveLinkTarget](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesysteminfo.resolvelinktarget) resolves Windows junctions and Unix symbolic links; canonical target paths and platform-appropriate comparison prevent discovered cycles and duplicate visits. Following discovered directory links is opt-in; explicitly selected linked roots remain valid.
- Directory-depth and candidate-count limits provide predictable finite traversal work; a single fixed byte limit bounds candidate input size. Rejected alternatives include unrestricted recursive enumeration, eager sorting of an entire directory (unbounded memory), and filename-only deduplication (incorrect across linked aliases). Platform-native inode/device-based hard-link deduplication was not introduced; identity here means normalized resolved physical *path*.

## Completeness and security limitations

A limit, denied traversal, candidate failure, or skipped linked directory is observable in the inventory and never silently reported as complete. Valid partial inventories retain successful findings and keep a successful CLI exit; invalid selected roots remain CAP-001 selection failures. Duplicate detection is recorded separately and does not itself imply missing coverage.

The limits are **cooperative**, not preemptive. A blocking filesystem call or parser operation can exceed the elapsed/CPU budget before the next check. [Microsoft cautions that `PEReader` and `System.Reflection.Metadata` are not hardened for hostile PE content](https://learn.microsoft.com/en-us/dotnet/standard/assembly/file-format). Reusing CAP-002 avoids executing or loading candidate assemblies, but does not create an OS-isolated parser sandbox or guarantee a hard memory/CPU ceiling. Inspected files may also change between the size probe and read; CAP-002 enforces its own read-side limits. This residual risk should be evaluated before treating the tool as safe for arbitrarily hostile binaries.

Windows/Linux/macOS filesystem and Native AOT behavior is separately evidenced in [traceability](../testing/traceability.md) and the linked PR, not asserted solely from compilation.
