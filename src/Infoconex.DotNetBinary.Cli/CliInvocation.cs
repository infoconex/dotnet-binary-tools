using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Application.ResultsReporting;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>Validated command intent, without filesystem or network side effects.</summary>
internal sealed record CliInvocation(string Command, IReadOnlyList<string> Paths,
    InventoryOptions Inventory, ReportOptions Reporting, string? ConfigurationPath = null,
    bool RefreshLifecycle = false);
