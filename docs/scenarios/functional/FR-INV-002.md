# FR-INV-002: Select DLL and EXE candidates during scanning by filename extension, using consistent ASCII case-insensitive extension matching without reading unrelated files.

**Capability:** [CAP-005 — Inspection Inventory](../../capabilities/CAP-005.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Select DLL and EXE candidates during scanning by filename extension, using consistent ASCII case-insensitive extension matching without reading unrelated files.

## Acceptance scenarios

### SC-INV-003: Select expected extensions

**Given** a directory contains .dll, .DLL, .exe, .EXE, .txt and .bin files

**When** a scan runs

**Then** DLL/EXE candidates are selected and unrelated extensions are excluded, without applying this restriction to inspect.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
