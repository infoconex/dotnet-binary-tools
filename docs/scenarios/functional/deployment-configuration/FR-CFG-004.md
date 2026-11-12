# FR-CFG-004: Parse modern .runtimeconfig.json runtimeOptions fields needed to describe TFM, framework/frameworks names and versions, and declared runtime-selection policies.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Parse modern .runtimeconfig.json runtimeOptions fields needed to describe TFM, framework/frameworks names and versions, and declared runtime-selection policies.

## Acceptance scenarios

### SC-CFG-006: Read framework and roll-forward policy

**Given** a valid runtimeconfig.json declares runtimeOptions.tfm, framework name/version and rollForward

**When** the file is parsed

**Then** the TFM, framework requirement and roll-forward policy are preserved as configured values.

### SC-CFG-007: Read multiple framework requirements

**Given** a valid runtimeconfig.json contains runtimeOptions.frameworks with two entries

**When** the file is parsed

**Then** both framework requests remain separate and in their declared form.
