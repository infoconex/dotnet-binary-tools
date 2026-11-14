# FR-ASM-003: Extract the exact declared TargetFrameworkAttribute moniker, preserving its original value and evidence source.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Extract the exact declared TargetFrameworkAttribute moniker, preserving its original value and evidence source.

## Acceptance scenarios

### SC-ASM-005: Preserve an explicit target

**Given** a managed assembly declaring .NETFramework,Version=v4.8

**When** the target is read

**Then** the original moniker and its attribute provenance are preserved without replacement by a normalized label.
