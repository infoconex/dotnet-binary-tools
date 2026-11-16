namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Independent CAP-002, CAP-003 and CAP-004 findings for one candidate.</summary>
/// <param name="Assembly">Assembly inspection result; never inferred from neighbors.</param>
/// <param name="Configuration">Directly associated deployment configuration evidence, if attempted.</param>
/// <param name="Lifecycle">Dated advisory findings for the distinct declared evidence.</param>
public sealed record InventoryEvidence(AssemblyInspectionResult Assembly,
    DeploymentConfigurationResult? Configuration, LifecycleAssessmentResult? Lifecycle);
