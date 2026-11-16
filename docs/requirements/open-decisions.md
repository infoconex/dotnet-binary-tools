# V0.1 design decisions to finalize before implementation

**Status:** Open technical decisions. The functional behaviors linked from the [requirements index](functional.md) and defined in the [capability specifications](../capabilities/README.md) are approved, but these numerical and contract details are **not** silently invented by the specification.

1. **Inventory resource limits — resolved for CAP-005:** [Implementation policy, values, ranges, safeguards and limitations](../engineering/inspection-inventory.md) record the technical choices made in [Issue #20](https://github.com/infoconex/dotnet-binary-tools/issues/20). They are not retrospective numerical product requirements; cooperative time/CPU bounds do not create a hard parser sandbox.
2. **Lifecycle reference freshness — resolved for CAP-004:** [Implementation policy and rationale](../engineering/lifecycle-assessment.md) defines the 90-day stale-data warning, exact Microsoft HTTPS allowlist, bounded/schema-validated refresh and same-directory atomic user-cache replacement. These are explicitly documented implementation decisions from [Issue #18](https://github.com/infoconex/dotnet-binary-tools/issues/18), not retrospective approved numerical product requirements. Routine execution remains offline.
3. **CLI spelling and JSON contract:** Finalize canonical argument aliases, report JSON property names and schema version; verify both through compatibility/contract tests.
4. **Input race handling:** Treat a selected file that disappears after initial validation as a safe read/inspection error; preserve the distinction between an invalid command *at validation* and an error discovered while executing a valid request.

**Already specified:** FR-CFG-002 limits an explicit `--config` to an individual `inspect` request. A multi-candidate `scan` cannot apply one shared explicit config. This is an approved requirement, not an open design decision.

These decisions must be resolved in implementation issues and acceptance contracts; they do **not** authorize unreviewed expanded product scope or feature claims.
