# FR-DST-007: Require separate owner approval before tagging, releasing or publishing packages and exclude targets lacking verified platform-specific acceptance.

**Capability:** [CAP-008 — Distribution](../../../capabilities/CAP-008.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Require separate owner approval before tagging, releasing or publishing packages and exclude any target lacking verified platform-specific acceptance evidence.

## Acceptance scenarios

### SC-DST-008: Block unauthorized publication

**Given** a candidate PR has green checks but no owner release authorization

**When** release automation is considered

**Then** no public tag, GitHub Release or NuGet push occurs.

### SC-DST-009: Avoid unsupported target claims

**Given** an unknown RID or a release target that has not completed native and clean-consumer acceptance

**When** supported RID metadata is generated

**Then** unknown RID packages are rejected and the unverified target is not advertised as release-ready; packaging alone is not evidence of compatibility.
