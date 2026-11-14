# FR-INV-007: When --follow-links is enabled, prevent traversal cycles and duplicate subtree visits and report what is skipped.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

When --follow-links is enabled, prevent traversal cycles and duplicate subtree visits and report what is skipped.

## Acceptance scenarios

### SC-INV-010: Prevent symbolic-link cycles

**Given** directory A links to B and B links back to A

**When** a recursive scan runs with --follow-links

**Then** processing terminates without repeated traversal, reports skips and preserves completed findings.
