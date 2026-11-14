# FR-INV-003: Scan only selected directories by default, with explicit opt-in recursion; do not discover unrelated drives, processes or application installations.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

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
