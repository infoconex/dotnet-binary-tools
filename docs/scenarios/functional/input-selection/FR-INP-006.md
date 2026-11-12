# FR-INP-006: Require literal paths

**Capability:** [CAP-001](../../../capabilities/CAP-001.md) · [Verification evidence](../../../testing/traceability.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Do not expand wildcard expressions in the application.

## Acceptance scenarios

### SC-INP-010: Do not expand an application-level wildcard

**Given** a literal wildcard argument not naming any existing path

**When** the unexpanded argument reaches the program

**Then** Input Selection validates that argument literally rather than expanding it into matches.
