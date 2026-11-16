using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.ResultsReporting;

/// <summary>Rendering port; implementations must stream without changing the inventory.</summary>
public interface IInventoryReportRenderer
{
    /// <summary>Writes one presentation to the supplied writer, propagating every write failure.</summary>
    void Render(InventoryReport inventory, string command, DateTimeOffset generatedUtc,
        ReportOptions options, TextWriter destination);
}
