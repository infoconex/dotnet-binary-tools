# FR-INP-001: Select exactly one file

**Capability:** [CAP-001](../../../capabilities/CAP-001.md) · [Verification evidence](../../../testing/traceability.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Accept an explicitly selected single regular file, without deciding its binary format.

## Acceptance scenarios

### SC-INP-001: Accept an unusual file extension

**Given** a valid regular file named `Sample.bin`

**When** the user invokes `inspect Sample.bin`

**Then** Input Selection accepts the file regardless of its extension and passes it to CAP-002 for classification.

### SC-INP-002: Reject multiple inspect inputs

**Given** two existing regular files

**When** the user supplies both to a single `inspect` invocation

**Then** the request is rejected before inspection and a diagnostic explains that one file is required.
