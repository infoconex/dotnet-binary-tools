# FR-INV-010: Aggregate accurate totals and distributions across managed, non-managed, unknown-target, failures, skipped paths, limits and applicable lifecycle findings.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Aggregate accurate totals and distributions across managed, non-managed, unknown-target, failures, skipped paths, limits and applicable lifecycle findings.

## Acceptance scenarios

### SC-INV-014: Reconcile counts

**Given** a scan produces managed, non-managed, unknown, errored and skipped candidates

**When** CAP-005 computes the summary

**Then** the summary counts agree with the recorded candidate events and do not double-count a result as both success and failure.
