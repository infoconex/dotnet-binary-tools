# FR-DST-005: Demonstrate standalone Native AOT runtime independence on clean consumers, and separately verify tool installation and invocation requirements.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Demonstrate standalone Native AOT runtime independence on clean consumers, and separately verify tool installation and invocation requirements.

## Acceptance scenarios

### SC-DST-005: Run without installed .NET

**Given** a clean consumer OS lacks the inspector .NET runtime

**When** the standalone native executable is launched

**Then** it runs successfully without installing .NET.

### SC-DST-006: Distinguish tool installation prerequisites

**Given** a consumer has the SDK/tool manager needed to install .NET tools

**When** the NuGet tool is installed and invoked

**Then** the documented installation and native execution expectations are separately verified.
