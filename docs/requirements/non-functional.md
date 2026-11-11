# Nonfunctional requirements: v0.1 quality baseline

**Status:** Proposed for review. The first four quality requirements map to the original README; the remaining three make security, Native AOT distribution, and resource limits explicit. A declared target does not prove runtime compatibility.

| ID | Quality obligation | Original ID | Acceptance scenarios |
| --- | --- | --- | --- |
| NFR-001 | Resilient error handling | REQ-009 | [Scenarios](../scenarios/non-functional/NFR-001.md) |
| NFR-002 | Cross-platform support | REQ-010 | [Scenarios](../scenarios/non-functional/NFR-002.md) |
| NFR-003 | NuGet .NET Tool packaging | REQ-011 | [Scenarios](../scenarios/non-functional/NFR-003.md) |
| NFR-004 | Automated acceptance and evidence | REQ-012 | [Scenarios](../scenarios/non-functional/NFR-004.md) |
| NFR-005 | Safe metadata-only inspection | New | [Scenarios](../scenarios/non-functional/NFR-005.md) |
| NFR-006 | Runtime-free Native AOT distribution | New | [Scenarios](../scenarios/non-functional/NFR-006.md) |
| NFR-007 | Bounded resource consumption | New | [Scenarios](../scenarios/non-functional/NFR-007.md) |

Accept only evidence for the exact supported OS/RID under test. Native AOT and package installation cannot be marked complete from a managed build alone.
