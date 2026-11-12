# FR-ASM-011: Preserve partial trustworthy metadata and record field-level problems when optional metadata is malformed, without converting unreadable content into confident findings.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Preserve partial trustworthy metadata and record field-level problems when optional metadata is malformed, without converting unreadable content into confident findings.

## Acceptance scenarios

### SC-ASM-017: Isolate an optional metadata fault

**Given** a structurally valid managed fixture containing a malformed optional attribute

**When** the fixture is inspected

**Then** independently verified fields remain available and the malformed attribute has an explicit issue without a fabricated replacement.
