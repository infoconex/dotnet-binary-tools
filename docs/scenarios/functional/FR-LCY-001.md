# FR-LCY-001: Ship a local, versioned lifecycle reference dataset with identifiable authoritative sources, publication/update date and coverage.

**Capability:** [CAP-004 — Lifecycle Assessment](../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Ship a local, versioned lifecycle reference dataset with identifiable authoritative sources, publication/update date and coverage.

## Acceptance scenarios

### SC-LCY-001: Use bundled data offline

**Given** the tool is installed without network access and includes a known lifecycle dataset version

**When** lifecycle assessment is requested

**Then** the available reference data is used without a network request and its version is reported.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
