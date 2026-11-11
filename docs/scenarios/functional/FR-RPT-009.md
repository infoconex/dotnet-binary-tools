# FR-RPT-009: Render large inventories without requiring every serialized item in memory at once, while treating output failure as an explicit reporting error rather than a successful report.

**Capability:** [CAP-006 — Results and Reporting](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Render large inventories without requiring every serialized item in memory at once, while treating output failure as an explicit reporting error rather than a successful report.

## Acceptance scenarios

### SC-RPT-013: Stream a bounded-memory report

**Given** an inventory generates many candidates under configured limits

**When** JSON is serialized

**Then** memory does not grow by retaining a second complete serialized copy of every item.

### SC-RPT-014: Report output write failure

**Given** a destination becomes unwritable while serializing

**When** the report writer fails

**Then** the error is exposed to CAP-007 for a nonzero command outcome, without claiming a completed report.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
