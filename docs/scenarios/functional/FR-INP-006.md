# FR-INP-006: Require literal paths

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../capabilities.md)

[Functional requirements](../../requirements/functional.md)

## Purpose

Do not expand wildcard expressions in the application.

## Acceptance scenarios

### SC-INP-010: Do not expand an application-level wildcard

**Given** a literal wildcard argument not naming any existing path

**When** the unexpanded argument reaches the program

**Then** Input Selection validates that argument literally rather than expanding it into matches.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../testing/traceability.md).
