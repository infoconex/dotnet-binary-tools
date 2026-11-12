# FR-LCY-003: Assess requested deployment runtime versions independently from assembly target lifecycles, never substituting one for the other.

**Capability:** [CAP-004 — Lifecycle Assessment](../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Assess requested deployment runtime versions independently from assembly target lifecycles, never substituting one for the other.

## Acceptance scenarios

### SC-LCY-004: Separate target and configuration status

**Given** an assembly target and its runtimeconfig request refer to different versions

**When** lifecycle assessment runs

**Then** two distinct assessments are produced and retain their individual provenance.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
