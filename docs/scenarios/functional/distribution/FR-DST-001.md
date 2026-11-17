# FR-DST-001: Ship independently runnable Native AOT standalone executables for Windows, Linux and macOS on x64 and ARM64, with correct host executable packaging.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Ship independently runnable Native AOT standalone executables for Windows, Linux and macOS on x64 and ARM64, with correct host executable packaging.

## Acceptance scenarios

### SC-DST-001: Run a native executable for each supported OS

**Given** a Native AOT artifact built for any of the six approved x64/ARM64 RIDs is provided to its matching native OS and CPU architecture

**When** the artifact is launched

**Then** the executable starts and its CLI help/version output works without invoking an installed application runtime.
