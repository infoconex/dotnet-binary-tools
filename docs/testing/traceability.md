# Scenario-to-test traceability

**Current status:** All v0.1 feature/quality acceptance scenarios are **Planned**. The engineering-baseline NUnit tests exercise only a raw-moniker model and the help/version stub; they do not verify these scenarios. Do not mark scenarios Automated until a real test is mapped and has observed passing execution evidence, including a run URL and exact commit SHA.

| Scenario | Requirement | Summary | Verification/test | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| [FR-001-SC01](../scenarios/functional/FR-001.md) | FR-001 | Recognize a managed library | Not yet assigned | Planned | None |
| [FR-001-SC02](../scenarios/functional/FR-001.md) | FR-001 | Reject a native binary as managed | Not yet assigned | Planned | None |
| [FR-002-SC01](../scenarios/functional/FR-002.md) | FR-002 | Read a declared framework | Not yet assigned | Planned | None |
| [FR-002-SC02](../scenarios/functional/FR-002.md) | FR-002 | Handle absent attribute | Not yet assigned | Planned | None |
| [FR-003-SC01](../scenarios/functional/FR-003.md) | FR-003 | Preserve the original moniker | Not yet assigned | Planned | None |
| [FR-003-SC02](../scenarios/functional/FR-003.md) | FR-003 | Handle unrecognized moniker | Not yet assigned | Planned | None |
| [FR-004-SC01](../scenarios/functional/FR-004.md) | FR-004 | Recognize known families | Not yet assigned | Planned | None |
| [FR-004-SC02](../scenarios/functional/FR-004.md) | FR-004 | Allow future versions | Not yet assigned | Planned | None |
| [FR-005-SC01](../scenarios/functional/FR-005.md) | FR-005 | Unknown is not CLR metadata version | Not yet assigned | Planned | None |
| [FR-005-SC02](../scenarios/functional/FR-005.md) | FR-005 | Corrupt metadata has distinct status | Not yet assigned | Planned | None |
| [FR-006-SC01](../scenarios/functional/FR-006.md) | FR-006 | Inspect a valid DLL | Not yet assigned | Planned | None |
| [FR-006-SC02](../scenarios/functional/FR-006.md) | FR-006 | Reject missing file | Not yet assigned | Planned | None |
| [FR-007-SC01](../scenarios/functional/FR-007.md) | FR-007 | Inventory mixed candidates | Not yet assigned | Planned | None |
| [FR-007-SC02](../scenarios/functional/FR-007.md) | FR-007 | Bound recursion and symlink traversal | Not yet assigned | Planned | None |
| [FR-008-SC01](../scenarios/functional/FR-008.md) | FR-008 | Produce human-readable results | Not yet assigned | Planned | None |
| [FR-008-SC02](../scenarios/functional/FR-008.md) | FR-008 | Emit valid machine-readable JSON | Not yet assigned | Planned | None |
| [NFR-001-SC01](../scenarios/non-functional/NFR-001.md) | NFR-001 | Single unreadable file yields useful failure | Not yet assigned | Planned | None |
| [NFR-001-SC02](../scenarios/non-functional/NFR-001.md) | NFR-001 | Directory scan isolates failures | Not yet assigned | Planned | None |
| [NFR-002-SC01](../scenarios/non-functional/NFR-002.md) | NFR-002 | Cross-platform fixture consistency | Not yet assigned | Planned | None |
| [NFR-002-SC02](../scenarios/non-functional/NFR-002.md) | NFR-002 | Independent of inspected runtime | Not yet assigned | Planned | None |
| [NFR-003-SC01](../scenarios/non-functional/NFR-003.md) | NFR-003 | Package command identity | Not yet assigned | Planned | None |
| [NFR-003-SC02](../scenarios/non-functional/NFR-003.md) | NFR-003 | Help and version exit successfully | Not yet assigned | Planned | None |
| [NFR-004-SC01](../scenarios/non-functional/NFR-004.md) | NFR-004 | Requirement scenario coverage | Not yet assigned | Planned | None |
| [NFR-004-SC02](../scenarios/non-functional/NFR-004.md) | NFR-004 | CI results are observable | Not yet assigned | Planned | None |
| [NFR-005-SC01](../scenarios/non-functional/NFR-005.md) | NFR-005 | Never execute untrusted assembly code | Not yet assigned | Planned | None |
| [NFR-005-SC02](../scenarios/non-functional/NFR-005.md) | NFR-005 | Do not use load-based inspection | Not yet assigned | Planned | None |
| [NFR-006-SC01](../scenarios/non-functional/NFR-006.md) | NFR-006 | AOT runs without the runtime | Not yet assigned | Planned | None |
| [NFR-006-SC02](../scenarios/non-functional/NFR-006.md) | NFR-006 | Tool package selects supported RID | Not yet assigned | Planned | None |
| [NFR-007-SC01](../scenarios/non-functional/NFR-007.md) | NFR-007 | Reject oversized or malformed input safely | Not yet assigned | Planned | None |
| [NFR-007-SC02](../scenarios/non-functional/NFR-007.md) | NFR-007 | Scan limits remain effective | Not yet assigned | Planned | None |


## Maintenance

- Use stable `FR-###-SC##` or `NFR-###-SC##` IDs; never reuse retired IDs.
- Planned means specified but not verified. Implemented-unverified means code exists but passing evidence is missing.
- Automated requires a linked test method, observed execution, CI run and exact SHA; Manual requires an observed repeatable procedure.
- One test can verify multiple scenarios only when assertions independently establish each expected behavior.
- Setup/build checks do not establish native runtime independence or genuine metadata inspection.
- When implementation begins, add test IDs and evidence in the same issue-scoped PR.
