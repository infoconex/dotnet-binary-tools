# Functional requirements — v0.1 index

**Status:** The 71 functional requirements across CAP-001–CAP-008 are approved for specification. **No functional acceptance has been demonstrated.**

The authoritative requirement titles and wording appear in the **Requirement coverage** table of each capability document. This page provides navigation without duplicating the individual requirements.

## Functional areas

Select a functional area to open its requirement-level Given/When/Then acceptance scenarios. The authoritative requirement wording remains in the [capability specifications](../capabilities/README.md).

| Capability | Requirement prefix | Functional area | Requirements |
| --- | --- | --- | ---: |
| [CAP-001](../capabilities/CAP-001.md) | `FR-INP-XXX` | [Input Selection](../scenarios/functional/input-selection/README.md) | 8 |
| [CAP-002](../capabilities/CAP-002.md) | `FR-ASM-XXX` | [Assembly Inspection](../scenarios/functional/assembly-inspection/README.md) | 11 |
| [CAP-003](../capabilities/CAP-003.md) | `FR-CFG-XXX` | [Deployment Configuration](../scenarios/functional/deployment-configuration/README.md) | 8 |
| [CAP-004](../capabilities/CAP-004.md) | `FR-LCY-XXX` | [Lifecycle Assessment](../scenarios/functional/lifecycle-assessment/README.md) | 8 |
| [CAP-005](../capabilities/CAP-005.md) | `FR-INV-XXX` | [Inspection Inventory](../scenarios/functional/inspection-inventory/README.md) | 12 |
| [CAP-006](../capabilities/CAP-006.md) | `FR-RPT-XXX` | [Results and Reporting](../scenarios/functional/results-and-reporting/README.md) | 9 |
| [CAP-007](../capabilities/CAP-007.md) | `FR-CLI-XXX` | [Command-Line Interface](../scenarios/functional/command-line-interface/README.md) | 8 |
| [CAP-008](../capabilities/CAP-008.md) | `FR-DST-XXX` | [Distribution](../scenarios/functional/distribution/README.md) | 7 |

## Boundaries and interpretation

- CAP-001 validates explicitly supplied paths; CAP-002 classifies/inspects the file; CAP-003 interprets supported runtime configuration; CAP-004 provides informational lifecycle data; CAP-005 owns inventory coverage/aggregation; CAP-006 owns report generation; CAP-007 is a thin interface; CAP-008 owns product distribution.
- Paths must be validated before scanning. File-level failures and resource-limit stops after valid inputs produce incomplete results without retroactively invalidating the user's request.
- Never infer a missing target framework from CLR metadata, an executable host from a neighboring DLL, or a deployed/actually executing runtime from static configuration.
- Static lifecycle advisories do not assert application compliance and do not control success exit codes.
- Resource defaults, lifecycle freshness thresholds, canonical JSON property names and CLI option aliases still require explicit design/contract decisions before their implementation; see [open decisions](open-decisions.md).

[Capabilities](../capabilities/README.md) · [Architecture constraints](architecture.md) · [Scenario index](../scenarios/README.md) · [Traceability](../testing/traceability.md).
