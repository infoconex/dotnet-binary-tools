# FR-ASM-006: Extract available assembly identity fields: simple name, assembly version, culture, public-key token and full assembly identity.

**Capability:** [CAP-002 — Assembly Inspection](../../../capabilities/CAP-002.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Extract available assembly identity fields: simple name, assembly version, culture, public-key token and full assembly identity.

## Acceptance scenarios

### SC-ASM-009: Read complete assembly identity

**Given** a fixture with known name, version, culture and public key information

**When** the fixture is inspected

**Then** each available identity field matches the metadata and the full identity is not confused with file version.
