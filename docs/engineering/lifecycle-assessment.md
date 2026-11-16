# CAP-004: lifecycle reference sources and update policy

[CAP-004 approved scope](../capabilities/CAP-004.md) · [Scenario evidence](../testing/traceability.md) · [Implementation issue #18](https://github.com/infoconex/dotnet-binary-tools/issues/18)

## Source authority and snapshot

The immutable bundled `lifecycle-reference.json` is version **2026-11-10**, retrieved **2026-11-16 UTC**. It was checked against these authoritative Microsoft sources:

| Evidence | Microsoft source | What the evidence does *not* establish |
| --- | --- | --- |
| .NET / .NET Core channels, including .NET 11 and 10 | [.NET Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy), [published releases index](https://github.com/dotnet/core/blob/main/release-notes/releases-index.json), [metadata README/schema](https://github.com/dotnet/core/blob/main/release-notes/README.md) | Which patch is installed, runtime execution, OS lifecycle/availability, support eligibility on a specific machine |
| .NET Framework version exceptions | [Microsoft Framework product lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework), [Framework FAQ](https://learn.microsoft.com/en-us/lifecycle/faq/dotnet-framework) | Whether a particular Windows version is supported or the Framework service pack installed; Framework 3.5 and 3.5 SP1 are not equivalent |
| .NET Standard | [Microsoft .NET Standard overview](https://learn.microsoft.com/en-us/dotnet/standard/net-standard) | Any standalone runtime lifecycle: it is an API specification |

The JSON snapshot stores source URLs, source-published/latest-release dates where evidenced, retrieval date, dataset version, applicable version/family and limitations for individual entries. The official releases index's `latest-release-date` is used as the recorded channel publication/update proxy; it is **not** an assertion of the document's first publication date. The source policy remains authoritative if its later corrections differ from a historical snapshot.

## Implementation choices, evaluated November 15, 2026 (Pacific)

These are CAP-004 technical selections from Issue #18, **not** newly approved product requirements:

| Choice | Selected policy | Tradeoff / rejected alternative |
| --- | --- | --- |
| Freshness | Warn when retrieval age is more than **90 days**, or the reference retrieval date is after the assessment date | 30 days creates frequent offline warnings; 180 days may conceal several servicing cycles; no refresh is triggered by staleness |
| Online source | Exact allowlisted `https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json`; HTTPS, redirect rejection, no custom URLs | Arbitrary URLs expand source-trust boundary; scraping support HTML is brittle; the Microsoft index is machine-readable and documented |
| Integrity | 256 KiB maximum source, 128 KiB maximum cache, 15-second HTTP timeout, JSON max depth 16, unique properties and channels, dates, family/product/phase, supported schema, approved source URLs and required known coverage | Transport TLS plus strict validation, **not cryptographic source signing**; Microsoft does not publish a signature for this index used by the tool |
| Storage | Embedded immutable fallback, explicit per-user local-app-data cache, stage bytes in same directory, flush and atomic rename; invalid cache uses bundle with a warning | In-place overwrite risks half-written data; current operating-system filesystem guarantees and per-user cache permissions still apply |

The executable only constructs an HTTP client when `lifecycle refresh` is requested. Ordinary `inspect` and lifecycle assessment use the embedded resource or validated local cache. The refresh modifies only .NET / .NET Core release-index facts; separately sourced Windows/.NET Framework component records remain intact. A failed download, rejected document or atomic move leaves previously valid data untouched and reports the version still used. Cross-process concurrent refreshes are not serialized; atomic rename prevents partial-reader data, but last-writer-wins applies to successful concurrent refreshes. Unknown/future or absent entries remain unknown.

## Advisory model and limits

An assembly target (`TargetFrameworkAttribute`) and a configuration runtime request (`runtimeconfig.json` framework entry or .NET Framework `sku`) are **distinct facts**. Findings retain their evidence origins, assessment date, known source date, version, potential support-end date, per-source limitation and freshness indication. `runtimeOptions.tfm` alone is not treated as a discovered runtime installation; a classic CLR startup `version` without a .NET Framework `sku` is not interpreted as a Framework product version. The application does not observe installed or executing runtimes or determine compatibility, compliance or security vulnerability exposure. Unsupported status does not change command exit behavior.

The CAP-005 CLI now returns success for a valid `inspect` inventory, even if coverage is incomplete; this is **not** a lifecycle enforcement exit. CAP-006 reporting and the full CAP-007 command interface remain incomplete. An explicit refresh command can exit nonzero on its own failed operation. The persistent dataset does not cryptographically detect tampering by a process running as the same user and able to rewrite that user's cache, so cache integrity remains limited to schema/allowlist checks and operating-system access control. Release candidate acceptance still requires the independently tracked Windows/macOS runtime-free Native AOT consumer checks in [Issue #4](https://github.com/infoconex/dotnet-binary-tools/issues/4).
