# FR-ASM-001: Statically inspect a selected file for PE/CLI metadata without loading, invoking, or executing the selected artifact.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Statically inspect a selected file for PE/CLI metadata without loading, invoking, or executing the selected artifact.

## Acceptance scenarios

### SC-ASM-001: Read a managed DLL

**Given** a valid .NET DLL whose metadata includes a CLI header

**When** CAP-002 inspects the file

**Then** the result identifies the file as managed and extracts its metadata without executing it.

### SC-ASM-002: Inspect a renamed managed assembly

**Given** a valid managed assembly renamed to an uncommon extension

**When** the file is passed from CAP-001 to CAP-002

**Then** the content is identified from its binary metadata rather than its filename extension.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
