# FR-INP-007: Provide actionable diagnostics

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../capabilities/CAP-001.md)

[Functional requirements](../../requirements/functional.md)

## Purpose

Distinguish invalid explicit requests from discovered inspection errors.

## Acceptance scenarios

### SC-INP-005: Identify a missing input

**Given** a nonexistent explicit file or directory path

**When** the user submits that path for inspection or scanning

**Then** the request is rejected with a diagnostic that identifies the input and why it is invalid.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../testing/traceability.md).
