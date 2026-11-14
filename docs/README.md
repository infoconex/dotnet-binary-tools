# Product and engineering documentation

## Governing specifications

- [Approved capability baseline](capabilities/README.md): CAP-001–CAP-008.
- [Approved architectural constraints](requirements/architecture.md): ARCH-001–ARCH-008, including SOLID, hexagonal architecture, distinct projects and inward-only references.
- [Functional requirements index](requirements/functional.md): links to 71 approved requirements maintained in the eight capability documents; [functional acceptance scenarios](scenarios/README.md) are indexed separately.
- [Cross-cutting nonfunctional requirements](requirements/non-functional.md): quality obligations and their approval status; distribution and evidence governance are specified separately.
- [Acceptance scenarios](scenarios/README.md): Given/When/Then specifications; [traceability](testing/traceability.md) is the source for actual verification.
- [TDD and testing strategy](testing/strategy.md).

## Engineering documents

- [Architecture](engineering/architecture.md) and [ADR 0001](engineering/decisions/0001-hexagonal-project-boundaries.md).
- [Distribution ADR 0002](engineering/decisions/0002-dotnet-11-aot-tools.md).
- [Build configuration](engineering/build-configuration.md), [packaging](engineering/packaging.md), [release gates](engineering/release-gates.md), [workflow](engineering/workflow.md), and [style](engineering/csharp-style.md).
