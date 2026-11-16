using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Application.ResultsReporting;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>Substitutable application boundaries used by the command adapter.</summary>
public sealed class CliUseCases(IInputSelectionUseCase selection, IInventoryUseCase inventory,
    IReportUseCase reporting, Func<LifecycleRefreshResult> refreshLifecycle)
{
    /// <summary>CAP-001 input selection, before inspection or refresh.</summary>
    public IInputSelectionUseCase Selection { get; } = selection ?? throw new ArgumentNullException(nameof(selection));

    /// <summary>CAP-005 inventory production.</summary>
    public IInventoryUseCase Inventory { get; } = inventory ?? throw new ArgumentNullException(nameof(inventory));

    /// <summary>CAP-006 reporting and safe persistence.</summary>
    public IReportUseCase Reporting { get; } = reporting ?? throw new ArgumentNullException(nameof(reporting));

    /// <summary>Explicit CAP-004 refresh, never called during parsing or help.</summary>
    public Func<LifecycleRefreshResult> RefreshLifecycle { get; } =
        refreshLifecycle ?? throw new ArgumentNullException(nameof(refreshLifecycle));
}
