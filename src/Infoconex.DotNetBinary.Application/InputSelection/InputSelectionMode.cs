namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>The explicit selection contract requested by a command.</summary>
public enum InputSelectionMode
{
    /// <summary>Require exactly one regular file.</summary>
    Inspect,

    /// <summary>Require one or more directory roots.</summary>
    Scan,
}
