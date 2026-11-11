# FR-INP-001: Select exactly one file

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../capabilities.md)

[Functional requirements](../../requirements/functional.md)

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

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../testing/traceability.md).
