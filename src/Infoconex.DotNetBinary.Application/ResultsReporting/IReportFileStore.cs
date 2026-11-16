namespace Infoconex.DotNetBinary.Application.ResultsReporting;

/// <summary>Output file port that exclusively creates new UTF-8 reports and never truncates existing files.</summary>
public interface IReportFileStore
{
    /// <summary>Opens a brand-new file or fails when the path exists or cannot be written.</summary>
    TextWriter CreateNew(string path);

    /// <summary>Identifies an existing path after an exclusive open failed, never authorizes overwriting it.</summary>
    bool Exists(string path);
}
