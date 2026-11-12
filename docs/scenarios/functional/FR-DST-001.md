# FR-DST-001: Ship independently runnable Native AOT standalone executables for Windows x64, Linux x64 and macOS x64, with correct host executable packaging.

**Capability:** [CAP-008 — Distribution](../../capabilities/CAP-008.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Ship independently runnable Native AOT standalone executables for Windows x64, Linux x64 and macOS x64, with correct host executable packaging.

## Acceptance scenarios

### SC-DST-001: Run a native executable for each supported OS

**Given** a Native AOT artifact built for win-x64, linux-x64 or osx-x64 is provided to its matching OS

**When** the artifact is launched

**Then** the executable starts and its CLI help/version output works without invoking an installed application runtime.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
