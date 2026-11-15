using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>A single host-filesystem path classification with no inspection contents.</summary>
public sealed record InputPathProbeResult
{
    private InputPathProbeResult(string? fullPath, InputKind? kind, InputSelectionFailureReason failure)
    {
        FullPath = fullPath;
        Kind = kind;
        Failure = failure;
    }

    /// <summary>Gets the qualified path when the probe succeeded.</summary>
    public string? FullPath { get; }

    /// <summary>Gets the observed kind when the probe succeeded.</summary>
    public InputKind? Kind { get; }

    /// <summary>Gets the failure reason, or <see cref="InputSelectionFailureReason.None"/>.</summary>
    public InputSelectionFailureReason Failure { get; }

    /// <summary>Creates a successful input-path classification.</summary>
    public static InputPathProbeResult Found(string fullPath, InputKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        return new(fullPath, kind, InputSelectionFailureReason.None);
    }

    /// <summary>Creates a failed input-path classification.</summary>
    public static InputPathProbeResult Failed(InputSelectionFailureReason reason)
    {
        if (reason is InputSelectionFailureReason.None or InputSelectionFailureReason.InvalidCount or InputSelectionFailureReason.WrongKind || !Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason));
        }

        return new(null, null, reason);
    }
}
