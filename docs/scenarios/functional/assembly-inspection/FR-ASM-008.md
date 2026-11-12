# FR-ASM-008: Read PE machine architecture and managed execution flags, distinguishing metadata facts from actual process bitness or runtime capabilities.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Read PE machine architecture and managed execution flags, distinguishing metadata facts from actual process bitness or runtime capabilities.

## Acceptance scenarios

### SC-ASM-012: Report architecture facts

**Given** fixtures with distinct PE machine types and managed flags

**When** the fixtures are inspected

**Then** the recorded architecture facts match each file without asserting what bitness any running process uses.
