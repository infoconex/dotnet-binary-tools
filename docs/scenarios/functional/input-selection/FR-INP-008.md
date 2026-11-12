# FR-INP-008: Keep input selection separate

**Capability:** [CAP-001](../../../capabilities/CAP-001.md) · [Verification evidence](../../../testing/traceability.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Do not read assembly metadata or enumerate directories while selecting inputs.

## Acceptance scenarios

### SC-INP-012: Do not inspect or traverse during selection

**Given** a valid file input and a valid directory input with observable inspection/traversal collaborators

**When** the application validates each explicit input

**Then** CAP-001 returns the validated input information without invoking content inspection or directory enumeration.
