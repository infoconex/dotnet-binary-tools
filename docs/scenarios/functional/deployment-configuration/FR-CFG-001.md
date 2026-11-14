# FR-CFG-001: Automatically discover only directly associated supported configuration filenames without searching neighboring application directories or guessing host relationships.

**Capability:** [CAP-003 — Deployment Configuration](../../../capabilities/CAP-003.md) · [Verification evidence](../../../testing/traceability.md)

## Requirement

Automatically discover only directly associated supported configuration filenames without searching neighboring application directories or guessing host relationships.

## Acceptance scenarios

### SC-CFG-001: Discover a matching runtimeconfig

**Given** MyApp.dll and MyApp.runtimeconfig.json share an exact basename in one directory

**When** CAP-003 inspects associated configuration

**Then** the matching runtimeconfig is considered and unrelated runtimeconfig files are not.

### SC-CFG-002: Do not associate another application

**Given** MyApp.exe and OtherApp.runtimeconfig.json are present together

**When** configuration discovery runs

**Then** the unrelated config is not attributed to MyApp.
