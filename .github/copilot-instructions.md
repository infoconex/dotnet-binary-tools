# GitHub Copilot guidance

Follow [AGENTS.md](../AGENTS.md) and [docs/README.md](../docs/README.md) as authoritative project guidance.

- Keep the project on .NET 11 with modern SDK-style builds and compatible language features.
- Enforce Microsoft's C# conventions using `.editorconfig`, nullable reference types, meaningful XML API comments and scoped architecture boundaries.
- Work in one issue-scoped PR; verify the exact current branch and avoid changes to `main` without review.
- Trace FR/NFR scenario specifications to NUnit tests and actual GitHub Actions evidence. Use red/green/refactor for missing functionality.
- Read compiled files through `System.Reflection.Metadata` and `PEReader` without executing them.
- Keep Core vendor-agnostic and independent from CLI; ensure future JSON output uses AOT-compatible serialization.
- Treat every binary as untrusted; bound traversal and resource consumption; do not disclose source bytes in logs.
- Never claim Native AOT, CI, NuGet or production compatibility success without executable proof.
- No package publication or GitHub release without explicit owner approval.
