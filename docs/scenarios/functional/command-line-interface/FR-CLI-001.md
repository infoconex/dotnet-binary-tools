# FR-CLI-001: Expose the executable as dotnet-binary and, when installed as a .NET Tool, dotnet binary; support help, version, inspect and scan commands.

**Capability:** [CAP-007 — Command-Line Interface](../../../capabilities/CAP-007.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Expose the executable as dotnet-binary and, when installed as a .NET Tool, dotnet binary; support help, version, inspect and scan commands.

## Acceptance scenarios

### SC-CLI-001: Dispatch supported commands

**Given** the CLI is installed or launched as an executable

**When** the user invokes --help, --version, inspect or scan

**Then** commands route to the intended operations, with help and version available without inspected input.
