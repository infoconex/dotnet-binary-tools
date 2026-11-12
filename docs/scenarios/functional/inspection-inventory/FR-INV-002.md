# FR-INV-002: Select DLL and EXE candidates during scanning by filename extension, using consistent ASCII case-insensitive extension matching without reading unrelated files.

**Capability:** [CAP-005 — Inspection Inventory](../../../capabilities/CAP-005.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Select DLL and EXE candidates during scanning by filename extension, using consistent ASCII case-insensitive extension matching without reading unrelated files.

## Acceptance scenarios

### SC-INV-003: Select expected extensions

**Given** a directory contains .dll, .DLL, .exe, .EXE, .txt and .bin files

**When** a scan runs

**Then** DLL/EXE candidates are selected and unrelated extensions are excluded, without applying this restriction to inspect.
