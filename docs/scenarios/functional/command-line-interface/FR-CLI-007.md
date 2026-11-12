# FR-CLI-007: Expose documented usage and version information, including safety constraints, defaults, output formats and the explicit online refresh option.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Expose documented usage and version information, including safety constraints, defaults, output formats and the explicit online refresh option.

## Acceptance scenarios

### SC-CLI-010: Provide usable help

**Given** the tool is installed on a supported system

**When** --help is invoked

**Then** documented command signatures, important defaults, and report/refresh options are available without network access or file inspection.
