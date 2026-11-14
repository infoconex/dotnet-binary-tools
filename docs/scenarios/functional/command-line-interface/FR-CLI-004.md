# FR-CLI-004: Validate unknown flags, missing option values, incompatible options and invalid input arity before starting costly processing; display actionable diagnostics.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Validate unknown flags, missing option values, incompatible options and invalid input arity before starting costly processing; display actionable diagnostics.

## Acceptance scenarios

### SC-CLI-005: Reject an unknown option

**Given** a command contains an unsupported named option

**When** the CLI parses it

**Then** the command fails with actionable error text and no inventory begins.

### SC-CLI-006: Reject conflicting formatting

**Given** --details and --fields are both supplied

**When** the CLI validates arguments

**Then** a nonzero validation result explains the conflict without producing misleading output.
