# FR-CFG-007: Keep each requested deployment runtime separate from the inspected assembly’s declared target, including when values disagree.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Keep each requested deployment runtime separate from the inspected assembly’s declared target, including when values disagree.

## Acceptance scenarios

### SC-CFG-011: Preserve conflicting declarations

**Given** an assembly declares one TFM while its directly associated runtimeconfig specifies a different runtime version

**When** combined inspection runs

**Then** both declarations appear with separate source labels and no silent reconciliation or compatibility conclusion.
