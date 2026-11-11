# Mandatory architecture constraints (approved)

**Status: Approved (Decision 35).** These are binding constraints across CAP-001–CAP-008, not another product capability.

| ID | Architectural constraint | Verification |
| --- | --- | --- |
| ARCH-001 | Follow SOLID (single responsibility, open/closed, Liskov substitution, interface segregation and dependency inversion). | Design/code reviews and relevant tests. |
| ARCH-002 | Adopt domain-centric hexagonal (Ports and Adapters) architecture. | Boundary review and structural tests. |
| ARCH-003 | Compile Domain, Application, Infrastructure and CLI/presentation layers as **separate .NET projects**. | CI verifies solution membership. |
| ARCH-004 | Enforce **inward-only compile-time references**: Application → Domain; Infrastructure → Application's ports; CLI → Application and Infrastructure for composition; inner projects never refer outward. | CI verifies the project-reference DAG. |
| ARCH-005 | Define ports at the appropriate consuming inner boundary and implement them in outer adapters; don't leak vendor types into Domain. | API review and application tests. |
| ARCH-006 | Keep PE/metadata access, filesystem, configuration, network, lifecycle source access and report persistence outside Domain. | Dependency review, static gates and tests. |
| ARCH-007 | Keep the CLI thin, responsible for command interaction and composition rather than business decisions. | CLI and component tests. |
| ARCH-008 | Automatically check structural architecture rules in CI and complement static checks with behavior tests. | Recorded CI evidence at the exact SHA. |

```text
CLI (composition root) ----> Application ---> Domain
       |                         ^
       +----> Infrastructure ----+
```

All arrows represent **compile-time dependencies toward the center**, not runtime invocation direction. Infrastructure implements ports owned by Application and is composed at the outer entry point.

Independent infrastructure adapter projects may be added when their boundaries justify it; do not create speculative abstraction layers. [Architecture details](../engineering/architecture.md) and [ADR 0001](../engineering/decisions/0001-hexagonal-project-boundaries.md).
