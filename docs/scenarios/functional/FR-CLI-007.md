# FR-CLI-007: Expose documented usage and version information, including safety constraints, defaults, output formats and the explicit online refresh option.

**Capability:** [CAP-007 — Command-Line Interface](../../capabilities/CAP-007.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Expose documented usage and version information, including safety constraints, defaults, output formats and the explicit online refresh option.

## Acceptance scenarios

### SC-CLI-010: Provide usable help

**Given** the tool is installed on a supported system

**When** --help is invoked

**Then** documented command signatures, important defaults, and report/refresh options are available without network access or file inspection.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
