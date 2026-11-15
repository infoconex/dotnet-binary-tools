namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>Stable classifications for explicit input selection failures.</summary>
public enum InputSelectionFailureReason
{
    /// <summary>No failure was observed.</summary>
    None,

    /// <summary>The command has an invalid number of selected paths.</summary>
    InvalidCount,

    /// <summary>The specified path does not exist.</summary>
    Missing,

    /// <summary>The path exists but has the wrong kind for the operation.</summary>
    WrongKind,

    /// <summary>Filesystem access was denied or the input could not be accessed.</summary>
    Inaccessible,

    /// <summary>The supplied path is not valid under host path rules.</summary>
    InvalidPath,

    /// <summary>An explicitly selected link has no existing target.</summary>
    BrokenLink,

    /// <summary>The filesystem could not establish the path status.</summary>
    Unavailable,
}
