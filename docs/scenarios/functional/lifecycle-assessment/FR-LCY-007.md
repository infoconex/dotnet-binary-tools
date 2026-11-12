# FR-LCY-007: Refresh lifecycle data only when explicitly requested, obtain it from approved authoritative sources, validate and atomically replace cached data, and never allow network access in the default path.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Refresh lifecycle data only when explicitly requested, obtain it from approved authoritative sources, validate and atomically replace cached data, and never allow network access in the default path.

## Acceptance scenarios

### SC-LCY-009: Default mode stays offline

**Given** a tool has network access but no refresh option was passed

**When** lifecycle assessment runs

**Then** no network refresh is attempted.

### SC-LCY-010: Refresh only verified source data

**Given** the user expressly requests refresh and a trusted source returns valid data

**When** CAP-004 refreshes reference data

**Then** validated data is used and its recorded version/provenance updates atomically.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
