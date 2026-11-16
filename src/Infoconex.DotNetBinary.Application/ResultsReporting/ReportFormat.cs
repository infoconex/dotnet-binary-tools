namespace Infoconex.DotNetBinary.Application.ResultsReporting;

/// <summary>The requested representation of one CAP-005 inventory.</summary>
public enum ReportFormat
{
    /// <summary>Concise or detailed text for people.</summary>
    Human,
    /// <summary>The stable v1 JSON contract for automation.</summary>
    Json,
}
