namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Independently verified facts from one managed assembly's PE/CLI metadata.</summary>
public sealed record AssemblyMetadata
{
    /// <summary>Manifest identity.</summary>
    public AssemblyIdentity? Identity { get; init; }

    /// <summary>TargetFrameworkAttribute declaration, or null when absent or unreadable.</summary>
    public TargetFrameworkIdentity? DeclaredTarget { get; init; }

    /// <summary>Raw CLR metadata version, never interpreted as a target framework.</summary>
    public string? ClrMetadataVersion { get; init; }

    /// <summary>PE machine type name, not observed process architecture.</summary>
    public string? PeMachine { get; init; }

    /// <summary>Raw CLI CorFlags name, not observed process architecture.</summary>
    public string? CorFlags { get; init; }

    /// <summary>Whether the CLI header declares a nonzero entry point.</summary>
    public bool HasEntryPoint { get; init; }

    /// <summary>Only directly declared AssemblyRef identities.</summary>
    public IReadOnlyList<AssemblyIdentity> References { get; init; } = [];

    /// <summary>Optional AssemblyFileVersionAttribute value.</summary>
    public string? FileVersion { get; init; }

    /// <summary>Optional AssemblyInformationalVersionAttribute value.</summary>
    public string? InformationalVersion { get; init; }

    /// <summary>Optional AssemblyCompanyAttribute value.</summary>
    public string? Company { get; init; }

    /// <summary>Optional AssemblyProductAttribute value.</summary>
    public string? Product { get; init; }

    /// <summary>Optional AssemblyDescriptionAttribute value.</summary>
    public string? Description { get; init; }
}
