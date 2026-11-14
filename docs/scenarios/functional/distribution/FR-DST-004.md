# FR-DST-004: Publish versioned platform-specific artifacts with verifiable package contents, checksums and appropriate provenance, without claiming unsupported architectures.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Publish versioned platform-specific artifacts with verifiable package contents, checksums and appropriate provenance, without claiming unsupported architectures.

## Acceptance scenarios

### SC-DST-004: Verify release contents

**Given** a release candidate has platform packages and checksums

**When** artifact inventory is audited

**Then** each declared x64 RID has correctly named binaries and matching version/checksums while no ARM64 package is advertised.
