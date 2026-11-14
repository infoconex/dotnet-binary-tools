# Approved layered architecture

The production architecture follows mandatory [ARCH-001–ARCH-008](../requirements/architecture.md) and [ADR 0001](decisions/0001-hexagonal-project-boundaries.md). Acceptance evidence belongs in [traceability](../testing/traceability.md).

```text
CLI (outer adapter/composition root) -> Application -> Domain
                   |                       ^
                   +-----> Infrastructure --+
```

Arrows mean **compile-time** project references, not runtime call direction. Infrastructure implements ports defined in Application; at runtime application use cases invoke adapter implementations through those ports. Domain has no dependency on Infrastructure, Application or CLI.

| Project | Responsibility |
| --- | --- |
| `Infoconex.DotNetBinary.Domain` | Domain invariants and metadata facts. |
| `Infoconex.DotNetBinary.Application` | Application use cases, coordination and required ports. |
| `Infoconex.DotNetBinary.Infrastructure` | External adapters; split into distinct adapter projects when required. |
| `Infoconex.DotNetBinary.Cli` | Thin command adapter and composition root. |
| `Infoconex.DotNetBinary.Domain.Tests` | Domain unit tests. |
| `Infoconex.DotNetBinary.Cli.Tests` | CLI behavior and integration tests. |

Metadata and binary parsing, filesystem I/O, deployment configuration, HTTP reference updates and report writing must remain outside Domain. Never load or execute inspected candidate binaries. Preserve explicit unknown vs non-managed results, and keep declared targets distinct from configuration and lifecycle assessments.
