# FR-ASM-010: Expose basic file facts and optional SHA-256 content hashing on explicit request, with hashing subject to resource and read-failure safeguards.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Expose basic file facts and optional SHA-256 content hashing on explicit request, with hashing subject to resource and read-failure safeguards.

## Acceptance scenarios

### SC-ASM-015: Report file facts

**Given** a known sample with file length and modification timestamp

**When** the file is inspected

**Then** file facts are returned independently of assembly attributes.

### SC-ASM-016: Hash only when requested

**Given** a known fixture and an optional hash request

**When** CAP-002 processes the file

**Then** the returned SHA-256 matches the file content, and the hash is omitted when not requested.
