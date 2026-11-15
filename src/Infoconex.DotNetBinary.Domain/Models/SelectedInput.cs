namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>
/// Captures an explicit input after its path and kind have been validated.
/// This is a snapshot; it does not guarantee subsequent filesystem availability.
/// </summary>
public sealed record SelectedInput
{
    /// <summary>Creates an immutable description of a selected input.</summary>
    /// <param name="requestedPath">The literal path supplied by the caller.</param>
    /// <param name="fullPath">The host-qualified path, preserving an explicitly selected link.</param>
    /// <param name="kind">The filesystem kind observed during validation.</param>
    public SelectedInput(string requestedPath, string fullPath, InputKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        RequestedPath = requestedPath;
        FullPath = fullPath;
        Kind = kind;
    }

    /// <summary>Gets the original user-supplied literal path.</summary>
    public string RequestedPath { get; }

    /// <summary>Gets the fully qualified host path observed during selection.</summary>
    public string FullPath { get; }

    /// <summary>Gets the validated input kind.</summary>
    public InputKind Kind { get; }
}
