# FR-DST-007: Require separate owner approval before tagging, releasing or publishing packages and defer ARM64 until its own verified acceptance evidence exists.

**Capability:** [CAP-008 — Distribution](../../capabilities/CAP-008.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Require separate owner approval before tagging, releasing or publishing packages and defer ARM64 until its own verified acceptance evidence exists.

## Acceptance scenarios

### SC-DST-008: Block unauthorized publication

**Given** a candidate PR has green checks but no owner release authorization

**When** release automation is considered

**Then** no public tag, GitHub Release or NuGet push occurs.

### SC-DST-009: Avoid unsupported ARM64 claims

**Given** ARM64 packages have not completed platform-specific acceptance

**When** supported RID metadata is generated

**Then** ARM64 is absent from the v0.1 advertised release set.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
