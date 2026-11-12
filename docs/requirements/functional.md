# Functional requirements — v0.1 index

**Status:** The 71 functional requirements across CAP-001–CAP-008 are approved for specification. **No functional acceptance has been demonstrated.**

The authoritative requirement titles and wording appear in the **Requirement coverage** table of each capability document. This page provides navigation without duplicating the individual requirements.

## Functional areas

Select a capability to review its approved requirements, or open its scenario index for the Given/When/Then acceptance scenarios.

| Capability | Requirement prefix | Functional area | Requirements | Acceptance scenarios |
| --- | --- | --- | ---: | --- |
| CAP-001 | `FR-INP-XXX` | [Input Selection](../capabilities/CAP-001.md) | 8 | [Scenarios](../scenarios/functional/input-selection/README.md) |
| CAP-002 | `FR-ASM-XXX` | [Assembly Inspection](../capabilities/CAP-002.md) | 11 | [Scenarios](../scenarios/functional/assembly-inspection/README.md) |
| CAP-003 | `FR-CFG-XXX` | [Deployment Configuration](../capabilities/CAP-003.md) | 8 | [Scenarios](../scenarios/functional/deployment-configuration/README.md) |
| CAP-004 | `FR-LCY-XXX` | [Lifecycle Assessment](../capabilities/CAP-004.md) | 8 | [Scenarios](../scenarios/functional/lifecycle-assessment/README.md) |
| CAP-005 | `FR-INV-XXX` | [Inspection Inventory](../capabilities/CAP-005.md) | 12 | [Scenarios](../scenarios/functional/inspection-inventory/README.md) |
| CAP-006 | `FR-RPT-XXX` | [Results and Reporting](../capabilities/CAP-006.md) | 9 | [Scenarios](../scenarios/functional/results-and-reporting/README.md) |
| CAP-007 | `FR-CLI-XXX` | [Command-Line Interface](../capabilities/CAP-007.md) | 8 | [Scenarios](../scenarios/functional/command-line-interface/README.md) |
| CAP-008 | `FR-DST-XXX` | [Distribution](../capabilities/CAP-008.md) | 7 | [Scenarios](../scenarios/functional/distribution/README.md) |

## Boundaries and interpretation

- CAP-001 validates explicitly supplied paths; CAP-002 classifies/inspects the file; CAP-003 interprets supported runtime configuration; CAP-004 provides informational lifecycle data; CAP-005 owns inventory coverage/aggregation; CAP-006 owns report generation; CAP-007 is a thin interface; CAP-008 owns product distribution.
- Paths must be validated before scanning. File-level failures and resource-limit stops after valid inputs produce incomplete results without retroactively invalidating the user's request.
- Never infer a missing target framework from CLR metadata, an executable host from a neighboring DLL, or a deployed/actually executing runtime from static configuration.
- Static lifecycle advisories do not assert application compliance and do not control success exit codes.
- Resource defaults, lifecycle freshness thresholds, canonical JSON property names and CLI option aliases still require explicit design/contract decisions before their implementation; see [open decisions](open-decisions.md).

[Capabilities](../capabilities/README.md) · [Architecture constraints](architecture.md) · [Scenario index](../scenarios/README.md) · [Traceability](../testing/traceability.md).
