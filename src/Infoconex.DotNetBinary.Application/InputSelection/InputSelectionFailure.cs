namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>An actionable, safely formatted failure of an explicit input request.</summary>
public sealed record InputSelectionFailure
{
    /// <summary>Creates a selection failure without exposing platform exception messages.</summary>
    /// <param name="requestedPath">The supplied input, or null for command-level cardinality.</param>
    /// <param name="reason">The stable failure classification.</param>
    /// <param name="message">A safe diagnostic explaining the resolution.</param>
    public InputSelectionFailure(string? requestedPath, InputSelectionFailureReason reason, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        RequestedPath = requestedPath;
        Reason = reason;
        Message = message;
    }

    /// <summary>The offending literal input, if applicable.</summary>
    public string? RequestedPath { get; }

    /// <summary>The stable error classification.</summary>
    public InputSelectionFailureReason Reason { get; }

    /// <summary>A diagnostic suitable for the command's error stream.</summary>
    public string Message { get; }
}
