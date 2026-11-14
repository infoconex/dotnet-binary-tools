# FR-DST-003: Ensure the standalone executable and NuGet tool deliver identical functional CLI behavior and report contract for the same fixtures and version.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Ensure the standalone executable and NuGet tool deliver identical functional CLI behavior and report contract for the same fixtures and version.

## Acceptance scenarios

### SC-DST-003: Compare two distribution forms

**Given** standalone and installed tool artifacts of the same version are available on one supported OS

**When** identical inspect and scan fixtures are processed

**Then** the semantic findings, schema version and completion status are equivalent.
