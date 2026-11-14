# FR-INP-003: Resolve file system paths

**Capability:** [CAP-001](../../../capabilities/CAP-001.md) · [Verification evidence](../../../testing/traceability.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Use current-directory and host-platform path rules.

## Acceptance scenarios

### SC-INP-004: Resolve a relative path

**Given** an existing file below the current working directory

**When** the user supplies its relative path to `inspect`

**Then** the input resolves to the correct file under the host platform's path semantics.
