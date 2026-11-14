# FR-LCY-007: Refresh lifecycle data only when explicitly requested, obtain it from approved authoritative sources, validate and atomically replace cached data, and never allow network access in the default path.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md) · [Verification evidence](../../../testing/traceability.md)

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
