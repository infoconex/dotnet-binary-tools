# FR-INV-003: Scan only selected directories by default, with explicit opt-in recursion; do not discover unrelated drives, processes or application installations.

**Capability:** [CAP-005 — Inspection Inventory](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Scan only selected directories by default, with explicit opt-in recursion; do not discover unrelated drives, processes or application installations.

## Acceptance scenarios

### SC-INV-004: Do not recurse by default

**Given** a selected root has a top-level DLL and a nested DLL

**When** scan runs without --recursive

**Then** only the top-level candidate is visited.

### SC-INV-005: Recurse when requested

**Given** the same fixture has a nested candidate

**When** scan runs with --recursive

**Then** nested candidates within the selected root are visited.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
