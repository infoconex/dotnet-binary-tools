# FR-ASM-004: Recognize declared .NET Framework, .NET Core, .NET 5+ and .NET Standard families and versions, while preserving unknown or future monikers.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Recognize declared .NET Framework, .NET Core, .NET 5+ and .NET Standard families and versions, while preserving unknown or future monikers.

## Acceptance scenarios

### SC-ASM-006: Recognize supported framework families

**Given** synthetic managed assemblies with declarations for .NET Framework, .NET Core, modern .NET and .NET Standard

**When** they are inspected

**Then** their declared framework families and versions are identified separately without treating .NET Standard as a runtime.

### SC-ASM-007: Preserve an unfamiliar moniker

**Given** a valid managed assembly carrying an unfamiliar or future target moniker

**When** the target is interpreted

**Then** the raw value is retained and the normalized identity is reported as unrecognized rather than guessed.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
