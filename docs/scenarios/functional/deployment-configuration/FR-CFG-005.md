# FR-CFG-005: Represent configuration absence explicitly as not found; never infer a runtime requirement, installed runtime or actual execution environment from absence.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Represent configuration absence explicitly as not found; never infer a runtime requirement, installed runtime or actual execution environment from absence.

## Acceptance scenarios

### SC-CFG-008: Handle no configuration

**Given** a valid managed assembly has no directly associated supported configuration file

**When** configuration discovery completes

**Then** the configuration state is not found and no requested-runtime value is fabricated.
