# Product and engineering documentation

**Current state:** This is an engineering foundation; `inspect`, `scan`, lifecycle assessment and JSON reporting are not implemented.

## Governing specifications

- [Approved capability baseline](capabilities.md): CAP-001–CAP-008.
- [Approved architectural constraints](requirements/architecture.md): ARCH-001–ARCH-008, including SOLID, hexagonal architecture, distinct projects and inward-only references.
- [Approved functional requirements](requirements/functional.md): 71 FRs across all eight capabilities; [102 functional acceptance scenarios](scenarios/README.md) are defined and remain unverified.
- [Proposed nonfunctional requirements](requirements/non-functional.md): draft, not ratified.
- [Acceptance scenarios](scenarios/README.md): Given/When/Then specifications; [traceability](testing/traceability.md) is the source for actual verification.
- [TDD and testing strategy](testing/strategy.md).

## Engineering documents

- [Architecture](engineering/architecture.md) and [ADR 0001](engineering/decisions/0001-hexagonal-project-boundaries.md).
- [Distribution ADR 0002](engineering/decisions/0002-dotnet-11-aot-tools.md).
- [Build configuration](engineering/build-configuration.md), [packaging](engineering/packaging.md), [release gates](engineering/release-gates.md), [workflow](engineering/workflow.md), and [style](engineering/csharp-style.md).

**Planned** describes specified, unverified behavior. **Automated** requires a passing scenario-mapped run and exact SHA. Do not claim passing feature acceptance from foundation smoke tests.
