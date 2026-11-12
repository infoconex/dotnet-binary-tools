# FR-RPT-006: Preserve explicit unknown, non-managed, skipped, malformed and failed results, including individual provenance and errors; never suppress incomplete-coverage warnings.

**Capability:** [CAP-006 — Results and Reporting](../../../capabilities/CAP-006.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Preserve explicit unknown, non-managed, skipped, malformed and failed results, including individual provenance and errors; never suppress incomplete-coverage warnings.

## Acceptance scenarios

### SC-RPT-008: Preserve item and inventory status

**Given** a mixed scan has one native result, one unknown target, one unreadable file and an incomplete root

**When** a report is rendered

**Then** each item is distinguishable and aggregate completeness is visible.
