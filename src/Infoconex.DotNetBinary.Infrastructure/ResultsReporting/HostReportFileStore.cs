using System.Text;
using Infoconex.DotNetBinary.Application.ResultsReporting;

namespace Infoconex.DotNetBinary.Infrastructure.ResultsReporting;

/// <summary>Host-file adapter using the OS's atomic exclusive-create operation.</summary>
public sealed class HostReportFileStore : IReportFileStore
{
    /// <inheritdoc />
    public TextWriter CreateNew(string path)
    {
        var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        return new StreamWriter(file, new UTF8Encoding(false, true));
    }

    /// <inheritdoc />
    public bool Exists(string path) => File.Exists(path);
}
