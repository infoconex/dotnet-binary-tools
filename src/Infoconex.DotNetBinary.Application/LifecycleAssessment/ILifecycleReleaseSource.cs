namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Explicit-only port for obtaining bounded bytes from the approved Microsoft endpoint.</summary>
public interface ILifecycleReleaseSource
{
    /// <summary>Downloads authoritative release-index data. Call only during explicit refresh.</summary>
    Task<byte[]> FetchAsync(CancellationToken cancellationToken = default);
}
