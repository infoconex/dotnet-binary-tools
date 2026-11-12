# FR-INV-004: Produce independently classified per-candidate entries and do not group binaries into inferred applications or treat a native apphost as its neighboring DLL.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Produce independently classified per-candidate entries and do not group binaries into inferred applications or treat a native apphost as its neighboring DLL.

## Acceptance scenarios

### SC-INV-006: Keep mixed candidates independent

**Given** a folder contains a managed DLL, native EXE and malformed DLL

**When** a scan runs

**Then** three individually attributable results are recorded without inferred application grouping.
