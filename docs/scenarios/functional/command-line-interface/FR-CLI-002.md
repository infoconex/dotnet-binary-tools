# FR-CLI-002: Interpret inspect with one file and scan with one or more directories, delegating path validation to CAP-001 and inventory work to the application.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Interpret inspect with one file and scan with one or more directories, delegating path validation to CAP-001 and inventory work to the application.

## Acceptance scenarios

### SC-CLI-002: Reject invalid command arity

**Given** the user passes no file to inspect or no directory to scan

**When** the CLI parses arguments

**Then** it reports invalid usage with nonzero exit and does not start inspection.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
