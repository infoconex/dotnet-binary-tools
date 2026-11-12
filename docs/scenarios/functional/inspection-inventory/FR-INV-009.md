# FR-INV-009: Return available findings and mark coverage incomplete when resource limits stop the scan, without treating the valid scan as a command failure.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Return available findings and mark coverage incomplete when resource limits stop the scan, without treating the valid scan as a command failure.

## Acceptance scenarios

### SC-INV-013: Report bounded partial inventory

**Given** a scan reaches its configured depth or file-count limit

**When** the inventory is finalized

**Then** its completion state is incomplete with specific limit reasons, retained findings and a normal success code for a valid report.
