namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Shared, structured and auditable result for one inspect or multi-root scan request.</summary>
public sealed class InventoryReport
{
    /// <summary>Snapshots roots and collected findings and derives honest completion semantics.</summary>
    public InventoryReport(IEnumerable<SelectedInput> roots, IEnumerable<InventoryCandidate> candidates,
        IEnumerable<InventorySkippedPath> skipped, IEnumerable<InventoryTraversalError> errors,
        IEnumerable<InventoryCompletionReason> reasons)
    {
        ArgumentNullException.ThrowIfNull(roots);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(skipped);
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentNullException.ThrowIfNull(reasons);
        Roots = Array.AsReadOnly(roots.ToArray());
        Candidates = Array.AsReadOnly(candidates.ToArray());
        Skipped = Array.AsReadOnly(skipped.ToArray());
        Errors = Array.AsReadOnly(errors.ToArray());
        Summary = new InventorySummary(Candidates, Skipped, Errors);
        var actualReasons = reasons.ToList();
        if (Summary.Failed != 0) actualReasons.Add(InventoryCompletionReason.CandidateFailure);
        if (Summary.ConfigurationFailures != 0) actualReasons.Add(InventoryCompletionReason.ConfigurationFailure);
        if (Candidates.Any(candidate => candidate.Assembly.Outcome == AssemblyInspectionOutcome.ResourceLimited &&
            !Skipped.Any(item => item.Path == candidate.Input.FullPath && item.Reason == InventorySkipReason.FileSizeLimit)))
        {
            actualReasons.Add(InventoryCompletionReason.ParsingLimit);
        }
        if (Errors.Count != 0) actualReasons.Add(InventoryCompletionReason.TraversalError);
        if (Skipped.Any(item => item.Reason == InventorySkipReason.LinkedDirectory))
        {
            actualReasons.Add(InventoryCompletionReason.LinkedDirectory);
        }

        Completion = new InventoryCompletion(actualReasons);
    }

    /// <summary>Exactly the validated selected input roots, even if overlapping.</summary>
    public IReadOnlyList<SelectedInput> Roots { get; }
    /// <summary>One entry per unique physical candidate.</summary>
    public IReadOnlyList<InventoryCandidate> Candidates { get; }
    /// <summary>Paths skipped due to limits, link policy or duplication.</summary>
    public IReadOnlyList<InventorySkippedPath> Skipped { get; }
    /// <summary>Safe path-specific enumeration failures.</summary>
    public IReadOnlyList<InventoryTraversalError> Errors { get; }
    /// <summary>Common aggregate counts and separate lifecycle distributions.</summary>
    public InventorySummary Summary { get; }
    /// <summary>Complete/incomplete coverage and its reasons.</summary>
    public InventoryCompletion Completion { get; }
}
