namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>
/// Describes the kind of explicit filesystem root validated for downstream work.
/// </summary>
public enum InputKind
{
    /// <summary>A selected regular file.</summary>
    File,

    /// <summary>A selected directory root.</summary>
    Directory,
}
