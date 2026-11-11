# V0.1 design decisions to finalize before implementation

**Status:** Open technical decisions. The functional behaviors in [the requirements catalog](functional.md) are approved, but these numerical and contract details are **not** silently invented by the specification.

1. **Inventory resource limits:** Select numerical default maximum depth, candidate count, and candidate/file size. Define command option names, override bounds and safeguards for CPU, metadata parsing and memory.
2. **Lifecycle reference freshness:** Select an observable stale-data threshold, source allowlist, refresh signature/integrity rules, and cache behavior. Default execution remains offline.
3. **CLI spelling and JSON contract:** Finalize canonical argument aliases, report JSON property names and schema version; verify both through compatibility/contract tests.
4. **Explicit configuration input:** Restrict a single explicit `--config` to `inspect` for v0.1. Multi-candidate scans may not apply one shared explicit configuration to unrelated artifacts without an independently reviewed mapping design.
5. **Input race handling:** Treat a selected file that disappears after initial validation as a safe read/inspection error; preserve the distinction between an invalid command *at validation* and an error discovered while executing a valid request.

These decisions must be resolved in implementation issues and acceptance contracts; they do **not** authorize unreviewed expanded product scope or feature claims.
