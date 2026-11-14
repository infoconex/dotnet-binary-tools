# FR-CFG-002: Accept an explicitly supplied supported configuration path for an individual inspection, validate that path, and distinguish it from automatic discovery.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Accept an explicitly supplied supported configuration path for an individual inspection, validate that path, and distinguish it from automatic discovery.

## Acceptance scenarios

### SC-CFG-003: Inspect an explicit config

**Given** an existing supported configuration file was explicitly selected with --config for one inspected file

**When** configuration analysis runs

**Then** the chosen file is read and its explicit source is recorded.

### SC-CFG-004: Reject a missing explicit config

**Given** a nonexistent file is passed through --config

**When** the CLI validates the explicit selection

**Then** an actionable input error is returned rather than silently falling back to auto-discovery.
