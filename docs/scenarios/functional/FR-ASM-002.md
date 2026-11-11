# FR-ASM-002: Classify managed assembly, native/non-managed candidate, corrupt/unrecognized file, and unreadable file as distinguishable outcomes.

**Capability:** [CAP-002 — Assembly Inspection](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Classify managed assembly, native/non-managed candidate, corrupt/unrecognized file, and unreadable file as distinguishable outcomes.

## Acceptance scenarios

### SC-ASM-003: Classify a native executable

**Given** a native executable with no CLI metadata

**When** CAP-002 inspects the file

**Then** the outcome is non-managed rather than an invented .NET target.

### SC-ASM-004: Differentiate bad data and I/O failure

**Given** one truncated file and a separate file that fails during reading

**When** each is inspected

**Then** the outcomes distinguish malformed or unrecognized content from an I/O failure without crashing the process.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
