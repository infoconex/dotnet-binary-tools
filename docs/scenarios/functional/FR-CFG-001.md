# FR-CFG-001: Automatically discover only directly associated supported configuration filenames without searching neighboring application directories or guessing host relationships.

**Capability:** [CAP-003 — Deployment Configuration](../../capabilities.md)  
**Requirement status:** Approved for v0.1 specification  
**Acceptance evidence:** Planned — not implemented or verified

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

## Verification

Use controlled, synthetic fixtures, including platform/edge variations where appropriate. Tests remain **Planned** until executed; do not claim feature acceptance from a written scenario. Record observed test names, CI runs and exact commit SHA in [traceability](../../testing/traceability.md).
