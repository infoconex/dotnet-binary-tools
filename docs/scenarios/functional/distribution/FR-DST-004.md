# FR-DST-004: Publish versioned platform-specific artifacts with verifiable package contents, checksums and appropriate provenance, without claiming unsupported architectures.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Publish versioned platform-specific artifacts with verifiable package contents, checksums and appropriate provenance, without claiming unsupported architectures.

## Acceptance scenarios

### SC-DST-004: Verify release contents

**Given** a release candidate has platform packages and checksums

**When** artifact inventory is audited

**Then** each declared x64 RID has correctly named binaries and matching version/checksums while no ARM64 package is advertised.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
