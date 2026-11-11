# Approved functional requirements — CAP-001 Input Selection

**Status: Approved (Decision 41).** Other capability boundaries are approved but their detailed FRs are not yet ratified. No feature acceptance has been demonstrated.

| ID | Capability | Requirement | Acceptance scenarios |
| --- | --- | --- | --- |
| FR-INP-001 | CAP-001 | Accept exactly one explicitly selected regular file for `inspect`, regardless of filename extension. | [Scenarios](../scenarios/functional/FR-INP-001.md) |
| FR-INP-002 | CAP-001 | Accept one or more explicitly selected directories for `scan`. | [Scenarios](../scenarios/functional/FR-INP-002.md) |
| FR-INP-003 | CAP-001 | Support absolute and relative paths under host platform semantics; resolve relative paths against the working directory. | [Scenarios](../scenarios/functional/FR-INP-003.md) |
| FR-INP-004 | CAP-001 | Validate all explicit inputs before inspection; reject the entire request if any input is invalid. | [Scenarios](../scenarios/functional/FR-INP-004.md) |
| FR-INP-005 | CAP-001 | Accept explicitly selected file symlinks and directory links/junctions when their targets are valid. | [Scenarios](../scenarios/functional/FR-INP-005.md) |
| FR-INP-006 | CAP-001 | Require explicit paths, without application-level wildcard expansion. | [Scenarios](../scenarios/functional/FR-INP-006.md) |
| FR-INP-007 | CAP-001 | Produce actionable diagnostics identifying invalid inputs and validation failures. | [Scenarios](../scenarios/functional/FR-INP-007.md) |
| FR-INP-008 | CAP-001 | Supply validated input information downstream without inspecting assembly contents or traversing directories. | [Scenarios](../scenarios/functional/FR-INP-008.md) |

## Boundaries

- File contents and extension-based candidate selection are CAP-002/CAP-005 responsibilities, not CAP-001. A renamed assembly is still a valid `inspect` input.
- A directory that does not exist is an invalid **explicit request**, rejected before traversal. A file failure discovered during a valid scan is an inventory finding and does not invalidate the input.
- A directly selected link is accepted when its resolved destination is of the correct kind. Links *encountered during recursion* follow CAP-005's opt-in policy.
- Shell expansion occurring before command invocation is outside application-level glob handling.
- The retired draft `FR-001`–`FR-008` files do not form the approved functional catalog. IDs are never recycled.

[Capabilities](../capabilities.md) · [Mandatory architecture](architecture.md) · [Acceptance scenarios](../scenarios/README.md) · [Verification traceability](../testing/traceability.md).
