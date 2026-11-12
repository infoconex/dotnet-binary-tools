# FR-LCY-008: On refresh failure, preserve last known valid data, warn about the failure and mark assessment provenance honestly; never install partial or untrusted data.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

On refresh failure, preserve last known valid data, warn about the failure and mark assessment provenance honestly; never install partial or untrusted data.

## Acceptance scenarios

### SC-LCY-011: Retain the prior dataset on failure

**Given** the refresh endpoint is unreachable or returns malformed data and a valid bundled dataset is present

**When** an explicit refresh is attempted

**Then** the old trusted dataset remains usable, the error is reported and no partial replacement is installed.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
