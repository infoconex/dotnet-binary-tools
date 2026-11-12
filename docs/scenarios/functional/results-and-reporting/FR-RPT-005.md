# FR-RPT-005: Reject incompatible --details and --fields choices and report unsupported field names through the CLI’s validation contract.

**Capability:** [CAP-006 — Results and Reporting](../../../capabilities/CAP-006.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Reject incompatible --details and --fields choices and report unsupported field names through the CLI’s validation contract.

## Acceptance scenarios

### SC-RPT-007: Reject conflicting selections

**Given** both --details and --fields are provided

**When** the CLI validates output options

**Then** the request is rejected before producing a report with an actionable diagnostic.
