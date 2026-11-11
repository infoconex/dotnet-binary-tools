# FR-RPT-006: Preserve explicit unknown, non-managed, skipped, malformed and failed results, including individual provenance and errors; never suppress incomplete-coverage warnings.

**Capability:** [CAP-006 — Results and Reporting](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Preserve explicit unknown, non-managed, skipped, malformed and failed results, including individual provenance and errors; never suppress incomplete-coverage warnings.

## Acceptance scenarios

### SC-RPT-008: Preserve item and inventory status

**Given** a mixed scan has one native result, one unknown target, one unreadable file and an incomplete root

**When** a report is rendered

**Then** each item is distinguishable and aggregate completeness is visible.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
