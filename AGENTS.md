# Repository instructions for human and AI contributions

Read [docs/README.md](docs/README.md), [capabilities](docs/capabilities.md), and [mandatory architecture](docs/requirements/architecture.md) before changes.

## Governance

- Work through reviewed branches and issue-scoped changes; do not commit directly to `main` without approval.
- Do not merge, tag, release or publish NuGet packages without explicit owner authorization.
- Maintain independent stable CAP, FR/NFR, ARCH and scenario IDs; trace scenarios to tests and **observed** exact-SHA CI evidence. Draft scenarios are not passing tests.
- All eight capabilities now have an approved specification baseline: 71 functional requirements and 102 independently numbered acceptance scenarios (`SC-INP-###`, `SC-ASM-###`, etc.). Every scenario remains Planned until verified.

## Architecture

- ARCH-001–ARCH-008 are **mandatory**: SOLID, domain-centered hexagonal (Ports and Adapters), separately compiled Domain/Application/Infrastructure/CLI, and inward-only compile-time project references.
- Domain contains pure invariants, Application owns use cases and consumer ports, Infrastructure implements ports, and CLI is an outer entry point and composition root.
- Do not put PE parsing, filesystem I/O, runtime config readers, network data sources or report persistence in Domain. Do not couple Application to concrete outer adapters.
- Never execute, load or invoke inspected candidate assemblies. Keep declared targets, configured runtime requirements and observed runtimes distinct.
- Native AOT standalone binaries and the NuGet tool are v0.1 distribution goals for Windows/Linux/macOS **x64**; ARM64 is deferred.

## Development

- Use .NET 11, `DotNetFramework.slnx`, root lowercase `nuget.config`, centralized package versions, and Microsoft-aligned `.editorconfig`.
- For new functionality, write meaningful failing tests first, implement minimally, then refactor. Ensure independent architecture, component, integration and negative tests as appropriate.
- Do not claim `inspect`/`scan` or lifecycle, config, JSON, AOT runtime-free or installation acceptance until actual code and observed evidence exist.
- Use `python3 scripts/validate_docs.py`, `python3 scripts/validate_architecture.py`, `python3 scripts/validate_test_metadata.py` in addition to build/NUnit/Native AOT checks.
- Require accurate public XML documentation, safe error messages, non-sensitive synthetic fixtures, versioned JSON contracts and precise PR verification claims.
