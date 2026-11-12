# FR-CFG-007: Keep each requested deployment runtime separate from the inspected assembly’s declared target, including when values disagree.

**Capability:** [CAP-003 — Deployment Configuration](../../capabilities/CAP-003.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Keep each requested deployment runtime separate from the inspected assembly’s declared target, including when values disagree.

## Acceptance scenarios

### SC-CFG-011: Preserve conflicting declarations

**Given** an assembly declares one TFM while its directly associated runtimeconfig specifies a different runtime version

**When** combined inspection runs

**Then** both declarations appear with separate source labels and no silent reconciliation or compatibility conclusion.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
