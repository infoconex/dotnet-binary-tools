# FR-CLI-008: Remain a thin adapter/composition root that invokes Application use cases through interfaces rather than implementing binary parsing, lifecycle rules or inventory logic.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Remain a thin adapter/composition root that invokes Application use cases through interfaces rather than implementing binary parsing, lifecycle rules or inventory logic.

## Acceptance scenarios

### SC-CLI-011: Keep logic out of CLI

**Given** application use cases are substituted with controlled test doubles

**When** CLI command parsing and dispatch are tested

**Then** the CLI forwards typed inputs and handles resulting output/exit behavior without performing metadata reads or traversal itself.
