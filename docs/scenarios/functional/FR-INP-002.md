# FR-INP-002: Select directory roots

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../capabilities/CAP-001.md)

[Functional requirements](../../requirements/functional.md)

## Purpose

Accept one or more explicit inventory roots.

## Acceptance scenarios

### SC-INP-003: Accept two directories

**Given** two existing directories

**When** the user supplies both to `scan`

**Then** both roots are accepted and forwarded without CAP-001 enumerating their contents.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../testing/traceability.md).
