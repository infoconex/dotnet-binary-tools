# FR-LCY-006: Expose lifecycle data provenance, dataset version, source publication/fetch dates, assessment date and freshness warnings wherever findings are emitted.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Expose lifecycle data provenance, dataset version, source publication/fetch dates, assessment date and freshness warnings wherever findings are emitted.

## Acceptance scenarios

### SC-LCY-008: Report a stale dataset

**Given** the bundled dataset is older than the documented freshness threshold

**When** an assessment is emitted

**Then** the report exposes its source, dataset version and a freshness warning without silently revising dates.
