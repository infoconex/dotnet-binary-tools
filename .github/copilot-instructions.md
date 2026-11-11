# GitHub Copilot guidance

Use [AGENTS.md](../AGENTS.md), [capabilities](../docs/capabilities.md) and [ARCH-001–ARCH-008](../docs/requirements/architecture.md).

- Follow SOLID and hexagonal Ports-and-Adapters; Domain, Application, Infrastructure and CLI are independent compiled projects with inward-only references.
- CLI stays thin. Define ports with inner consuming use cases, and keep concrete filesystem/metadata/configuration/network/report adapters external.
- Use .NET 11, Microsoft naming conventions, nullable and public XML documentation.
- Work from approved CAP and FR/NFR/ARCH identifiers. CAP-001 uses `FR-INP-###` requirements and independent `SC-INP-###` scenario IDs. Earlier `FR-001` drafts are retired.
- Use TDD for new functionality, cross-platform fixtures, security and negative tests. Record only actual passing CI evidence.
- Never execute or load inspected binaries. Maintain AOT-compatible output contracts and no unsupported runtime-inference claims.
- Do not merge, tag, publish or release without explicit owner authorization.
