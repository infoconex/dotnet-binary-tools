# FR-RPT-007: Write human or JSON results to stdout by default, or to a user-selected report file with --output; reserve stderr for diagnostics.

**Capability:** [CAP-006 — Results and Reporting](../../../capabilities/CAP-006.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Write human or JSON results to stdout by default, or to a user-selected report file with --output; reserve stderr for diagnostics.

## Acceptance scenarios

### SC-RPT-009: Honor output destination

**Given** a valid report and a writable output path are provided

**When** --output is used

**Then** the report is written to disk and stdout is not polluted with report content.

### SC-RPT-010: Preserve pipeline output

**Given** the same report is emitted without --output

**When** JSON output is generated

**Then** stdout contains the report while warnings and diagnostics are written to stderr.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
