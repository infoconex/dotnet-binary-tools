# FR-ASM-007: Report optional informational, file, company, product and description metadata only when present, without inventing defaults or treating these values as runtime targets.

**Capability:** [CAP-002 — Assembly Inspection](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Report optional informational, file, company, product and description metadata only when present, without inventing defaults or treating these values as runtime targets.

## Acceptance scenarios

### SC-ASM-010: Preserve optional versions and attributes

**Given** an assembly with different assembly, file and informational versions plus company and product metadata

**When** it is inspected

**Then** the versions remain distinguishable and present optional fields preserve their values.

### SC-ASM-011: Handle absent optional attributes

**Given** an assembly lacking company and informational-version attributes

**When** it is inspected

**Then** those attributes remain explicitly absent or unavailable without substituted placeholders.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
