namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Configurable, bounded CAP-005 inventory policy, independent of eventual CAP-007 parsing.</summary>
/// <param name="Recursive">Explicitly descend into discovered subdirectories.</param>
/// <param name="FollowLinks">Follow discovered linked directories, with physical cycle control.</param>
/// <param name="MaxDepth">Maximum depth below each selected root (root is zero).</param>
/// <param name="MaxCandidates">Maximum number of unique file candidates to inspect.</param>
/// <param name="MaxFileBytes">Maximum size of one candidate, in bytes.</param>
/// <param name="MaxElapsedSeconds">Cooperative wall time budget, in seconds.</param>
/// <param name="MaxCpuSeconds">Cooperative process CPU time budget, in seconds.</param>
public sealed record InventoryOptions(bool Recursive = false, bool FollowLinks = false,
    int MaxDepth = 32, int MaxCandidates = 10_000, long MaxFileBytes = 512L * 1024 * 1024,
    int MaxElapsedSeconds = 300, int MaxCpuSeconds = 300)
{
    /// <summary>Absolute bound on unique candidates.</summary>
    public const int MaximumCandidates = 100_000;

    /// <summary>Validates the documented CAP-005 implementation range before any work.</summary>
    public void Validate()
    {
        if (MaxDepth is < 0 or > 128) throw new ArgumentOutOfRangeException(nameof(MaxDepth));
        if (MaxCandidates is < 1 or > MaximumCandidates) throw new ArgumentOutOfRangeException(nameof(MaxCandidates));
        if (MaxFileBytes < 1024L * 1024 || MaxFileBytes > 1024L * 1024 * 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxFileBytes));
        }

        if (MaxElapsedSeconds is < 1 or > 3600) throw new ArgumentOutOfRangeException(nameof(MaxElapsedSeconds));
        if (MaxCpuSeconds is < 1 or > 3600) throw new ArgumentOutOfRangeException(nameof(MaxCpuSeconds));
    }
}
