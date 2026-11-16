using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class FakeInventoryInspector : IInventoryCandidateInspector
{
    public List<string> Visited { get; } = [];

    public Dictionary<string, AssemblyInspectionOutcome> Outcomes { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, InventoryEvidence> Evidence { get; } = new(StringComparer.Ordinal);

    public HashSet<string> ThrowPaths { get; } = new(StringComparer.Ordinal);

    public InventoryEvidence Inspect(SelectedInput file, SelectedInput? explicitConfiguration, long maxFileBytes,
        bool computeSha256 = false)
    {
        Visited.Add(file.FullPath);
        if (ThrowPaths.Contains(file.FullPath)) throw new IOException("synthetic sensitive error");
        if (Evidence.TryGetValue(file.FullPath, out var observed)) return observed;
        var outcome = Outcomes.GetValueOrDefault(file.FullPath, AssemblyInspectionOutcome.ManagedAssembly);
        return new InventoryEvidence(
            new AssemblyInspectionResult(outcome, null, null, null, []),
            new DeploymentConfigurationResult(DeploymentConfigurationOutcome.NotFound, null, null, [], [], []),
            new LifecycleAssessmentResult([], null));
    }
}
