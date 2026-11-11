# FR-RPT-001: Use one documented, versioned JSON envelope for inspect and scan, containing command, inputs, generation metadata, completion, summary, findings and applicable dataset provenance.

**Capability:** [CAP-006 — Results and Reporting](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Use one documented, versioned JSON envelope for inspect and scan, containing command, inputs, generation metadata, completion, summary, findings and applicable dataset provenance.

## Acceptance scenarios

### SC-RPT-001: Use one schema for both commands

**Given** one inspect result and one scan inventory

**When** each is serialized as JSON

**Then** both documents have the same envelope structure and schemaVersion, differing in their contents rather than the top-level contract.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
