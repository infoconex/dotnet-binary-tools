# FR-CLI-006: Write report data to stdout by default, diagnostics to stderr, avoid ANSI/progress noise in machine-readable output, and support --output without corrupting pipelines.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Write report data to stdout by default, diagnostics to stderr, avoid ANSI/progress noise in machine-readable output, and support --output without corrupting pipelines.

## Acceptance scenarios

### SC-CLI-009: Protect JSON pipelines

**Given** a successful JSON scan also produces warnings

**When** the CLI renders the response

**Then** stdout has parseable JSON only, warnings are on stderr, and the success exit code reflects the command contract.
