# FR-RPT-005: Reject incompatible --details and --fields choices and report unsupported field names through the CLI’s validation contract.

**Capability:** [CAP-006 — Results and Reporting](../../../capabilities/CAP-006.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Reject incompatible --details and --fields choices and report unsupported field names through the CLI’s validation contract.

## Acceptance scenarios

### SC-RPT-007: Reject conflicting selections

**Given** both --details and --fields are provided

**When** the CLI validates output options

**Then** the request is rejected before producing a report with an actionable diagnostic.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
