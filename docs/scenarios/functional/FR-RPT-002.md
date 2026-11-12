# FR-RPT-002: Provide concise human-readable output by default and expanded metadata output under --details without changing the underlying inspection.

**Capability:** [CAP-006 — Results and Reporting](../../capabilities/CAP-006.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Provide concise human-readable output by default and expanded metadata output under --details without changing the underlying inspection.

## Acceptance scenarios

### SC-RPT-002: Concise human output

**Given** a candidate has extensive optional metadata

**When** the user selects default human formatting

**Then** only core findings are presented clearly.

### SC-RPT-003: Expanded human output

**Given** the same candidate is reported with --details

**When** the report is formatted

**Then** available optional metadata, provenance and diagnostics are shown without made-up values.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
