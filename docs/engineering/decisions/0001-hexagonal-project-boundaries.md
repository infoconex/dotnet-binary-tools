# ADR 0001: Hexagonal, domain-centric architecture with physical project boundaries

**Status: Approved.**

## Context

The product inspects binary metadata, deployment configuration, lifecycle information, and inventories, then reports findings through a CLI. Separating these concerns into independently compiled projects keeps domain behavior testable and prevents external I/O and command-specific details from leaking into application contracts.

## Decision

Adopt SOLID principles and domain-centric Ports and Adapters architecture.

- **Domain** owns pure concepts, facts, and invariants without references to application, infrastructure, or presentation projects.
- **Application** owns use-case coordination and ports defined at the consuming inner boundary; it references Domain.
- **Infrastructure** implements application ports for filesystem access, PE metadata inspection, configuration parsing, lifecycle reference data, network requests, and report persistence; it references Application.
- **CLI** is an outer adapter and composition root, referencing Application and Infrastructure to wire dependencies without owning domain behavior.

Compile-time references point **inward**: Application → Domain, Infrastructure → Application, CLI → Application and Infrastructure. Add adapter-specific infrastructure projects when concrete dependency boundaries justify them, rather than speculatively.

The engineering scaffold contains separate Domain, Application, Infrastructure and CLI projects, plus Domain and CLI tests. Application and Infrastructure remain minimal until functional requirements are implemented; this architectural decision does not claim inspection functionality is complete.

## Enforcement

The CI architecture validator checks solution membership, approved project-reference direction, and Native AOT package settings. Automated structural checks complement SOLID reviews and tests of application contracts and adapter behavior.

[Mandatory architectural constraints](../../requirements/architecture.md) · [Layer design](../architecture.md).
