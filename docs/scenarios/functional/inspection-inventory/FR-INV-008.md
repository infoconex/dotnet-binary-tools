# FR-INV-008: Enforce documented configurable defaults and explicit bounds for traversal depth, candidate file count and file size, plus mandatory parsing/time/memory safeguards.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Enforce documented configurable defaults and explicit bounds for traversal depth, candidate file count and file size, plus mandatory parsing/time/memory safeguards.

## Acceptance scenarios

### SC-INV-011: Enforce candidate count limit

**Given** a scan root contains more valid candidates than the configured maximum

**When** scanning begins

**Then** the configured bound stops further candidate processing and the limit hit is recorded.

### SC-INV-012: Skip an oversized candidate

**Given** a candidate exceeds the configured maximum file size

**When** a scan processes its path

**Then** unsafe parsing is avoided and the item is marked with its size-limit reason.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
