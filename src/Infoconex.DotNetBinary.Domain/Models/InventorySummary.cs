namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Non-overlapping candidate classifications and separately counted advisories.</summary>
public sealed class InventorySummary
{
    /// <summary>Aggregates only the supplied inventory events without inferring applications.</summary>
    public InventorySummary(IReadOnlyList<InventoryCandidate> candidates,
        IReadOnlyList<InventorySkippedPath> skipped, IReadOnlyList<InventoryTraversalError> errors)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(skipped);
        ArgumentNullException.ThrowIfNull(errors);
        TotalCandidates = candidates.Count;
        Managed = Count(AssemblyInspectionOutcome.ManagedAssembly);
        Native = Count(AssemblyInspectionOutcome.NativeBinary);
        Corrupt = Count(AssemblyInspectionOutcome.CorruptOrUnrecognized);
        Unreadable = Count(AssemblyInspectionOutcome.Unreadable);
        ResourceLimited = Count(AssemblyInspectionOutcome.ResourceLimited);
        Failed = Corrupt + Unreadable + ResourceLimited;
        ConfigurationFailures = candidates.Count(candidate => candidate.Configuration?.Outcome is
            DeploymentConfigurationOutcome.Malformed or DeploymentConfigurationOutcome.Unsupported or
            DeploymentConfigurationOutcome.Unreadable or DeploymentConfigurationOutcome.ResourceLimited);
        UnknownTargets = candidates.Count(candidate => candidate.Assembly.Outcome == AssemblyInspectionOutcome.ManagedAssembly &&
            (candidate.Assembly.Metadata?.DeclaredTarget is null ||
             candidate.Assembly.Metadata.DeclaredTarget.Family == TargetFrameworkFamily.Unrecognized));
        Skipped = skipped.Count;
        Duplicates = skipped.Count(item => item.Reason is InventorySkipReason.DuplicateCandidate or InventorySkipReason.DuplicateDirectory);
        TraversalErrors = errors.Count;
        Lifecycle = candidates.SelectMany(candidate => candidate.Lifecycle?.Findings ?? [])
            .GroupBy(finding => (finding.EvidenceKind, finding.Status))
            .Select(group => new InventoryLifecycleCount(group.Key.EvidenceKind, group.Key.Status, group.Count()))
            .OrderBy(count => count.EvidenceKind).ThenBy(count => count.Status).ToArray();

        int Count(AssemblyInspectionOutcome outcome) => candidates.Count(candidate => candidate.Assembly.Outcome == outcome);
    }

    /// <summary>Total unique candidates represented by entries.</summary>
    public int TotalCandidates { get; }
    /// <summary>Managed candidates with trusted assembly metadata.</summary>
    public int Managed { get; }
    /// <summary>Native PE candidates.</summary>
    public int Native { get; }
    /// <summary>Corrupt or unrecognized file content.</summary>
    public int Corrupt { get; }
    /// <summary>Unreadable candidates.</summary>
    public int Unreadable { get; }
    /// <summary>Candidates limited by parsing or file-size policy.</summary>
    public int ResourceLimited { get; }
    /// <summary>Total candidate failures across three mutually exclusive failure classifications.</summary>
    public int Failed { get; }
    /// <summary>Configuration observations that could not be interpreted, independently of assembly outcomes.</summary>
    public int ConfigurationFailures { get; }
    /// <summary>Managed assemblies without a recognized explicitly declared target.</summary>
    public int UnknownTargets { get; }
    /// <summary>Recorded skipped path events, including intentional deduplication.</summary>
    public int Skipped { get; }
    /// <summary>Already visited candidate or directory identities.</summary>
    public int Duplicates { get; }
    /// <summary>Recorded filesystem traversal errors.</summary>
    public int TraversalErrors { get; }
    /// <summary>Advisory distributions grouped by evidence kind, never conflated.</summary>
    public IReadOnlyList<InventoryLifecycleCount> Lifecycle { get; }
}
