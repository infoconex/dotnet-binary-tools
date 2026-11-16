using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Infrastructure.AssemblyInspection;
using Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;
using Infoconex.DotNetBinary.Infrastructure.InputSelection;
using Infoconex.DotNetBinary.Infrastructure.InspectionInventory;
using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;
using Infoconex.DotNetBinary.Infrastructure.ResultsReporting;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>Composes outer adapters only after successful CLI grammar validation.</summary>
internal static class CliCompositionRoot
{
    internal static int Execute(CliInvocation invocation, TextWriter output, TextWriter error)
    {
        using var cpuClock = new HostInventoryCpuClock();
        var store = new FileLifecycleDatasetStore();
        var useCases = new CliUseCases(
            new InputSelectionService(new HostFileSystemInputPathProbe()),
            new InventoryService(new HostInventoryFileSystem(),
                new InventoryCandidateInspectionService(
                    new AssemblyInspectionService(new PortableExecutableAssemblyReader()),
                    new DeploymentConfigurationService(new FileDeploymentConfigurationReader()),
                    new LifecycleAssessmentService(store, TimeProvider.System)),
                TimeProvider.System, cpuClock),
            new ReportService(new InventoryReportRenderer(), new HostReportFileStore(),
                TimeProvider.System, () => Guid.NewGuid().ToString("N")),
            () => Refresh(store));
        return CliApplication.Execute(invocation, output, error, useCases);
    }

    private static LifecycleRefreshResult Refresh(FileLifecycleDatasetStore store)
    {
        using var client = MicrosoftLifecycleReleaseSource.CreateHttpClient();
        return new LifecycleRefreshService(store, new MicrosoftLifecycleReleaseSource(client),
            new MicrosoftReleaseIndexReader(), TimeProvider.System).RefreshAsync().GetAwaiter().GetResult();
    }
}
