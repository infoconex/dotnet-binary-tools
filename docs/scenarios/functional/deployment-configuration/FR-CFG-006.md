# FR-CFG-006: Represent malformed, unreadable or unsupported configuration as distinct findings with diagnostic context, without executing application code.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Represent malformed, unreadable or unsupported configuration as distinct findings with diagnostic context, without executing application code.

## Acceptance scenarios

### SC-CFG-009: Contain malformed configuration

**Given** an associated runtimeconfig.json is invalid JSON

**When** the file is parsed

**Then** CAP-003 records a parse problem without inventing a requested framework or aborting unrelated inventory candidates.

### SC-CFG-010: Do not parse unsupported formats

**Given** a candidate has only web.config or deps.json nearby

**When** discovery runs

**Then** those files are not interpreted as supported configuration sources.
