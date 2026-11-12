# FR-DST-002: Provide an installable, RID-aware NuGet .NET Tool package using the approved package ID and command dispatch on each supported platform.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Provide an installable, RID-aware NuGet .NET Tool package using the approved package ID and command dispatch on each supported platform.

## Acceptance scenarios

### SC-DST-002: Install the .NET Tool on a supported host

**Given** a published test package for the host RID and a compatible dotnet tool installation environment

**When** the user installs Infoconex.DotNetBinary.Tools

**Then** the dotnet binary command becomes available and dispatches to the installed native tool.
