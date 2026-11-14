# Contributing to .NET Binary Tools

Thank you for helping improve the project. It is currently an **engineering
scaffold and approved specification**, not a released binary-inspection tool.
Start with the [product and engineering documentation](docs/README.md),
[capabilities](docs/capabilities/README.md), and [mandatory architectural
constraints](docs/requirements/architecture.md). Repository-specific guidance
also lives in [AGENTS.md](AGENTS.md).

## Choose an issue-sized change

1. Open or reference a GitHub issue and identify the affected capability,
   requirement, scenario, architectural constraint, or engineering concern.
2. Branch from the current `main` using `issue/<number>-<short-description>`.
   Do not commit directly to `main`.
3. Keep the change focused. Preserve existing CAP, FR, NFR, ARCH, and scenario
   identifiers; propose specification changes explicitly rather than silently
   renumbering or rewriting approved requirements.
4. For new behavior, start with a failing, meaningful test. Implement the
   smallest passing behavior and refactor. For defects, reproduce the failure
   first. Documentation-only or verification-only changes should identify why
   a new failing behavior test is not appropriate.
5. Open a pull request linked to the issue and provide the commands, observed
   results, exact commit SHA, supported operating systems, and remaining gaps.

## Architecture and safe inputs

Follow [ARCH-001–ARCH-008](docs/requirements/architecture.md): SOLID,
hexagonal architecture, separate Domain/Application/Infrastructure/CLI
projects, and inward-only compile-time dependencies. Put adapters and external
I/O outside Domain; the CLI is the composition root, not the application logic.

Always treat inspected binaries as untrusted data. **Never execute or load
candidate assemblies** to inspect them. Cover corrupt metadata, unexpected
paths, links, access failures, resource limits, and platform differences with
controlled, non-sensitive test fixtures. Follow the [testing
strategy](docs/testing/strategy.md).

Use the existing Microsoft-aligned code style, central build and NuGet
configuration, and normalized line endings; do not add a new tool or
configuration file without a clear, demonstrated purpose.

## Run the relevant validations

The [CI/local validation guide](docs/engineering/ci-local-validation.md) lists
the current commands, platform prerequisites, workflow behavior, and the
approved plan to move reusable validation logic out of GitHub Actions YAML.
Run the relevant Python validators and .NET 11/NUnit/Native AOT checks for your
change. CI runs on Windows, Linux, and macOS where applicable. Preserve
existing validators and do not report skipped or unexecuted checks as passes.

For changes implementing an approved scenario, update only the authoritative
[traceability matrix](docs/testing/traceability.md) with **observed** test and
CI evidence. Do not duplicate status throughout capability or scenario files.

## Pull requests, security, and publication

- Use the [pull request template](.github/pull_request_template.md) and link the
  corresponding issue. Disclose risks, limitations, and any checks that could
  not run.
- The owner performs a documented second-pass review; zero mandatory GitHub
  approving reviews does not mean review is optional. Do not merge without
  explicit owner approval.
- A merged PR is not authorization to create a tag, release, standalone
  download, or NuGet package. See the [release
  gates](docs/engineering/release-gates.md).
- Report suspected vulnerabilities **privately** following
  [SECURITY.md](SECURITY.md), never in a public issue or PR.
