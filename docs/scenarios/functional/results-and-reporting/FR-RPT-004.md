# FR-RPT-004: Support explicit per-assembly field projection through --fields for human and JSON formats, preserving envelope metadata, totals and completion unchanged.

**Capability:** [CAP-006 — Results and Reporting](../../../capabilities/CAP-006.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Support explicit per-assembly field projection through --fields for human and JSON formats, preserving envelope metadata, totals and completion unchanged.

## Acceptance scenarios

### SC-RPT-005: Project only item fields

**Given** a report includes targets, references and versions and --fields selects name,version,targetFramework

**When** JSON output is generated

**Then** item payloads contain selected fields while schemaVersion, inputs, summaries and completion are unchanged.

### SC-RPT-006: Project fields in human mode

**Given** the same projection is requested for human output

**When** results are formatted

**Then** the selected assembly fields are shown and unrelated optional assembly fields are omitted.
