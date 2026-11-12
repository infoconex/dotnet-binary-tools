# FR-LCY-002: Assess known .NET Framework, .NET Core and modern .NET target identities using dated published support information, subject to source-specific applicability limits.

**Capability:** [CAP-004 — Lifecycle Assessment](../../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Assess known .NET Framework, .NET Core and modern .NET target identities using dated published support information, subject to source-specific applicability limits.

## Acceptance scenarios

### SC-LCY-002: Assess a known target as of a date

**Given** a known framework target and a bundled reference entry with lifecycle dates

**When** assessment occurs on a specified assessment date

**Then** the reported lifecycle category follows the applicable reference dates and identifies the assessed target.

### SC-LCY-003: Qualify environment-dependent support

**Given** a .NET Framework version whose support is affected by operating-system lifecycle

**When** assessment runs without host OS evidence

**Then** the output states the applicability limitation rather than universally claiming the deployment is supported.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../../testing/traceability.md).
