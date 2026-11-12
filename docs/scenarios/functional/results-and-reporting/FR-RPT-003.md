# FR-RPT-003: Emit exactly one syntactically valid JSON document in JSON mode with no diagnostic prose mixed into stdout.

**Capability:** [CAP-006 — Results and Reporting](../../../capabilities/CAP-006.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Emit exactly one syntactically valid JSON document in JSON mode with no diagnostic prose mixed into stdout.

## Acceptance scenarios

### SC-RPT-004: Keep JSON machine-readable

**Given** a scan has per-file errors and lifecycle warnings

**When** the user selects JSON output

**Then** stdout contains a single parseable JSON report and diagnostic text is not interleaved.
