namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>One sourced major/minor release or version's support information.</summary>
/// <param name="Family">Product family this entry applies to.</param>
/// <param name="Version">Major/minor version, including Framework 4.8.1 where applicable.</param>
/// <param name="EndOfSupport">Published end date, absent if host-dependent or unavailable.</param>
/// <param name="HostDependent">True when Windows OS servicing/lifecycle evidence is additionally required.</param>
/// <param name="Source">Official source URL for this particular fact.</param>
/// <param name="PublishedOn">Source publication date when published/identifiable.</param>
/// <param name="Limitation">Source-specific applicability limitations.</param>
public sealed record LifecycleReferenceEntry(TargetFrameworkFamily Family, string Version,
    DateOnly? EndOfSupport, bool HostDependent, string Source, DateOnly? PublishedOn, string Limitation);
