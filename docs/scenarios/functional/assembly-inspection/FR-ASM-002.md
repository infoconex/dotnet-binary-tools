# FR-ASM-002: Classify managed assembly, native/non-managed candidate, corrupt/unrecognized file, and unreadable file as distinguishable outcomes.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md) · [Verification evidence](../../../testing/traceability.md)

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
