# FR-CFG-005: Represent configuration absence explicitly as not found; never infer a runtime requirement, installed runtime or actual execution environment from absence.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Represent configuration absence explicitly as not found; never infer a runtime requirement, installed runtime or actual execution environment from absence.

## Acceptance scenarios

### SC-CFG-008: Handle no configuration

**Given** a valid managed assembly has no directly associated supported configuration file

**When** configuration discovery completes

**Then** the configuration state is not found and no requested-runtime value is fabricated.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
