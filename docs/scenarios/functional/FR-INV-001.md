# FR-INV-001: Coordinate an inventory result for both an individual inspect request and a multi-root scan using shared aggregation and completion semantics.

**Capability:** [CAP-005 — Inspection Inventory](../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Coordinate an inventory result for both an individual inspect request and a multi-root scan using shared aggregation and completion semantics.

## Acceptance scenarios

### SC-INV-001: Inventory an individual inspection

**Given** a valid explicitly selected file

**When** the inspect use case runs

**Then** one inventory report containing one candidate and the common completion structure is produced.

### SC-INV-002: Inventory multiple roots

**Given** two validated scan roots

**When** the scan use case runs

**Then** a single inventory report spans both roots while preserving each selected input.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
