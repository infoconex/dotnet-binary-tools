using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Usable verified dataset and whether a corrupt cache required fallback.</summary>
/// <param name="Dataset">Validated reference data actually used.</param>
/// <param name="UsedBundledFallback">True only when an unusable existing cache was bypassed.</param>
/// <param name="Warning">Non-sensitive data-quality warning when applicable.</param>
public sealed record LifecycleDatasetLoad(LifecycleReferenceDataset Dataset, bool UsedBundledFallback, string? Warning);
