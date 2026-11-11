# FR-CFG-008: Preserve configuration provenance (path, association mode and field origin) and never claim that a requested runtime is installed or running.

**Capability:** [CAP-003 — Deployment Configuration](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Preserve configuration provenance (path, association mode and field origin) and never claim that a requested runtime is installed or running.

## Acceptance scenarios

### SC-CFG-012: Trace a runtime request to its file

**Given** an associated configuration contains a supported runtime declaration

**When** CAP-003 emits its result

**Then** the exact source file and configuration key or element are available and no observed-runtime statement appears.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
