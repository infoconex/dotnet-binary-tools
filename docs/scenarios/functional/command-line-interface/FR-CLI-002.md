# FR-CLI-002: Interpret inspect with one file and scan with one or more directories, delegating path validation to CAP-001 and inventory work to the application.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Interpret inspect with one file and scan with one or more directories, delegating path validation to CAP-001 and inventory work to the application.

## Acceptance scenarios

### SC-CLI-002: Reject invalid command arity

**Given** the user passes no file to inspect or no directory to scan

**When** the CLI parses arguments

**Then** it reports invalid usage with nonzero exit and does not start inspection.
