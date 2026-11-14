# FR-LCY-001: Ship a local, versioned lifecycle reference dataset with identifiable authoritative sources, publication/update date and coverage.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Ship a local, versioned lifecycle reference dataset with identifiable authoritative sources, publication/update date and coverage.

## Acceptance scenarios

### SC-LCY-001: Use bundled data offline

**Given** the tool is installed without network access and includes a known lifecycle dataset version

**When** lifecycle assessment is requested

**Then** the available reference data is used without a network request and its version is reported.
