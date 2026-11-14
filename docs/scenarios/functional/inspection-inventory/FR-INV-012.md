# FR-INV-012: Record the list of validated roots, traversal errors and explicit completion reasons so consumers can distinguish completed, partial and failed inventory work.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Record the list of validated roots, traversal errors and explicit completion reasons so consumers can distinguish completed, partial and failed inventory work.

## Acceptance scenarios

### SC-INV-016: Describe incomplete coverage

**Given** a valid scan encounters a denied subdirectory but completes other roots

**When** the inventory is finalized

**Then** the report names the affected root or traversal error, provides partial coverage status and preserves successful results.
