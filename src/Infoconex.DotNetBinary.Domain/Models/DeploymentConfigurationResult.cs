namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Independent, provenance-preserving deployment configuration evidence.</summary>
/// <param name="Outcome">Configuration classification, not a runtime compatibility conclusion.</param>
/// <param name="Path">Full source path when a candidate was considered; null if none was found.</param>
/// <param name="Association">How the source was associated, or null on configuration absence.</param>
/// <param name="RuntimeRequests">Each declared runtime request without implied defaults.</param>
/// <param name="Values">Other supported top-level configuration declarations and policies.</param>
/// <param name="Diagnostics">Safe diagnostics with no file-content or exception-message disclosure.</param>
public sealed record DeploymentConfigurationResult(
    DeploymentConfigurationOutcome Outcome,
    string? Path,
    DeploymentConfigurationAssociation? Association,
    IReadOnlyList<DeploymentRuntimeRequest> RuntimeRequests,
    IReadOnlyList<DeploymentConfigurationValue> Values,
    IReadOnlyList<InspectionDiagnostic> Diagnostics);
