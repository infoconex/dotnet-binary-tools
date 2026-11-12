# FR-CLI-001: Expose the executable as dotnet-binary and, when installed as a .NET Tool, dotnet binary; support help, version, inspect and scan commands.

**Capability:** [CAP-007 — Command-Line Interface](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Expose the executable as dotnet-binary and, when installed as a .NET Tool, dotnet binary; support help, version, inspect and scan commands.

## Acceptance scenarios

### SC-CLI-001: Dispatch supported commands

**Given** the CLI is installed or launched as an executable

**When** the user invokes --help, --version, inspect or scan

**Then** commands route to the intended operations, with help and version available without inspected input.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
