# FR-CLI-005: Return exit code zero for successfully generated valid reports, including scans with per-file errors or resource-limit partial coverage; return nonzero for invalid inputs or unrecoverable command/report errors.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Return exit code zero for successfully generated valid reports, including scans with per-file errors or resource-limit partial coverage; return nonzero for invalid inputs or unrecoverable command/report errors.

## Acceptance scenarios

### SC-CLI-007: Preserve success for a partial scan

**Given** a valid scan produces a completed report with per-file errors and incomplete coverage

**When** the CLI exits

**Then** exit code is zero and the report clearly marks its errors and incompleteness.

### SC-CLI-008: Reject invalid selected inputs

**Given** one of several explicitly supplied directories does not exist

**When** scan is invoked

**Then** validation returns a nonzero exit and no directory scan starts.
