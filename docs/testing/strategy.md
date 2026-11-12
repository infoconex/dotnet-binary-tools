# Testing strategy and TDD policy

The approved 71 functional requirements and 102 Planned scenarios map to the authoritative product baseline [CAP-001–CAP-008](../capabilities/README.md) and the mandatory architecture rules are [ARCH-001–ARCH-008](../requirements/architecture.md).

## Test-first workflow

For a new approved requirement, begin with a meaningful failing test, implement the minimum behavior, then refactor with a passing suite. For defects, capture a reproducing regression. For pre-existing behavior, verification-only tests are acceptable if clearly identified; do not manufacture red tests. Record **observed** acceptance evidence in [traceability](traceability.md), including exact test name, run URL and commit SHA.

## Test levels

| Level | Focus |
| --- | --- |
| Domain unit | Invariants, immutable facts and honest unknown-target states. |
| Application unit | Input resolution and use-case behavior against ports, independent of the CLI. |
| Adapter component | Filesystem, metadata and configuration operations with controlled fixtures. |
| CLI integration | Argument validation, stdout/stderr, stable JSON, report file behavior and exit semantics. |
| Negative/security | Corrupt binaries, malformed metadata, oversized data, access failures, symlink loops, and resource limits. |
| Cross-platform | Windows, Linux and macOS x64 behavior, including platform-specific path/link rules. |
| Distribution | Native AOT execution without installed .NET and NuGet tool installation on clean consumers. |

The approved scenario strategy is a **focused** set of normative Given/When/Then cases plus independently traceable tests for Unicode and spaces in paths, case sensitivity, permissions, links, and other boundary variations.

Architectural checks enforce compiled-layer dependency direction. They complement but do not replace SOLID review, component tests or functional acceptance. The current CLI supports only help/version; its tests do not establish inspection behavior.
