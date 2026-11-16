using System.Text;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class FakeReleaseSource(string json) : ILifecycleReleaseSource
{
    public bool FailDownload { get; set; }

    public int Attempts { get; private set; }

    public Task<byte[]> FetchAsync(CancellationToken cancellationToken = default)
    {
        Attempts++;
        return FailDownload ? Task.FromException<byte[]>(new HttpRequestException("synthetic error")) :
            Task.FromResult(Encoding.UTF8.GetBytes(json));
    }
}
