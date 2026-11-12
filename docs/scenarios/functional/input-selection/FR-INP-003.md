# FR-INP-003: Resolve file system paths

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../../capabilities/CAP-001.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Use current-directory and host-platform path rules.

## Acceptance scenarios

### SC-INP-004: Resolve a relative path

**Given** an existing file below the current working directory

**When** the user supplies its relative path to `inspect`

**Then** the input resolves to the correct file under the host platform's path semantics.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../../testing/traceability.md).
