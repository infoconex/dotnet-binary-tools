# FR-DST-006: Require real build, smoke execution and relevant feature acceptance on each supported RID in CI or equivalent recorded release validation, never substituting a managed build for AOT evidence.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Require real build, smoke execution and relevant feature acceptance on each supported RID in CI or equivalent recorded release validation, never substituting a managed build for AOT evidence.

## Acceptance scenarios

### SC-DST-007: Gate a supported RID

**Given** a release candidate passes managed unit tests but has no Native AOT verification on one advertised OS

**When** release verification is evaluated

**Then** that RID remains unverified and the release gate does not pass.
