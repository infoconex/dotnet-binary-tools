# FR-INP-008: Keep input selection separate

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../capabilities/CAP-001.md)

[Functional requirements](../../requirements/functional.md)

## Purpose

Do not read assembly metadata or enumerate directories while selecting inputs.

## Acceptance scenarios

### SC-INP-012: Do not inspect or traverse during selection

**Given** a valid file input and a valid directory input with observable inspection/traversal collaborators

**When** the application validates each explicit input

**Then** CAP-001 returns the validated input information without invoking content inspection or directory enumeration.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../testing/traceability.md).
