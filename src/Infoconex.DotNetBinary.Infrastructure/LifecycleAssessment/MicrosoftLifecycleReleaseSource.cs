using System.Net;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;

namespace Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

/// <summary>Explicit HTTPS fetcher pinned to Microsoft's authoritative release metadata endpoint.</summary>
public sealed class MicrosoftLifecycleReleaseSource(HttpClient client) : ILifecycleReleaseSource
{
    /// <summary>The sole allowlisted refresh endpoint; not user-configurable.</summary>
    public const string Endpoint = "https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json";
    private const int MaxDownloadBytes = 256 * 1024;
    private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>Creates a client with platform TLS, no redirect and a bounded connection timeout.</summary>
    public static HttpClient CreateHttpClient() => new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
    })
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    /// <inheritdoc />
    public async Task<byte[]> FetchAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _client.GetAsync(Endpoint, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.OK || response.Content.Headers.ContentLength > MaxDownloadBytes)
        {
            throw new InvalidDataException("Official release source returned an unacceptable response.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        while (true)
        {
            var count = await stream.ReadAsync(chunk, cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                break;
            }

            if (buffer.Length + count > MaxDownloadBytes)
            {
                throw new InvalidDataException("Official release index exceeds the allowed byte budget.");
            }

            buffer.Write(chunk, 0, count);
        }

        return buffer.ToArray();
    }
}
