# FR-LCY-008: On refresh failure, preserve last known valid data, warn about the failure and mark assessment provenance honestly; never install partial or untrusted data.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

On refresh failure, preserve last known valid data, warn about the failure and mark assessment provenance honestly; never install partial or untrusted data.

## Acceptance scenarios

### SC-LCY-011: Retain the prior dataset on failure

**Given** the refresh endpoint is unreachable or returns malformed data and a valid bundled dataset is present

**When** an explicit refresh is attempted

**Then** the old trusted dataset remains usable, the error is reported and no partial replacement is installed.
