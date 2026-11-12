# FR-LCY-005: Emit advisory, non-enforcing statuses with assessment date, source and limitations; do not decide actual application compliance or change CLI success based on lifecycle findings.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Emit advisory, non-enforcing statuses with assessment date, source and limitations; do not decide actual application compliance or change CLI success based on lifecycle findings.

## Acceptance scenarios

### SC-LCY-007: Keep advisories non-enforcing

**Given** a reported target has a source-backed end-of-support date in the past

**When** inventory reporting completes

**Then** the lifecycle advisory appears but does not convert an otherwise valid scan into a nonzero exit or compliance judgment.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
