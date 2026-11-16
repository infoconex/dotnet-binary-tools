using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Honest explicit refresh outcome, including retained data after failure.</summary>
/// <param name="Succeeded">True only after committed replacement.</param>
/// <param name="Dataset">Data actually left available after the operation.</param>
/// <param name="Warning">Safe, non-sensitive failure information.</param>
public sealed record LifecycleRefreshResult(bool Succeeded, LifecycleReferenceDataset Dataset, string? Warning);
