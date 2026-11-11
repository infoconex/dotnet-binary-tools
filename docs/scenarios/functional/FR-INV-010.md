# FR-INV-010: Aggregate accurate totals and distributions across managed, non-managed, unknown-target, failures, skipped paths, limits and applicable lifecycle findings.

**Capability:** [CAP-005 — Inspection Inventory](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Aggregate accurate totals and distributions across managed, non-managed, unknown-target, failures, skipped paths, limits and applicable lifecycle findings.

## Acceptance scenarios

### SC-INV-014: Reconcile counts

**Given** a scan produces managed, non-managed, unknown, errored and skipped candidates

**When** CAP-005 computes the summary

**Then** the summary counts agree with the recorded candidate events and do not double-count a result as both success and failure.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
