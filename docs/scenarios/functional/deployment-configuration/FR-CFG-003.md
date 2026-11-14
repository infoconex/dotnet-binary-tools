# FR-CFG-003: Parse supported .NET Framework executable configuration runtime declarations including startup supportedRuntime version and sku when present, preserving raw values.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Parse supported .NET Framework executable configuration runtime declarations including startup supportedRuntime version and sku when present, preserving raw values.

## Acceptance scenarios

### SC-CFG-005: Read a Framework runtime declaration

**Given** MyApp.exe.config contains startup supportedRuntime version and sku values

**When** the file is parsed

**Then** the requested Framework/CLR configuration is reported as declared, with exact values and source.
