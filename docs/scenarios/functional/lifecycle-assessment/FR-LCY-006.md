# FR-LCY-006: Expose lifecycle data provenance, dataset version, source publication/fetch dates, assessment date and freshness warnings wherever findings are emitted.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Expose lifecycle data provenance, dataset version, source publication/fetch dates, assessment date and freshness warnings wherever findings are emitted.

## Acceptance scenarios

### SC-LCY-008: Report a stale dataset

**Given** the bundled dataset is older than the documented freshness threshold

**When** an assessment is emitted

**Then** the report exposes its source, dataset version and a freshness warning without silently revising dates.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
