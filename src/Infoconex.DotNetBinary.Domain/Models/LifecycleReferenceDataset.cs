namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Validated local lifecycle snapshot, immutable to callers.</summary>
/// <param name="Version">Snapshot version, derived from identified official publication.</param>
/// <param name="RetrievedOn">When the Microsoft source was actually retrieved, in UTC.</param>
/// <param name="Entries">Versioned entries including source-specific applicability information.</param>
public sealed record LifecycleReferenceDataset(string Version, DateOnly RetrievedOn,
    IReadOnlyList<LifecycleReferenceEntry> Entries);
