namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>How confidently the inspected content can be classified.</summary>
public enum AssemblyInspectionOutcome
{
    /// <summary>PE content with trustworthy CLI assembly metadata.</summary>
    ManagedAssembly,
    /// <summary>A valid native PE image with no managed CLI directory.</summary>
    NativeBinary,
    /// <summary>Content is not a recognizable or structurally valid assembly/PE.</summary>
    CorruptOrUnrecognized,
    /// <summary>The candidate cannot be read or changed while being inspected.</summary>
    Unreadable,
    /// <summary>Reading the candidate would exceed an explicit resource limit.</summary>
    ResourceLimited,
}
