# FR-INV-004: Produce independently classified per-candidate entries and do not group binaries into inferred applications or treat a native apphost as its neighboring DLL.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Produce independently classified per-candidate entries and do not group binaries into inferred applications or treat a native apphost as its neighboring DLL.

## Acceptance scenarios

### SC-INV-006: Keep mixed candidates independent

**Given** a folder contains a managed DLL, native EXE and malformed DLL

**When** a scan runs

**Then** three individually attributable results are recorded without inferred application grouping.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
