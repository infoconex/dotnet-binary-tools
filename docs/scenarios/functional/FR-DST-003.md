# FR-DST-003: Ensure the standalone executable and NuGet tool deliver identical functional CLI behavior and report contract for the same fixtures and version.

**Capability:** [CAP-008 — Distribution](../../capabilities/CAP-008.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Ensure the standalone executable and NuGet tool deliver identical functional CLI behavior and report contract for the same fixtures and version.

## Acceptance scenarios

### SC-DST-003: Compare two distribution forms

**Given** standalone and installed tool artifacts of the same version are available on one supported OS

**When** identical inspect and scan fixtures are processed

**Then** the semantic findings, schema version and completion status are equivalent.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
