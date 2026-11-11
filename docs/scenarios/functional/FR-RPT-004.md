# FR-RPT-004: Support explicit per-assembly field projection through --fields for human and JSON formats, preserving envelope metadata, totals and completion unchanged.

**Capability:** [CAP-006 — Results and Reporting](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

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

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
