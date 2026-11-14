# V0.1 design decisions to finalize before implementation

**Status:** Open technical decisions. The functional behaviors linked from the [requirements index](functional.md) and defined in the [capability specifications](../capabilities/README.md) are approved, but these numerical and contract details are **not** silently invented by the specification.

1. **Inventory resource limits:** Select numerical default maximum depth, candidate count, and candidate/file size. Define command option names, override bounds and safeguards for CPU, metadata parsing and memory.
2. **Lifecycle reference freshness:** Select an observable stale-data threshold, source allowlist, refresh signature/integrity rules, and cache behavior. Default execution remains offline.
3. **CLI spelling and JSON contract:** Finalize canonical argument aliases, report JSON property names and schema version; verify both through compatibility/contract tests.
4. **Input race handling:** Treat a selected file that disappears after initial validation as a safe read/inspection error; preserve the distinction between an invalid command *at validation* and an error discovered while executing a valid request.

**Already specified:** FR-CFG-002 limits an explicit `--config` to an individual `inspect` request. A multi-candidate `scan` cannot apply one shared explicit config. This is an approved requirement, not an open design decision.

These decisions must be resolved in implementation issues and acceptance contracts; they do **not** authorize unreviewed expanded product scope or feature claims.
