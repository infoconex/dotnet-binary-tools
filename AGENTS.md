# Repository guidance for human and AI-assisted contributions

These instructions apply to the whole repository. Read [docs/README.md](docs/README.md) before editing.

## Scope, review, and change control

- Work on **one GitHub issue or independently scoped change at a time**. Use `issue/<number>-<slug>` branches from the current `main`; use a dedicated `setup/` branch for reviewed foundation work.
- Open PRs to `main`; do not commit directly to protected `main` once protection is configured. Squash merge only after the owner reviews the exact candidate and all required checks pass.
- Keep issue, FR/NFR identifier, scenario IDs, change scope, executable test evidence, and documentation consistent in each PR.
- Do not publish NuGet packages, create GitHub releases or tags, or merge a PR without explicit authorization. A successful CI run is not publication approval.
- State exactly what was built, executed, and tested. Never invent test results or claim full cross-platform or runtime-free coverage from source review.

## Product identity and platform

- GitHub repository: `infoconex/dotnet-framework`.
- NuGet package ID: **`Infoconex.DotNetFramework.Tools`** (proposed, availability not reserved).
- Command/executable: `dotnet-framework`; users can run `dotnet framework` when installed as a .NET Tool.
- Build target: **.NET 11** (SDK selected by `global.json`); modern C#, nullable references, SDK-style projects.
- Production distribution **goal**: Native AOT standalone executables plus RID-specific NuGet Tool packages. Native AOT/package acceptance must be demonstrated on each supported platform before release.
- Inspect binaries through metadata only. **Never load, execute, invoke, or run an assembly being inspected**; never use `Assembly.Load` or load-context execution to determine its target.

## Engineering standards

- Follow [Microsoft-aligned C# style](docs/engineering/csharp-style.md), the root [`.editorconfig`](.editorconfig), and [architecture](docs/engineering/architecture.md).
- Use PascalCase public identifiers and constants, `I` interfaces, `_camelCase` private instance fields, `s_camelCase` mutable private static fields. CI enforces Roslyn naming via `dotnet format`.
- Prefer small, explicit, immutable models and cohesive, independently testable behavior. Avoid needless abstractions, implicit ambient state, and vendor types in core contracts.
- Keep Core independent of CLI; CLI may reference Core. Tests for Core must not depend on CLI. Architectural rules are review requirements until backed by executable gates.
- All external/public APIs require accurate XML comments; compiler warnings are errors.
- Treat input files as hostile. Bound file reads and traversal; handle malformed metadata, OS errors and symlink cycles. Avoid dumping bytes or sensitive paths into CI logs.
- Use `DotNetFramework.slnx` as the sole solution file and lowercase `nuget.config`; pass `--configfile nuget.config` to explicit restores for deterministic configuration.
- Use explicit packages and centrally managed versions in `Directory.Packages.props`. Review licenses and vulnerability findings before adding dependencies.
- `System.Text.Json` source generation is preferred for future Native AOT-compatible JSON contracts. JSON schema changes require contract tests and review.

## Requirements and TDD

- The v0.1 requirement catalog is under `docs/requirements`, with stable `FR-###` / `NFR-###` identifiers and `FR-###-SC##` / `NFR-###-SC##` scenarios.
- The earlier README `REQ-001`–`REQ-012` numbers are retained via an explicit mapping in the requirements index.
- For a new behavior: first add an independently failing test, then implement the smallest passing change, then refactor.
- For existing behavior: verification-only tests are allowed without manufacturing a red test; state why.
- Include positive, negative, edge, malicious, and platform-specific cases as appropriate. Use sanitized synthetic binary fixtures.
- Maintain `docs/testing/traceability.md` with scenario, test, status, and actual run SHA. A written test is not proof of a successful run.
- Keep inspection and scanning unimplemented on this setup branch; current CLI only exposes informational commands and rejects other commands.

## Build and review

- Typical checks: `dotnet restore DotNetFramework.slnx --configfile nuget.config`, `dotnet build DotNetFramework.slnx -c Release --no-restore`, `dotnet test DotNetFramework.slnx -c Release --no-build`, `dotnet format style DotNetFramework.slnx --verify-no-changes --no-restore --diagnostics IDE1006 --severity error`.
- Native AOT builds are separate platform-specific gates. Do not equate a managed build with successful AOT publication.
- PRs include exact commands, observable counts, runs, scope exclusions, dependency changes, security impact, and any docs/traceability updates.
- For a solo-maintainer project, distinguish an owner second-pass review from independent review; explicit owner GO is required for release and publication.
