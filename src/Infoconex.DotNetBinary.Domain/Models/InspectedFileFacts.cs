namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Basic file facts observed before and after metadata reading.</summary>
/// <param name="Length">File size in bytes.</param>
/// <param name="LastModifiedUtc">Last-write timestamp reported by the host filesystem.</param>
public sealed record InspectedFileFacts(long Length, DateTimeOffset LastModifiedUtc);
