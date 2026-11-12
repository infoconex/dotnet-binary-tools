# FR-ASM-003: Extract the exact declared TargetFrameworkAttribute moniker, preserving its original value and evidence source.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Extract the exact declared TargetFrameworkAttribute moniker, preserving its original value and evidence source.

## Acceptance scenarios

### SC-ASM-005: Preserve an explicit target

**Given** a managed assembly declaring .NETFramework,Version=v4.8

**When** the target is read

**Then** the original moniker and its attribute provenance are preserved without replacement by a normalized label.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
