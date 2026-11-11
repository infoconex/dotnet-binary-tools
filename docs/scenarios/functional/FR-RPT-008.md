# FR-RPT-008: Never overwrite an existing report: use the requested filename when available or a UTC timestamped and collision-resistant suffix with exclusive creation.

**Capability:** [CAP-006 — Results and Reporting](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Never overwrite an existing report: use the requested filename when available or a UTC timestamped and collision-resistant suffix with exclusive creation.

## Acceptance scenarios

### SC-RPT-011: Do not overwrite an existing file

**Given** the requested report filename already contains unrelated content

**When** the report is saved

**Then** the existing file is byte-for-byte unchanged and a new unique timestamped filename is used.

### SC-RPT-012: Handle simultaneous filename collision

**Given** two report writers request the same name at the same instant

**When** both attempt file creation

**Then** each creates a distinct file or reports a safe failure, with no clobbered output.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
