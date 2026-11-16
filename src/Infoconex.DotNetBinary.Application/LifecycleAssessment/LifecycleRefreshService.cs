using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Coordinates intentionally requested network refresh with validation before replacement.</summary>
public sealed class LifecycleRefreshService(
    ILifecycleDatasetStore store, ILifecycleReleaseSource releaseSource,
    ILifecycleReleaseIndexReader indexReader, TimeProvider clock)
{
    private readonly ILifecycleDatasetStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly ILifecycleReleaseSource _source = releaseSource ?? throw new ArgumentNullException(nameof(releaseSource));
    private readonly ILifecycleReleaseIndexReader _indexReader = indexReader ?? throw new ArgumentNullException(nameof(indexReader));
    private readonly TimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>Downloads and validates data only on explicit invocation.</summary>
    public async Task<LifecycleRefreshResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var previous = _store.Load().Dataset;
        try
        {
            var contents = await _source.FetchAsync(cancellationToken).ConfigureAwait(false);
            var date = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
            var replacement = _indexReader.Read(contents, previous, date);
            _store.Replace(replacement);
            return new LifecycleRefreshResult(true, replacement, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or InvalidDataException or
            UnauthorizedAccessException or System.Text.Json.JsonException or OperationCanceledException)
        {
            return new LifecycleRefreshResult(false, _store.Load().Dataset,
                "Lifecycle refresh failed; retained the last valid local dataset (or bundled fallback). No partial update was accepted.");
        }
    }
}
