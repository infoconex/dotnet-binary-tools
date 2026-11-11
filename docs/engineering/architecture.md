# Approved layered architecture

The production architecture follows mandatory [ARCH-001–ARCH-008](../requirements/architecture.md) and [ADR 0001](decisions/0001-hexagonal-project-boundaries.md).

```text
CLI (outer adapter/composition root) -> Application -> Domain
                   |                       ^
                   +-----> Infrastructure --+
```

Arrows mean **compile-time** project references, not runtime call direction. Infrastructure implements ports defined in Application; at runtime application use cases invoke adapter implementations through those ports. Domain has no dependency on Infrastructure, Application or CLI.

| Project | Responsibility |
| --- | --- |
| `Infoconex.DotNetFramework.Domain` | Domain invariants and metadata facts; currently contains the raw target-framework moniker value object. |
| `Infoconex.DotNetFramework.Application` | Future application use cases, coordination and required ports. |
| `Infoconex.DotNetFramework.Infrastructure` | Future external adapters; split into distinct adapter projects when required. |
| `Infoconex.DotNetFramework.Cli` | Help/version skeleton now; future thin command adapter and composition root. |
| `Infoconex.DotNetFramework.Domain.Tests` | Domain unit tests. |
| `Infoconex.DotNetFramework.Cli.Tests` | Current CLI scaffold tests. |

Metadata and binary parsing, filesystem I/O, deployment configuration, HTTP reference updates and report writing must remain outside Domain. Never load or execute inspected candidate binaries. Preserve explicit unknown vs non-managed results, and keep declared targets distinct from configuration and lifecycle assessments.

**Implementation status:** This is structural preparation. `inspect`, `scan`, lifecycle assessment, configuration and JSON reporting remain unimplemented. Architectural tests and reviews for real ports/adapters will accompany their accepted requirements.
