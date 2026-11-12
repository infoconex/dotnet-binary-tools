# FR-DST-006: Require real build, smoke execution and relevant feature acceptance on each supported RID in CI or equivalent recorded release validation, never substituting a managed build for AOT evidence.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Require real build, smoke execution and relevant feature acceptance on each supported RID in CI or equivalent recorded release validation, never substituting a managed build for AOT evidence.

## Acceptance scenarios

### SC-DST-007: Gate a supported RID

**Given** a release candidate passes managed unit tests but has no Native AOT verification on one advertised OS

**When** release verification is evaluated

**Then** that RID remains unverified and the release gate does not pass.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
