# CAP-005: Inspection Inventory — functional scenarios

[Functional scenarios index](../README.md) · [Capability specification](../../../capabilities/CAP-005.md)

**Status:** 12 approved requirements, 16 focused acceptance scenarios. All scenarios are **Planned**; no feature acceptance is verified.

Each requirement has its own document containing the approved Given/When/Then scenarios. The titles below mirror the requirement-coverage table in the capability specification.

| Requirement | Title | Acceptance scenario IDs |
| --- | --- | --- |
| [FR-INV-001](FR-INV-001.md) | Use shared inventory completion semantics | `SC-INV-001`, `SC-INV-002` |
| [FR-INV-002](FR-INV-002.md) | Select DLL and EXE candidates | `SC-INV-003` |
| [FR-INV-003](FR-INV-003.md) | Limit scans to selected directories | `SC-INV-004`, `SC-INV-005` |
| [FR-INV-004](FR-INV-004.md) | Classify candidates independently | `SC-INV-006` |
| [FR-INV-005](FR-INV-005.md) | Continue after per-candidate errors | `SC-INV-007` |
| [FR-INV-006](FR-INV-006.md) | Skip discovered directory links by default | `SC-INV-008`, `SC-INV-009` |
| [FR-INV-007](FR-INV-007.md) | Prevent link cycles and duplicate subtrees | `SC-INV-010` |
| [FR-INV-008](FR-INV-008.md) | Bound scanning and parsing resources | `SC-INV-011`, `SC-INV-012` |
| [FR-INV-009](FR-INV-009.md) | Report resource-limited incomplete coverage | `SC-INV-013` |
| [FR-INV-010](FR-INV-010.md) | Aggregate inventory totals and distributions | `SC-INV-014` |
| [FR-INV-011](FR-INV-011.md) | Deduplicate overlapping physical paths | `SC-INV-015` |
| [FR-INV-012](FR-INV-012.md) | Record roots, errors, and completion reasons | `SC-INV-016` |

Actual test-to-scenario evidence is maintained in [traceability](../../../testing/traceability.md); a written scenario is not proof of implementation.
