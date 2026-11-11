# FR-CFG-003: Parse supported .NET Framework executable configuration runtime declarations including startup supportedRuntime version and sku when present, preserving raw values.

**Capability:** [CAP-003 — Deployment Configuration](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Parse supported .NET Framework executable configuration runtime declarations including startup supportedRuntime version and sku when present, preserving raw values.

## Acceptance scenarios

### SC-CFG-005: Read a Framework runtime declaration

**Given** MyApp.exe.config contains startup supportedRuntime version and sku values

**When** the file is parsed

**Then** the requested Framework/CLR configuration is reported as declared, with exact values and source.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
