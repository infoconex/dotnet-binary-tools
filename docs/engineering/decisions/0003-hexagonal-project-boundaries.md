# ADR 0003: Hexagonal, domain-centric architecture with physical project boundaries

**Status: Approved.** Supersedes [ADR 0001](0001-core-cli-boundary.md).

## Context

The original Core/CLI proposal did not satisfy the approved requirement to separate Domain, Application and Infrastructure into independent .NET projects.

## Decision

Adopt SOLID and Ports-and-Adapters. Domain contains pure concepts and invariants; Application contains use-case coordination and ports at the consuming inner boundary; Infrastructure implements those ports for filesystem, PE metadata, deployment configuration, reference data, network and report persistence; CLI is a thin outer adapter and composition root.

Compile-time references point **inward**: Application → Domain, Infrastructure → Application, CLI → Application and Infrastructure. CLI may refer to concrete adapter implementations for composition, but neither Domain nor Application may depend on those outer projects. Create adapter-specific infrastructure projects only when justified by concrete dependencies.

The engineering foundation moves the existing moniker value object into Domain, adds minimal Application and Infrastructure projects, and retains help/version-only CLI behavior. It does **not** introduce unapproved inspection implementations.

## Enforcement

The updated Python architecture validator checks solution membership, project references, absence of superseded Core projects, and Native AOT package metadata. CI runs it. Automated structure checks complement SOLID/ports review and behavior tests.

[Binding architecture rules](../../requirements/architecture.md) · [Layer design](../architecture.md).
