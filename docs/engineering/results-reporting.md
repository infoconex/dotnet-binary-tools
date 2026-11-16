# CAP-006: Results and Reporting — JSON contract v1

**Public contract version:** `schemaVersion = "1.0.0"`. The approved [capability](../capabilities/CAP-006.md) and [acceptance scenarios](../scenarios/functional/results-and-reporting/README.md) remain authoritative for behavior; this document records the previously open technical selections, property names, nullability and compatibility policy.

## Evidence-based selections

- Microsoft's [System.Text.Json Native AOT guidance](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation) recommends source-generated metadata instead of reflection for typed serializers. The fixed v1 contract instead uses built-in [`Utf8JsonWriter`](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter) and explicit field writes, so no reflection or source-generation metadata is required.
- JSON bytes are streamed to a `TextWriter` with a fixed 4096-character conversion buffer. Each candidate is flushed as it is written; no second whole-report JSON object or string is retained. CAP-005 still owns one bounded in-memory inventory; this is not a hard process-memory guarantee for adversarial individual fields.
- Microsoft's [`FileMode.CreateNew`](https://learn.microsoft.com/en-us/dotnet/api/system.io.filemode) makes the OS reject a file if it already exists. On confirmed collision, use `<stem>_yyyyMMddTHHmmssfffZ_<128-bit-random-hex><extension>` in UTC, attempting up to sixteen exclusive retries. Never truncate, replace or use existence as permission to overwrite. Other IO errors propagate. A failed output may leave an incomplete **new** file and must not be claimed as a completed report.
- CAP-006 CLI bindings: `--format human|json` (human default), `--details` (human only), `--fields <comma-separated names>`, `--output <file>`. Invalid or duplicate fields, unsupported formats, and `--details` with `--fields` fail before writing. Canonical aliases, complete help and remaining CLI semantics belong to CAP-007.
- Reports go to stdout unless `--output` is specified, in which case stdout has no report content. Coverage warnings, configuration diagnostic codes and stale-lifecycle notices go to stderr. Incomplete inventory coverage is not itself a failed command. Reporting errors are nonzero with no raw exception text in stderr.

## Versioned JSON envelope

All ten top-level properties are mandatory and emitted in the documented order for both inspect and scan. `[]` means JSON array, `{}` JSON object, and integers are non-fractional numbers.

| Property | Type | Required meaning |
| --- | --- | --- |
| `schemaVersion` | string | Exact `1.0.0` |
| `command` | string | `inspect` or `scan` |
| `inputs` | object[] | Original and fully qualified explicit inputs |
| `generatedAtUtc` | string | ISO-8601 UTC timestamp |
| `completion` | object | Complete/incomplete and distinct coverage reasons |
| `summary` | object | Unmodified CAP-005 totals and distributions |
| `lifecycleDatasets` | object[] | Distinct published dataset provenance, empty if no finding |
| `candidates` | object[] | Each candidate and independent findings |
| `skipped` | object[] | Skipped/duplicate path events |
| `errors` | object[] | Traversal errors and safe codes |

### Nested property contracts

| Location | Mandatory property names and types |
| --- | --- |
| `inputs[]` | `requestedPath` string, `fullPath` string, `kind` string enum |
| `completion` | `state` string enum (`Complete`/`Incomplete`), `reasons` string[] of named limitation codes |
| `summary` | `totalCandidates`, `managed`, `native`, `corrupt`, `unreadable`, `resourceLimited`, `failed`, `configurationFailures`, `unknownTargets`, `skipped`, `duplicates`, `traversalErrors`: integer; `lifecycle`: object[] |
| `summary.lifecycle[]` | `evidenceKind` string enum, `status` string enum, `count` integer |
| `lifecycleDatasets[]` | `version` string, `sourceUrl` string/null, `sourcePublishedOn` ISO date string/null, `sourceRetrievedOn` ISO date string |
| `candidates[]` | `path` string, `outcome` string enum, `fileFacts` object/null, `assembly` object, `sha256` string/null, `diagnostics` object[], `configuration` object/null, `lifecycle` object/null |
| `fileFacts` | `length` integer (bytes), `lastModifiedUtc` ISO-8601 UTC string |
| `diagnostics[]` | `code` string, `field` string; no raw exception messages |
| `configuration` | `outcome` string enum, `path` string/null, `association` string enum/null, `values` object[], `runtimeRequests` object[], `diagnostics` object[] |
| `configuration.values[]` | `field` string (source field), `rawValue` string (declared value) |
| `configuration.runtimeRequests[]` | `origin` string, `values` object[] |
| `lifecycle` | `datasetWarning` string/null, `findings` object[] |
| `lifecycle.findings[]` | `evidenceKind`, `declaredIdentity`, `evidenceSource`, `family`, `status`, `datasetVersion`, `sourceIdentity`, `limitation`: string; `endOfSupport`, `sourcePublishedOn`: date string/null; `assessmentDate`, `sourceRetrievedOn`: date string; `sourceUrl`: string/null; `isStale`: boolean |
| `skipped[]` | `root` string, `path` string, `reason` string enum |
| `errors[]` | `root` string, `path` string, `code` string |

### Per-candidate assembly field contract

`assembly` is **always an object**, including native/unreadable results; unknown fields are JSON null, not invented values. With no projection, the following thirteen case-sensitive fields appear in this order. With `--fields`, only the requested subset appears under `assembly`; **all other candidate evidence and top-level envelope fields remain mandatory**.

| Field | JSON type | Meaning |
| --- | --- | --- |
| `name` | string/null | Declared manifest name |
| `version` | string/null | Declared assembly version |
| `targetFramework` | string/null | Original TargetFrameworkAttribute moniker, not an installed runtime |
| `fileVersion` | string/null | FileVersionAttribute |
| `informationalVersion` | string/null | InformationalVersionAttribute |
| `company` | string/null | CompanyAttribute |
| `product` | string/null | ProductAttribute |
| `description` | string/null | DescriptionAttribute |
| `references` | object[]/null | Declared references; each has `name` string, `version` string, `culture` string/null, `publicKeyToken` string/null |
| `clrMetadataVersion` | string/null | Raw CLR metadata version; not a target |
| `peMachine` | string/null | PE machine declaration |
| `corFlags` | string/null | CLI flags declaration |
| `hasEntryPoint` | boolean/null | Verified CLI entry-point presence |

All string enums use the CAP-005 .NET enum member names (case-sensitive). ISO dates use `yyyy-MM-dd`. Empty arrays mean no observed entries; null means unavailable or unattempted evidence. A found config with `outcome: "NotFound"` is distinct from a null unattempted configuration. Raw paths and source strings are JSON-escaped; human output replaces control characters and bounds displayed strings. `--details` expands human assembly fields, lifecycle provenance and diagnostics; it cannot be combined with `--fields` and cannot affect JSON.

## Compatibility policy

- **Major** (`2.0.0`): remove/rename a property, change meaning/type/nullability or enum spelling. Consumers require migration guidance.
- **Minor** (`1.1.0`): add optional properties without breaking existing fields; document additions and contract-test forward compatibility.
- **Patch** (`1.0.1`): correct output without changing published JSON shapes/semantics.

The contract specifies names and types, not byte-for-byte stable timestamps or candidate discovery order. Schema modifications require separately reviewed requirements impact and tests. No extra schema-generator dependency is justified for this first version.
