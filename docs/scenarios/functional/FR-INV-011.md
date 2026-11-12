# FR-INV-011: Avoid duplicate candidate reporting from overlapping selected roots and followed links when the same physical path is encountered, recording deduplication.

**Capability:** [CAP-005 — Inspection Inventory](../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Avoid duplicate candidate reporting from overlapping selected roots and followed links when the same physical path is encountered, recording deduplication.

## Acceptance scenarios

### SC-INV-015: Deduplicate overlapping roots

**Given** the user selects a directory and one of its subdirectories for the same recursive scan

**When** inventory runs

**Then** a candidate reached by both roots appears once, with a documented duplicate or overlap count.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
