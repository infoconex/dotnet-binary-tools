using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.ResultsReporting;

/// <summary>Selects the corresponding presentation adapter; neither adapter edits inventory facts.</summary>
public sealed class InventoryReportRenderer : IInventoryReportRenderer
{
    private readonly JsonInventoryReportRenderer _json = new();
    private readonly HumanInventoryReportRenderer _human = new();

    /// <inheritdoc />
    public void Render(InventoryReport inventory, string command, DateTimeOffset generatedUtc,
        ReportOptions options, TextWriter destination)
    {
        switch (options.Format)
        {
            case ReportFormat.Json:
                _json.Render(inventory, command, generatedUtc, options, destination);
                break;
            case ReportFormat.Human:
                _human.Render(inventory, command, generatedUtc, options, destination);
                break;
            default:
                throw new ArgumentException("Unsupported report format.", nameof(options));
        }
    }
}
