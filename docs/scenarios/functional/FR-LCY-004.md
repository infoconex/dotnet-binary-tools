# FR-LCY-004: Treat .NET Standard as an API specification rather than an installed runtime; handle unknown, unsupported-to-classify or future versions without making up dates.

**Capability:** [CAP-004 — Lifecycle Assessment](../../capabilities/CAP-004.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

## Requirement

Treat .NET Standard as an API specification rather than an installed runtime; handle unknown, unsupported-to-classify or future versions without making up dates.

## Acceptance scenarios

### SC-LCY-005: Do not assign a runtime lifecycle to .NET Standard

**Given** an assembly declares .NETStandard,Version=v2.0

**When** lifecycle assessment runs

**Then** the result explains that standard compatibility is not a runtime lifecycle status.

### SC-LCY-006: Do not fabricate an unknown lifecycle

**Given** an unrecognized target or an absent dataset entry is assessed

**When** assessment runs

**Then** the status is unknown/not-assessable with an explanation and no invented end-of-support date.

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
