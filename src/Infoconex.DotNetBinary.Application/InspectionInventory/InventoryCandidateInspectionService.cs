using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Reuses CAP-002/003/004 application services without duplicating their rules.</summary>
public sealed class InventoryCandidateInspectionService(
    AssemblyInspectionService assembly, DeploymentConfigurationService configuration,
    LifecycleAssessmentService lifecycle) : IInventoryCandidateInspector
{
    private readonly AssemblyInspectionService _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
    private readonly DeploymentConfigurationService _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly LifecycleAssessmentService _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));

    /// <inheritdoc />
    public InventoryEvidence Inspect(SelectedInput file, SelectedInput? explicitConfiguration, long maxFileBytes,
        bool computeSha256 = false)
    {
        var assembly = _assembly.Inspect(file,
            new AssemblyInspectionOptions(ComputeSha256: computeSha256, MaxFileBytes: maxFileBytes));
        var configuration = _configuration.Inspect(file, explicitConfiguration);
        var lifecycle = _lifecycle.Assess(assembly.Metadata?.DeclaredTarget, configuration);
        return new InventoryEvidence(assembly, configuration, lifecycle);
    }
}
