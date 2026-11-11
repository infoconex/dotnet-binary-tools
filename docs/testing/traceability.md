# Scenario-to-test traceability

**Current verification status: All active scenarios are Planned.** Existing NUnit tests demonstrate the moniker value object and CLI help/version scaffold only. They do not establish CAP-001 acceptance or inspect/scan functionality.

| Scenario | Capability | Requirement | Acceptance outcome | Verification/test | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| [SC-INP-001](../scenarios/functional/FR-INP-001.md) | CAP-001 | FR-INP-001 | Accept an unusual file extension | Not yet assigned | Planned | None |
| [SC-INP-002](../scenarios/functional/FR-INP-001.md) | CAP-001 | FR-INP-001 | Reject multiple inspect inputs | Not yet assigned | Planned | None |
| [SC-INP-003](../scenarios/functional/FR-INP-002.md) | CAP-001 | FR-INP-002 | Accept two directories | Not yet assigned | Planned | None |
| [SC-INP-004](../scenarios/functional/FR-INP-003.md) | CAP-001 | FR-INP-003 | Resolve a relative path | Not yet assigned | Planned | None |
| [SC-INP-005](../scenarios/functional/FR-INP-007.md) | CAP-001 | FR-INP-007 | Identify a missing input | Not yet assigned | Planned | None |
| [SC-INP-006](../scenarios/functional/FR-INP-004.md) | CAP-001 | FR-INP-004 | Reject all roots when one is invalid | Not yet assigned | Planned | None |
| [SC-INP-007](../scenarios/functional/FR-INP-005.md) | CAP-001 | FR-INP-005 | Accept an explicit file symlink | Not yet assigned | Planned | None |
| [SC-INP-008](../scenarios/functional/FR-INP-005.md) | CAP-001 | FR-INP-005 | Accept an explicit directory link | Not yet assigned | Planned | None |
| [SC-INP-009](../scenarios/functional/FR-INP-005.md) | CAP-001 | FR-INP-005 | Reject a broken symbolic link | Not yet assigned | Planned | None |
| [SC-INP-010](../scenarios/functional/FR-INP-006.md) | CAP-001 | FR-INP-006 | Do not expand an application-level wildcard | Not yet assigned | Planned | None |
| [SC-INP-011](../scenarios/functional/FR-INP-004.md) | CAP-001 | FR-INP-004 | Reject an input of the wrong kind | Not yet assigned | Planned | None |
| [SC-INP-012](../scenarios/functional/FR-INP-008.md) | CAP-001 | FR-INP-008 | Do not inspect or traverse during selection | Not yet assigned | Planned | None |
| [NFR-001-SC01](../scenarios/non-functional/NFR-001.md) | Proposed | NFR-001 | Single unreadable file yields useful failure | Not yet assigned | Planned | None |
| [NFR-001-SC02](../scenarios/non-functional/NFR-001.md) | Proposed | NFR-001 | Directory scan isolates failures | Not yet assigned | Planned | None |
| [NFR-002-SC01](../scenarios/non-functional/NFR-002.md) | Proposed | NFR-002 | Cross-platform fixture consistency | Not yet assigned | Planned | None |
| [NFR-002-SC02](../scenarios/non-functional/NFR-002.md) | Proposed | NFR-002 | Independent of inspected runtime | Not yet assigned | Planned | None |
| [NFR-003-SC01](../scenarios/non-functional/NFR-003.md) | Proposed | NFR-003 | Package command identity | Not yet assigned | Planned | None |
| [NFR-003-SC02](../scenarios/non-functional/NFR-003.md) | Proposed | NFR-003 | Help and version exit successfully | Not yet assigned | Planned | None |
| [NFR-004-SC01](../scenarios/non-functional/NFR-004.md) | Proposed | NFR-004 | Requirement scenario coverage | Not yet assigned | Planned | None |
| [NFR-004-SC02](../scenarios/non-functional/NFR-004.md) | Proposed | NFR-004 | CI results are observable | Not yet assigned | Planned | None |
| [NFR-005-SC01](../scenarios/non-functional/NFR-005.md) | Proposed | NFR-005 | Never execute untrusted assembly code | Not yet assigned | Planned | None |
| [NFR-005-SC02](../scenarios/non-functional/NFR-005.md) | Proposed | NFR-005 | Do not use load-based inspection | Not yet assigned | Planned | None |
| [NFR-006-SC01](../scenarios/non-functional/NFR-006.md) | Proposed | NFR-006 | AOT runs without the runtime | Not yet assigned | Planned | None |
| [NFR-006-SC02](../scenarios/non-functional/NFR-006.md) | Proposed | NFR-006 | Tool package selects supported RID | Not yet assigned | Planned | None |
| [NFR-007-SC01](../scenarios/non-functional/NFR-007.md) | Proposed | NFR-007 | Reject oversized or malformed input safely | Not yet assigned | Planned | None |
| [NFR-007-SC02](../scenarios/non-functional/NFR-007.md) | Proposed | NFR-007 | Scan limits remain effective | Not yet assigned | Planned | None |

## Evidence policy

- Stable `SC-INP-###` scenario IDs are independent from `FR-INP-###` requirement IDs. The active FR scenario set is approved; the legacy `FR-001-SC##` rows are retired.
- `Planned` means specified but not verified. `Implemented-unverified` means code exists but observed evidence is missing. `Automated` requires scenario-mapped test method, passing CI run URL, and exact SHA. `Manual` requires a repeatable observed verification. `Blocked` requires a stated blocker.
- One test may verify multiple scenarios only with independently meaningful assertions for each. More detailed unit, negative, platform and edge tests trace back to focused acceptance scenarios.
- Never infer successful feature acceptance or Native AOT runtime independence from build success or a help/version smoke test alone.
