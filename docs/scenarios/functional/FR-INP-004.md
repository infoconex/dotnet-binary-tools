# FR-INP-004: Validate the complete request

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../capabilities.md)

[Functional requirements](../../requirements/functional.md)

## Purpose

Reject invalid explicit inputs before starting inventory or inspection.

## Acceptance scenarios

### SC-INP-006: Reject all roots when one is invalid

**Given** two existing directories and one nonexistent directory

**When** the user supplies all three to `scan`

**Then** the whole request is rejected, no traversal begins, and a nonzero command exit is produced.

### SC-INP-011: Reject an input of the wrong kind

**Given** a directory and a regular file

**When** the user supplies the directory to `inspect` or the file to `scan`

**Then** the operation is rejected before inspecting or traversing the input.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../testing/traceability.md).
