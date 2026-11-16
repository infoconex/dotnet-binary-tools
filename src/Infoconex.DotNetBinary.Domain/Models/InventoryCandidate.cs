namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>One physical candidate with independent inspection and advisory evidence.</summary>
/// <param name="Input">Individual selected file, retaining the observed logical path.</param>
/// <param name="Evidence">Independent binary, configuration and lifecycle observations.</param>
public sealed record InventoryCandidate(SelectedInput Input, InventoryEvidence Evidence)
{
    /// <summary>Gets the candidate's CAP-002 outcome.</summary>
    public AssemblyInspectionResult Assembly => Evidence.Assembly;

    /// <summary>Gets directly associated CAP-003 evidence, if inspected.</summary>
    public DeploymentConfigurationResult? Configuration => Evidence.Configuration;

    /// <summary>Gets independent CAP-004 lifecycle advisories, if available.</summary>
    public LifecycleAssessmentResult? Lifecycle => Evidence.Lifecycle;
}
