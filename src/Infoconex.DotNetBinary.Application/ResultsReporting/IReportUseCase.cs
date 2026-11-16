using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.ResultsReporting;

/// <summary>Application boundary for presenting and safely persisting one inventory report.</summary>
public interface IReportUseCase
{
    /// <summary>Writes the requested report or propagates a failure without claiming success.</summary>
    string? Write(InventoryReport inventory, string command, ReportOptions options,
        TextWriter output, TextWriter diagnostics);
}
