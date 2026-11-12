# FR-INP-002: Select directory roots

**Capability:** [CAP-001](../../../capabilities/CAP-001.md) · [Verification evidence](../../../testing/traceability.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Accept one or more explicit inventory roots.

## Acceptance scenarios

### SC-INP-003: Accept two directories

**Given** two existing directories

**When** the user supplies both to `scan`

**Then** both roots are accepted and forwarded without CAP-001 enumerating their contents.
