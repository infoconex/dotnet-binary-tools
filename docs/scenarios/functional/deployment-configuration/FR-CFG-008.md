# FR-CFG-008: Preserve configuration provenance (path, association mode and field origin) and never claim that a requested runtime is installed or running.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Preserve configuration provenance (path, association mode and field origin) and never claim that a requested runtime is installed or running.

## Acceptance scenarios

### SC-CFG-012: Trace a runtime request to its file

**Given** an associated configuration contains a supported runtime declaration

**When** CAP-003 emits its result

**Then** the exact source file and configuration key or element are available and no observed-runtime statement appears.
