# Scenario-to-test traceability

**Current verification status:** All 102 active v0.1 functional acceptance scenarios are **Planned**. The NFR scenarios also remain proposed and unverified. Existing NUnit tests verify only the domain identity value object and help/version scaffold; there is no production implementation of inspect, scan or other product capabilities.

| Scenario | Capability | Requirement | Acceptance outcome | Verification/test | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| [SC-INP-001](../scenarios/functional/input-selection/FR-INP-001.md) | CAP-001 | FR-INP-001 | Accept an unusual file extension | Not yet assigned | Planned | None |
| [SC-INP-002](../scenarios/functional/input-selection/FR-INP-001.md) | CAP-001 | FR-INP-001 | Reject multiple inspect inputs | Not yet assigned | Planned | None |
| [SC-INP-003](../scenarios/functional/input-selection/FR-INP-002.md) | CAP-001 | FR-INP-002 | Accept two directories | Not yet assigned | Planned | None |
| [SC-INP-004](../scenarios/functional/input-selection/FR-INP-003.md) | CAP-001 | FR-INP-003 | Resolve a relative path | Not yet assigned | Planned | None |
| [SC-INP-005](../scenarios/functional/input-selection/FR-INP-007.md) | CAP-001 | FR-INP-007 | Identify a missing input | Not yet assigned | Planned | None |
| [SC-INP-006](../scenarios/functional/input-selection/FR-INP-004.md) | CAP-001 | FR-INP-004 | Reject all roots when one is invalid | Not yet assigned | Planned | None |
| [SC-INP-007](../scenarios/functional/input-selection/FR-INP-005.md) | CAP-001 | FR-INP-005 | Accept an explicit file symlink | Not yet assigned | Planned | None |
| [SC-INP-008](../scenarios/functional/input-selection/FR-INP-005.md) | CAP-001 | FR-INP-005 | Accept an explicit directory link | Not yet assigned | Planned | None |
| [SC-INP-009](../scenarios/functional/input-selection/FR-INP-005.md) | CAP-001 | FR-INP-005 | Reject a broken symbolic link | Not yet assigned | Planned | None |
| [SC-INP-010](../scenarios/functional/input-selection/FR-INP-006.md) | CAP-001 | FR-INP-006 | Do not expand an application-level wildcard | Not yet assigned | Planned | None |
| [SC-INP-011](../scenarios/functional/input-selection/FR-INP-004.md) | CAP-001 | FR-INP-004 | Reject an input of the wrong kind | Not yet assigned | Planned | None |
| [SC-INP-012](../scenarios/functional/input-selection/FR-INP-008.md) | CAP-001 | FR-INP-008 | Do not inspect or traverse during selection | Not yet assigned | Planned | None |
| [SC-ASM-001](../scenarios/functional/assembly-inspection/FR-ASM-001.md) | CAP-002 | FR-ASM-001 | Read a managed DLL | Not yet assigned | Planned | None |
| [SC-ASM-002](../scenarios/functional/assembly-inspection/FR-ASM-001.md) | CAP-002 | FR-ASM-001 | Inspect a renamed managed assembly | Not yet assigned | Planned | None |
| [SC-ASM-003](../scenarios/functional/assembly-inspection/FR-ASM-002.md) | CAP-002 | FR-ASM-002 | Classify a native executable | Not yet assigned | Planned | None |
| [SC-ASM-004](../scenarios/functional/assembly-inspection/FR-ASM-002.md) | CAP-002 | FR-ASM-002 | Differentiate bad data and I/O failure | Not yet assigned | Planned | None |
| [SC-ASM-005](../scenarios/functional/assembly-inspection/FR-ASM-003.md) | CAP-002 | FR-ASM-003 | Preserve an explicit target | Not yet assigned | Planned | None |
| [SC-ASM-006](../scenarios/functional/assembly-inspection/FR-ASM-004.md) | CAP-002 | FR-ASM-004 | Recognize supported framework families | Not yet assigned | Planned | None |
| [SC-ASM-007](../scenarios/functional/assembly-inspection/FR-ASM-004.md) | CAP-002 | FR-ASM-004 | Preserve an unfamiliar moniker | Not yet assigned | Planned | None |
| [SC-ASM-008](../scenarios/functional/assembly-inspection/FR-ASM-005.md) | CAP-002 | FR-ASM-005 | Do not infer a target | Not yet assigned | Planned | None |
| [SC-ASM-009](../scenarios/functional/assembly-inspection/FR-ASM-006.md) | CAP-002 | FR-ASM-006 | Read complete assembly identity | Not yet assigned | Planned | None |
| [SC-ASM-010](../scenarios/functional/assembly-inspection/FR-ASM-007.md) | CAP-002 | FR-ASM-007 | Preserve optional versions and attributes | Not yet assigned | Planned | None |
| [SC-ASM-011](../scenarios/functional/assembly-inspection/FR-ASM-007.md) | CAP-002 | FR-ASM-007 | Handle absent optional attributes | Not yet assigned | Planned | None |
| [SC-ASM-012](../scenarios/functional/assembly-inspection/FR-ASM-008.md) | CAP-002 | FR-ASM-008 | Report architecture facts | Not yet assigned | Planned | None |
| [SC-ASM-013](../scenarios/functional/assembly-inspection/FR-ASM-009.md) | CAP-002 | FR-ASM-009 | Report direct references and entry point | Not yet assigned | Planned | None |
| [SC-ASM-014](../scenarios/functional/assembly-inspection/FR-ASM-009.md) | CAP-002 | FR-ASM-009 | Avoid apphost inference | Not yet assigned | Planned | None |
| [SC-ASM-015](../scenarios/functional/assembly-inspection/FR-ASM-010.md) | CAP-002 | FR-ASM-010 | Report file facts | Not yet assigned | Planned | None |
| [SC-ASM-016](../scenarios/functional/assembly-inspection/FR-ASM-010.md) | CAP-002 | FR-ASM-010 | Hash only when requested | Not yet assigned | Planned | None |
| [SC-ASM-017](../scenarios/functional/assembly-inspection/FR-ASM-011.md) | CAP-002 | FR-ASM-011 | Isolate an optional metadata fault | Not yet assigned | Planned | None |
| [SC-CFG-001](../scenarios/functional/deployment-configuration/FR-CFG-001.md) | CAP-003 | FR-CFG-001 | Discover a matching runtimeconfig | Not yet assigned | Planned | None |
| [SC-CFG-002](../scenarios/functional/deployment-configuration/FR-CFG-001.md) | CAP-003 | FR-CFG-001 | Do not associate another application | Not yet assigned | Planned | None |
| [SC-CFG-003](../scenarios/functional/deployment-configuration/FR-CFG-002.md) | CAP-003 | FR-CFG-002 | Inspect an explicit config | Not yet assigned | Planned | None |
| [SC-CFG-004](../scenarios/functional/deployment-configuration/FR-CFG-002.md) | CAP-003 | FR-CFG-002 | Reject a missing explicit config | Not yet assigned | Planned | None |
| [SC-CFG-005](../scenarios/functional/deployment-configuration/FR-CFG-003.md) | CAP-003 | FR-CFG-003 | Read a Framework runtime declaration | Not yet assigned | Planned | None |
| [SC-CFG-006](../scenarios/functional/deployment-configuration/FR-CFG-004.md) | CAP-003 | FR-CFG-004 | Read framework and roll-forward policy | Not yet assigned | Planned | None |
| [SC-CFG-007](../scenarios/functional/deployment-configuration/FR-CFG-004.md) | CAP-003 | FR-CFG-004 | Read multiple framework requirements | Not yet assigned | Planned | None |
| [SC-CFG-008](../scenarios/functional/deployment-configuration/FR-CFG-005.md) | CAP-003 | FR-CFG-005 | Handle no configuration | Not yet assigned | Planned | None |
| [SC-CFG-009](../scenarios/functional/deployment-configuration/FR-CFG-006.md) | CAP-003 | FR-CFG-006 | Contain malformed configuration | Not yet assigned | Planned | None |
| [SC-CFG-010](../scenarios/functional/deployment-configuration/FR-CFG-006.md) | CAP-003 | FR-CFG-006 | Do not parse unsupported formats | Not yet assigned | Planned | None |
| [SC-CFG-011](../scenarios/functional/deployment-configuration/FR-CFG-007.md) | CAP-003 | FR-CFG-007 | Preserve conflicting declarations | Not yet assigned | Planned | None |
| [SC-CFG-012](../scenarios/functional/deployment-configuration/FR-CFG-008.md) | CAP-003 | FR-CFG-008 | Trace a runtime request to its file | Not yet assigned | Planned | None |
| [SC-LCY-001](../scenarios/functional/lifecycle-assessment/FR-LCY-001.md) | CAP-004 | FR-LCY-001 | Use bundled data offline | Not yet assigned | Planned | None |
| [SC-LCY-002](../scenarios/functional/lifecycle-assessment/FR-LCY-002.md) | CAP-004 | FR-LCY-002 | Assess a known target as of a date | Not yet assigned | Planned | None |
| [SC-LCY-003](../scenarios/functional/lifecycle-assessment/FR-LCY-002.md) | CAP-004 | FR-LCY-002 | Qualify environment-dependent support | Not yet assigned | Planned | None |
| [SC-LCY-004](../scenarios/functional/lifecycle-assessment/FR-LCY-003.md) | CAP-004 | FR-LCY-003 | Separate target and configuration status | Not yet assigned | Planned | None |
| [SC-LCY-005](../scenarios/functional/lifecycle-assessment/FR-LCY-004.md) | CAP-004 | FR-LCY-004 | Do not assign a runtime lifecycle to .NET Standard | Not yet assigned | Planned | None |
| [SC-LCY-006](../scenarios/functional/lifecycle-assessment/FR-LCY-004.md) | CAP-004 | FR-LCY-004 | Do not fabricate an unknown lifecycle | Not yet assigned | Planned | None |
| [SC-LCY-007](../scenarios/functional/lifecycle-assessment/FR-LCY-005.md) | CAP-004 | FR-LCY-005 | Keep advisories non-enforcing | Not yet assigned | Planned | None |
| [SC-LCY-008](../scenarios/functional/lifecycle-assessment/FR-LCY-006.md) | CAP-004 | FR-LCY-006 | Report a stale dataset | Not yet assigned | Planned | None |
| [SC-LCY-009](../scenarios/functional/lifecycle-assessment/FR-LCY-007.md) | CAP-004 | FR-LCY-007 | Default mode stays offline | Not yet assigned | Planned | None |
| [SC-LCY-010](../scenarios/functional/lifecycle-assessment/FR-LCY-007.md) | CAP-004 | FR-LCY-007 | Refresh only verified source data | Not yet assigned | Planned | None |
| [SC-LCY-011](../scenarios/functional/lifecycle-assessment/FR-LCY-008.md) | CAP-004 | FR-LCY-008 | Retain the prior dataset on failure | Not yet assigned | Planned | None |
| [SC-INV-001](../scenarios/functional/inspection-inventory/FR-INV-001.md) | CAP-005 | FR-INV-001 | Inventory an individual inspection | Not yet assigned | Planned | None |
| [SC-INV-002](../scenarios/functional/inspection-inventory/FR-INV-001.md) | CAP-005 | FR-INV-001 | Inventory multiple roots | Not yet assigned | Planned | None |
| [SC-INV-003](../scenarios/functional/inspection-inventory/FR-INV-002.md) | CAP-005 | FR-INV-002 | Select expected extensions | Not yet assigned | Planned | None |
| [SC-INV-004](../scenarios/functional/inspection-inventory/FR-INV-003.md) | CAP-005 | FR-INV-003 | Do not recurse by default | Not yet assigned | Planned | None |
| [SC-INV-005](../scenarios/functional/inspection-inventory/FR-INV-003.md) | CAP-005 | FR-INV-003 | Recurse when requested | Not yet assigned | Planned | None |
| [SC-INV-006](../scenarios/functional/inspection-inventory/FR-INV-004.md) | CAP-005 | FR-INV-004 | Keep mixed candidates independent | Not yet assigned | Planned | None |
| [SC-INV-007](../scenarios/functional/inspection-inventory/FR-INV-005.md) | CAP-005 | FR-INV-005 | Isolate a file failure | Not yet assigned | Planned | None |
| [SC-INV-008](../scenarios/functional/inspection-inventory/FR-INV-006.md) | CAP-005 | FR-INV-006 | Default link behavior | Not yet assigned | Planned | None |
| [SC-INV-009](../scenarios/functional/inspection-inventory/FR-INV-006.md) | CAP-005 | FR-INV-006 | Honor a linked selected root | Not yet assigned | Planned | None |
| [SC-INV-010](../scenarios/functional/inspection-inventory/FR-INV-007.md) | CAP-005 | FR-INV-007 | Prevent symbolic-link cycles | Not yet assigned | Planned | None |
| [SC-INV-011](../scenarios/functional/inspection-inventory/FR-INV-008.md) | CAP-005 | FR-INV-008 | Enforce candidate count limit | Not yet assigned | Planned | None |
| [SC-INV-012](../scenarios/functional/inspection-inventory/FR-INV-008.md) | CAP-005 | FR-INV-008 | Skip an oversized candidate | Not yet assigned | Planned | None |
| [SC-INV-013](../scenarios/functional/inspection-inventory/FR-INV-009.md) | CAP-005 | FR-INV-009 | Report bounded partial inventory | Not yet assigned | Planned | None |
| [SC-INV-014](../scenarios/functional/inspection-inventory/FR-INV-010.md) | CAP-005 | FR-INV-010 | Reconcile counts | Not yet assigned | Planned | None |
| [SC-INV-015](../scenarios/functional/inspection-inventory/FR-INV-011.md) | CAP-005 | FR-INV-011 | Deduplicate overlapping roots | Not yet assigned | Planned | None |
| [SC-INV-016](../scenarios/functional/inspection-inventory/FR-INV-012.md) | CAP-005 | FR-INV-012 | Describe incomplete coverage | Not yet assigned | Planned | None |
| [SC-RPT-001](../scenarios/functional/results-and-reporting/FR-RPT-001.md) | CAP-006 | FR-RPT-001 | Use one schema for both commands | Not yet assigned | Planned | None |
| [SC-RPT-002](../scenarios/functional/results-and-reporting/FR-RPT-002.md) | CAP-006 | FR-RPT-002 | Concise human output | Not yet assigned | Planned | None |
| [SC-RPT-003](../scenarios/functional/results-and-reporting/FR-RPT-002.md) | CAP-006 | FR-RPT-002 | Expanded human output | Not yet assigned | Planned | None |
| [SC-RPT-004](../scenarios/functional/results-and-reporting/FR-RPT-003.md) | CAP-006 | FR-RPT-003 | Keep JSON machine-readable | Not yet assigned | Planned | None |
| [SC-RPT-005](../scenarios/functional/results-and-reporting/FR-RPT-004.md) | CAP-006 | FR-RPT-004 | Project only item fields | Not yet assigned | Planned | None |
| [SC-RPT-006](../scenarios/functional/results-and-reporting/FR-RPT-004.md) | CAP-006 | FR-RPT-004 | Project fields in human mode | Not yet assigned | Planned | None |
| [SC-RPT-007](../scenarios/functional/results-and-reporting/FR-RPT-005.md) | CAP-006 | FR-RPT-005 | Reject conflicting selections | Not yet assigned | Planned | None |
| [SC-RPT-008](../scenarios/functional/results-and-reporting/FR-RPT-006.md) | CAP-006 | FR-RPT-006 | Preserve item and inventory status | Not yet assigned | Planned | None |
| [SC-RPT-009](../scenarios/functional/results-and-reporting/FR-RPT-007.md) | CAP-006 | FR-RPT-007 | Honor output destination | Not yet assigned | Planned | None |
| [SC-RPT-010](../scenarios/functional/results-and-reporting/FR-RPT-007.md) | CAP-006 | FR-RPT-007 | Preserve pipeline output | Not yet assigned | Planned | None |
| [SC-RPT-011](../scenarios/functional/results-and-reporting/FR-RPT-008.md) | CAP-006 | FR-RPT-008 | Do not overwrite an existing file | Not yet assigned | Planned | None |
| [SC-RPT-012](../scenarios/functional/results-and-reporting/FR-RPT-008.md) | CAP-006 | FR-RPT-008 | Handle simultaneous filename collision | Not yet assigned | Planned | None |
| [SC-RPT-013](../scenarios/functional/results-and-reporting/FR-RPT-009.md) | CAP-006 | FR-RPT-009 | Stream a bounded-memory report | Not yet assigned | Planned | None |
| [SC-RPT-014](../scenarios/functional/results-and-reporting/FR-RPT-009.md) | CAP-006 | FR-RPT-009 | Report output write failure | Not yet assigned | Planned | None |
| [SC-CLI-001](../scenarios/functional/command-line-interface/FR-CLI-001.md) | CAP-007 | FR-CLI-001 | Dispatch supported commands | Not yet assigned | Planned | None |
| [SC-CLI-002](../scenarios/functional/command-line-interface/FR-CLI-002.md) | CAP-007 | FR-CLI-002 | Reject invalid command arity | Not yet assigned | Planned | None |
| [SC-CLI-003](../scenarios/functional/command-line-interface/FR-CLI-003.md) | CAP-007 | FR-CLI-003 | Bind inventory options | Not yet assigned | Planned | None |
| [SC-CLI-004](../scenarios/functional/command-line-interface/FR-CLI-003.md) | CAP-007 | FR-CLI-003 | Bind report and lifecycle options | Not yet assigned | Planned | None |
| [SC-CLI-005](../scenarios/functional/command-line-interface/FR-CLI-004.md) | CAP-007 | FR-CLI-004 | Reject an unknown option | Not yet assigned | Planned | None |
| [SC-CLI-006](../scenarios/functional/command-line-interface/FR-CLI-004.md) | CAP-007 | FR-CLI-004 | Reject conflicting formatting | Not yet assigned | Planned | None |
| [SC-CLI-007](../scenarios/functional/command-line-interface/FR-CLI-005.md) | CAP-007 | FR-CLI-005 | Preserve success for a partial scan | Not yet assigned | Planned | None |
| [SC-CLI-008](../scenarios/functional/command-line-interface/FR-CLI-005.md) | CAP-007 | FR-CLI-005 | Reject invalid selected inputs | Not yet assigned | Planned | None |
| [SC-CLI-009](../scenarios/functional/command-line-interface/FR-CLI-006.md) | CAP-007 | FR-CLI-006 | Protect JSON pipelines | Not yet assigned | Planned | None |
| [SC-CLI-010](../scenarios/functional/command-line-interface/FR-CLI-007.md) | CAP-007 | FR-CLI-007 | Provide usable help | Not yet assigned | Planned | None |
| [SC-CLI-011](../scenarios/functional/command-line-interface/FR-CLI-008.md) | CAP-007 | FR-CLI-008 | Keep logic out of CLI | Not yet assigned | Planned | None |
| [SC-DST-001](../scenarios/functional/distribution/FR-DST-001.md) | CAP-008 | FR-DST-001 | Run a native executable for each supported OS | Not yet assigned | Planned | None |
| [SC-DST-002](../scenarios/functional/distribution/FR-DST-002.md) | CAP-008 | FR-DST-002 | Install the .NET Tool on a supported host | Not yet assigned | Planned | None |
| [SC-DST-003](../scenarios/functional/distribution/FR-DST-003.md) | CAP-008 | FR-DST-003 | Compare two distribution forms | Not yet assigned | Planned | None |
| [SC-DST-004](../scenarios/functional/distribution/FR-DST-004.md) | CAP-008 | FR-DST-004 | Verify release contents | Not yet assigned | Planned | None |
| [SC-DST-005](../scenarios/functional/distribution/FR-DST-005.md) | CAP-008 | FR-DST-005 | Run without installed .NET | Not yet assigned | Planned | None |
| [SC-DST-006](../scenarios/functional/distribution/FR-DST-005.md) | CAP-008 | FR-DST-005 | Distinguish tool installation prerequisites | Not yet assigned | Planned | None |
| [SC-DST-007](../scenarios/functional/distribution/FR-DST-006.md) | CAP-008 | FR-DST-006 | Gate a supported RID | Not yet assigned | Planned | None |
| [SC-DST-008](../scenarios/functional/distribution/FR-DST-007.md) | CAP-008 | FR-DST-007 | Block unauthorized publication | Not yet assigned | Planned | None |
| [SC-DST-009](../scenarios/functional/distribution/FR-DST-007.md) | CAP-008 | FR-DST-007 | Avoid unsupported ARM64 claims | Not yet assigned | Planned | None |
| [NFR-001-SC01](../scenarios/non-functional/NFR-001.md) | Proposed | NFR-001 | Handle a file read failure safely | Not yet assigned | Planned | None |
| [NFR-001-SC02](../scenarios/non-functional/NFR-001.md) | Proposed | NFR-001 | Contain individual scan failures | Not yet assigned | Planned | None |
| [NFR-002-SC01](../scenarios/non-functional/NFR-002.md) | Proposed | NFR-002 | Interpret the same fixtures consistently | Not yet assigned | Planned | None |
| [NFR-002-SC02](../scenarios/non-functional/NFR-002.md) | Proposed | NFR-002 | Avoid target-runtime prerequisites | Not yet assigned | Planned | None |
| [NFR-003-SC01](../scenarios/non-functional/NFR-003.md) | Proposed | NFR-003 | Do not execute inspected code | Not yet assigned | Planned | None |
| [NFR-003-SC02](../scenarios/non-functional/NFR-003.md) | Proposed | NFR-003 | Contain adversarial binary metadata | Not yet assigned | Planned | None |
| [NFR-004-SC01](../scenarios/non-functional/NFR-004.md) | Proposed | NFR-004 | Bound parsing of oversized candidates | Not yet assigned | Planned | None |
| [NFR-004-SC02](../scenarios/non-functional/NFR-004.md) | Proposed | NFR-004 | Bound recursive inventory | Not yet assigned | Planned | None |

## Evidence policy

- Independent scenario IDs are traceable to one or more independently approved functional requirements. Each scenario must have a concrete test method and **observed passing CI run with exact SHA** before being upgraded from Planned to Automated.
- Planned = specification only; Implemented-unverified = code exists but acceptance evidence is missing; Automated = observed mapped passing test; Manual = observed reproducible verification; Blocked = documented obstacle.
- Focused scenarios are supplemented by detailed test cases for negative, boundary, platform, security and edge behavior. One test may cover several scenarios only if independently asserted.
- No build-only, source-only, help/version smoke or Native AOT publish step establishes product feature acceptance or runtime-free distribution by itself.
