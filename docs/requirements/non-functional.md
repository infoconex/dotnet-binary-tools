# Nonfunctional requirements: v0.1 quality baseline

**Status:** Proposed for future review; not yet ratified. [Approved capabilities](../capabilities.md) and [mandatory architecture](architecture.md) are separate governing specifications. Quality requirements include security, cross-platform support, Native AOT distribution, and resource limits. A declared target does not prove runtime compatibility.

| ID | Quality obligation | Acceptance scenarios |
| --- | --- | --- |
| NFR-001 | Resilient error handling | [Scenarios](../scenarios/non-functional/NFR-001.md) |
| NFR-002 | Cross-platform support | [Scenarios](../scenarios/non-functional/NFR-002.md) |
| NFR-003 | NuGet .NET Tool packaging | [Scenarios](../scenarios/non-functional/NFR-003.md) |
| NFR-004 | Automated acceptance and evidence | [Scenarios](../scenarios/non-functional/NFR-004.md) |
| NFR-005 | Safe metadata-only inspection | [Scenarios](../scenarios/non-functional/NFR-005.md) |
| NFR-006 | Runtime-free Native AOT distribution | [Scenarios](../scenarios/non-functional/NFR-006.md) |
| NFR-007 | Bounded resource consumption | [Scenarios](../scenarios/non-functional/NFR-007.md) |

Accept only evidence for the exact supported OS/RID under test. Native AOT and package installation cannot be marked complete from a managed build alone.
