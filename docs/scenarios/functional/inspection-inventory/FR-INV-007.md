# FR-INV-007: When --follow-links is enabled, prevent traversal cycles and duplicate subtree visits and report what is skipped.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

When --follow-links is enabled, prevent traversal cycles and duplicate subtree visits and report what is skipped.

## Acceptance scenarios

### SC-INV-010: Prevent symbolic-link cycles

**Given** directory A links to B and B links back to A

**When** a recursive scan runs with --follow-links

**Then** processing terminates without repeated traversal, reports skips and preserves completed findings.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
