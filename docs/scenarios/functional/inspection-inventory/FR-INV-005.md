# FR-INV-005: Continue after per-candidate read, parse or permission failures, recording failure details and counts without discarding successful findings.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Continue after per-candidate read, parse or permission failures, recording failure details and counts without discarding successful findings.

## Acceptance scenarios

### SC-INV-007: Isolate a file failure

**Given** two readable candidates and one candidate that fails during reading

**When** the validated directory is scanned

**Then** both readable results and one failed result are retained in the same inventory.
