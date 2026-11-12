# FR-ASM-008: Read PE machine architecture and managed execution flags, distinguishing metadata facts from actual process bitness or runtime capabilities.

**Capability:** [CAP-002 — Assembly Inspection](../../capabilities/CAP-002.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Read PE machine architecture and managed execution flags, distinguishing metadata facts from actual process bitness or runtime capabilities.

## Acceptance scenarios

### SC-ASM-012: Report architecture facts

**Given** fixtures with distinct PE machine types and managed flags

**When** the fixtures are inspected

**Then** the recorded architecture facts match each file without asserting what bitness any running process uses.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
