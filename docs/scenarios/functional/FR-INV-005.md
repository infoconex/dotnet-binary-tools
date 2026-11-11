# FR-INV-005: Continue after per-candidate read, parse or permission failures, recording failure details and counts without discarding successful findings.

**Capability:** [CAP-005 — Inspection Inventory](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Continue after per-candidate read, parse or permission failures, recording failure details and counts without discarding successful findings.

## Acceptance scenarios

### SC-INV-007: Isolate a file failure

**Given** two readable candidates and one candidate that fails during reading

**When** the validated directory is scanned

**Then** both readable results and one failed result are retained in the same inventory.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
