# Testing strategy and TDD policy

Adapted from RecordSift's requirements-driven approach.

## Red / Green / Refactor

For each FR/NFR, read the authoritative `docs/scenarios/<category>/<ID>.md` and classify work as:
- **New capability:** create a meaningful failing test, implement minimally, refactor with all tests green.
- **Defect:** capture a reproducible failing regression test before fixing the behavior.
- **Acceptance verification only:** if the behavior exists, add independent evidence without manufacturing an intentionally failing test.
- **Engineering foundation:** small tests can verify project wiring and honest CLI help behavior, but must not claim feature acceptance.

Scenarios use `FR-###-SC##` and `NFR-###-SC##`. Traceability captures exact test name, observed result, workflow run, and SHA. No status is upgraded to Automated merely because test source exists.

## Test levels

| Level | Purpose |
| --- | --- |
| Unit | Target moniker parsing, normalized mapping, status and error-code rules |
| Component | In-memory synthetic managed PE and metadata-reader tests |
| Integration | Real synthetic DLL/EXE fixtures, single-file CLI behavior and directory traversal |
| Contract | Stable output JSON schema, exit codes, error isolation, public Core APIs |
| Negative/security | Corrupt/truncated/oversized binaries, malformed metadata, symlink cycles, restricted file access |
| Cross-platform | Equivalent test fixtures on Windows, Linux and macOS |
| Deployment | AOT executable and NuGet Tool install on supported RIDs, including runtime-free consumer |

Tests should be deterministic, use no customer/proprietary binaries or secrets, and avoid hidden dependence on local SDKs or current time. Preserve test fixture origins and hashes.

## Smoke baseline vs acceptance

The engineering-foundation branch tests a raw-moniker value object and a deliberately unimplemented CLI. **It has not satisfied FR-001 through FR-008.** The scenario traceability matrix remains Planned until real implementation and execution evidence exist.

## AOT release proof

Treat managed Debug/Release tests and AOT smoke tests as distinct gates. AOT compilation on a CI runner that has .NET installed does **not** prove that the resulting binary works on a host without .NET; include a separate clean consumer test before release.
