# FR-ASM-005: Report a missing target-framework declaration as unknown and keep CLR metadata version separate; never infer a target from CLR version, references, APIs or filename.

**Capability:** [CAP-002 — Assembly Inspection](../../capabilities/CAP-002.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Report a missing target-framework declaration as unknown and keep CLR metadata version separate; never infer a target from CLR version, references, APIs or filename.

## Acceptance scenarios

### SC-ASM-008: Do not infer a target

**Given** a valid managed assembly without TargetFrameworkAttribute but with CLR metadata version and assembly references

**When** the file is inspected

**Then** the declared target is explicitly unknown and CLR metadata is reported only as a separate fact.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
