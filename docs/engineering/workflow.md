# Development and issue workflow

## Ordinary changes

1. Start from an approved CAP ID, then define one independently testable FR/NFR or engineering issue and its acceptance scenarios.
2. Branch from current `main` as `issue/<number>-<slug>`.
3. Write a failing test for missing/defective functionality, then implement, refactor, and run all affected tests.
4. Update test traceability with **observed** execution evidence and exact SHA.
5. Open a PR to `main`, document risks and limitations, and confirm required CI runs.
6. Owner performs second-pass review, squash merges only after verification, and checks resulting `main` CI.

Create a release branch only when concurrent changes or stabilization requirements justify it.

## Evidence-based technical decisions

For changes involving established .NET, NuGet, OS, library, or tooling behavior,
check available guidance **before** designing a solution or experiment:

1. Consult authoritative platform/vendor documentation and applicable published
   specifications, checking the SDK, package version, OS and architecture to
   which the guidance applies.
2. When documentation leaves a material question unanswered, inspect relevant
   upstream source or tests, then use a narrowly scoped, reproducible experiment
   to resolve the remaining uncertainty. Community advice is supporting context,
   not a substitute for an applicable documented contract.
3. Implement the documented approach when applicable, then verify it with this
   repository's actual dependencies, tests, platforms and release requirements.
4. Capture important findings in the issue and PR without duplicating evolving
   status in permanent engineering documentation. Explain deviations and their
   tradeoffs; an authoritative recommendation is a starting point for judgment,
   not an automatic mandate.

Classify evidence accurately:

| Classification | What it establishes |
| --- | --- |
| **Documented** | An authoritative source describes the behavior; cite the source and its applicable version or scope. |
| **Verified** | An observed test or CI run demonstrates the behavior in this repository; link commands, results and exact SHA. |
| **Experimental** | A controlled investigation addresses a question not settled by the documentation; record the question, method, outcome and limits. |

Before starting an experiment, ask: **What uncertainty remains after reviewing
authoritative guidance?** If none remains, implement and validate without an
additional discovery experiment. Use this practice proportionately; routine
changes and well-understood application logic need no research write-up.

The [multi-RID NuGet lock-file implementation](build-configuration.md#committed-nuget-lock-files)
is a concrete example: Microsoft's restore and lock-file behavior is documented,
while cross-platform Native AOT/tool installation and Apple Silicon restore
graphs needed repository-specific verification.

## Review ownership

A solo maintainer can configure zero mandatory reviewer approvals while still requiring a PR, passing checks and a documented second-pass owner review. Do not label self-review as independent review.

Promotion to `main` is not authorization to tag or publish. Separate explicit GO is needed for public GitHub Releases, NuGet pushes, and standalone downloads. See [release gates](release-gates.md).
