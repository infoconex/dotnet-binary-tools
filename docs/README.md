# Engineering documentation

This repository is in its **engineering-foundation phase**. No assembly-inspection commands have been implemented or released. The CLI deliberately rejects `inspect` and `scan` until their acceptance scenarios are implemented and verified.

## Governing documents

- [Functional requirements](requirements/functional.md) and [nonfunctional requirements](requirements/non-functional.md): stable FR/NFR IDs and priorities.
- [Acceptance scenarios](scenarios/README.md): normative Given/When/Then specifications, one file per requirement.
- [Scenario-to-test traceability](testing/traceability.md): execution status and observed evidence; no speculative passes.
- [TDD and testing strategy](testing/strategy.md): test levels, fixtures, red/green/refactor, platform proof.
- [Architecture](engineering/architecture.md), [C# conventions](engineering/csharp-style.md), [workflow](engineering/workflow.md), [release gates](engineering/release-gates.md).
- [ADR 0001](engineering/decisions/0001-core-cli-boundary.md): Core and CLI dependency boundaries.
- [ADR 0002](engineering/decisions/0002-dotnet-11-aot-tools.md): .NET 11, AOT, RID-specific packaging, and command/package naming.
- [Packaging and distribution](engineering/packaging.md): planned NuGet Tool and standalone executables.
- [Centralized build configuration](engineering/build-configuration.md): .slnx, SDK selection, shared MSBuild properties, NuGet feeds and optional files.

## Status semantics

- **Planned:** normative requirement/scenario specified; no adequate passing execution evidence.
- **Implemented-unverified:** implementation or tests exist, but verification is not established.
- **Automated:** exact scenario-mapped automated test(s) passed with recorded run and SHA.
- **Manual:** a documented, observed manual verification exists.
- **Blocked:** verification cannot proceed; blocker is recorded.

Setup smoke tests exercise project wiring and the help-only CLI; they **do not** establish assembly-inspection acceptance.
