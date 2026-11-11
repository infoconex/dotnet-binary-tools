# FR-ASM-009: Report declared entry-point presence and direct assembly references, without constructing a dependency graph or associating an application host with adjacent files.

**Capability:** [CAP-002 — Assembly Inspection](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Report declared entry-point presence and direct assembly references, without constructing a dependency graph or associating an application host with adjacent files.

## Acceptance scenarios

### SC-ASM-013: Report direct references and entry point

**Given** an assembly with a declared entry point and known AssemblyRef metadata

**When** the file is inspected

**Then** the entry-point fact and direct reference identities are included without resolving dependencies.

### SC-ASM-014: Avoid apphost inference

**Given** a native .exe beside a managed .dll

**When** both files are inspected independently

**Then** no host-to-library association or inferred target appears.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
