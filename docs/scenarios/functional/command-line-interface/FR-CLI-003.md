# FR-CLI-003: Expose previously approved options for recursion, link traversal, configuration selection, format, details, fields, output, resource limits, optional hashes and explicit lifecycle refresh.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Expose previously approved options for recursion, link traversal, configuration selection, format, details, fields, output, resource limits, optional hashes and explicit lifecycle refresh.

## Acceptance scenarios

### SC-CLI-003: Bind inventory options

**Given** a scan command supplies --recursive, --follow-links and configured scan limits

**When** arguments are parsed

**Then** the corresponding application request contains those explicit settings without CLI-owned traversal.

### SC-CLI-004: Bind report and lifecycle options

**Given** an inspect command requests --format json, selected fields and an explicit lifecycle refresh

**When** the command is parsed

**Then** the application request preserves each user selection without performing refresh during parsing.
