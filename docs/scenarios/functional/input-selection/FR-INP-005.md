# FR-INP-005: Accept explicitly selected links

**Requirement:** Approved · **Acceptance verification:** Planned · **Capability:** [CAP-001](../../../capabilities/CAP-001.md)

[Functional requirements](../../../requirements/functional.md)

## Purpose

Follow directly requested links while leaving recursive traversal policy to CAP-005.

## Acceptance scenarios

### SC-INP-007: Accept an explicit file symlink

**Given** a symbolic link to a valid regular file

**When** the user selects the link with `inspect`

**Then** Input Selection accepts its target as the selected file.

### SC-INP-008: Accept an explicit directory link

**Given** a symbolic link or junction to an existing directory

**When** the user selects that link with `scan`

**Then** Input Selection accepts the directory root regardless of the recursive link-following default.

### SC-INP-009: Reject a broken symbolic link

**Given** an explicitly selected link with a missing target

**When** the user invokes `inspect` or `scan` with that link

**Then** input validation rejects the path without examining or traversing its missing destination.

## Verification notes

Use synthetic and cross-platform fixtures. Scenario approval does **not** establish that tests or command behavior exist. Record exact test names, execution results, CI run URLs and commit SHAs in [traceability](../../../testing/traceability.md).
