# FR-INV-006: Skip discovered directory symbolic links and junctions by default, but honor explicitly selected linked roots regardless of the recursion setting.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Skip discovered directory symbolic links and junctions by default, but honor explicitly selected linked roots regardless of the recursion setting.

## Acceptance scenarios

### SC-INV-008: Default link behavior

**Given** a valid selected root contains a directory link to another subtree

**When** a recursive scan runs without --follow-links

**Then** the discovered linked subtree is skipped and the skip is tracked.

### SC-INV-009: Honor a linked selected root

**Given** an explicitly selected root is itself a valid directory junction

**When** scan runs with that root and without --follow-links

**Then** the directly selected root is accepted and its normal contents are scanned.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
