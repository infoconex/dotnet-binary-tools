namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Count of lifecycle findings for one evidence kind and advisory status.</summary>
/// <param name="EvidenceKind">Source kind, not a merged candidate verdict.</param>
/// <param name="Status">Published advisory status.</param>
/// <param name="Count">Number of individual findings.</param>
public sealed record InventoryLifecycleCount(LifecycleEvidenceKind EvidenceKind, LifecycleStatus Status, int Count);
