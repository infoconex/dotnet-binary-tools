# FR-LCY-003: Assess requested deployment runtime versions independently from assembly target lifecycles, never substituting one for the other.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Assess requested deployment runtime versions independently from assembly target lifecycles, never substituting one for the other.

## Acceptance scenarios

### SC-LCY-004: Separate target and configuration status

**Given** an assembly target and its runtimeconfig request refer to different versions

**When** lifecycle assessment runs

**Then** two distinct assessments are produced and retain their individual provenance.
