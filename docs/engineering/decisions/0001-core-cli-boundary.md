# ADR 0001: Separate metadata inspection Core from CLI

**Status:** Proposed for review on the engineering-foundation branch.

## Context

The tool reads compiled managed assemblies without source code and must not execute them. A reusable metadata reader should be tested independently of user-facing command routing and output formatting.

## Decision

Use two production SDK-style projects: `Infoconex.DotNetFramework.Core` and `Infoconex.DotNetFramework.Cli`. CLI depends on Core, never the reverse. Core owns parsing and results; CLI owns paths, arguments, output and packaging. Use corresponding independent NUnit test projects.

Prefer standard Microsoft metadata libraries. Do not create RecordSift's extensive domain/application/adapter assembly graph without an actual need.

## Consequences

The initial solution has four projects and one straightforward dependency. Core API compatibility and CLI output formats will need separate tests as features are introduced. The setup branch contains only a minimal raw-moniker model and help/version CLI, not a binary inspector.
