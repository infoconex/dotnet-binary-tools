# ADR 0001: Original Core/CLI foundation design

**Status: Superseded by [ADR 0003](0003-hexagonal-project-boundaries.md).** Retained for architectural history.

## Original decision

The first engineering scaffold separated a reusable Core assembly from a CLI command assembly. The Core project held raw target-framework identity and was intended to handle metadata parsing. CLI held command handling and was intended to handle scanning and formatting.

## Why superseded

The approved v0.1 architecture explicitly requires SOLID, hexagonal Ports-and-Adapters, separate compiled Domain, Application, Infrastructure and CLI layers, and inward-only references. The original design does not meet that requirement and is not the architectural target.

See [mandatory ARCH constraints](../../requirements/architecture.md).
