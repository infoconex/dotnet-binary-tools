namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>
/// Preserves a target-framework moniker as declared by assembly metadata.
/// This value does not establish runtime compatibility or the executing runtime.
/// </summary>
public sealed record TargetFrameworkIdentity
{
    /// <summary>
    /// Gets the original target-framework moniker without normalization.
    /// </summary>
    public string Moniker { get; }

    /// <summary>The attribute that provides this declaration, not a runtime observation.</summary>
    public string EvidenceSource => "System.Runtime.Versioning.TargetFrameworkAttribute";

    /// <summary>A recognized family, or Unrecognized if the declaration cannot be interpreted safely.</summary>
    public TargetFrameworkFamily Family { get; }

    /// <summary>Parsed framework version only when the declared family is recognized.</summary>
    public Version? Version { get; }

    /// <summary>
    /// Initializes an immutable framework identity from a declared moniker.
    /// </summary>
    /// <param name="moniker">The nonempty moniker read from metadata.</param>
    /// <exception cref="ArgumentException">The moniker is null, empty, or whitespace.</exception>
    public TargetFrameworkIdentity(string moniker)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moniker);
        Moniker = moniker;
        (Family, Version) = Identify(moniker);
    }

    private static (TargetFrameworkFamily Family, Version? Version) Identify(string moniker)
    {
        var fields = moniker.Split(',');
        if (fields.Length < 2 || !fields[1].Trim().StartsWith("Version=v", StringComparison.Ordinal) ||
            !Version.TryParse(fields[1].Trim()["Version=v".Length..], out var version) ||
            version.Minor < 0 || version.Revision >= 0)
        {
            return (TargetFrameworkFamily.Unrecognized, null);
        }

        var twoPartVersion = version.Build < 0;
        var family = fields[0].Trim() switch
        {
            ".NETFramework" when (twoPartVersion &&
                    ((version.Major >= 1 && version.Major <= 3) ||
                    (version.Major == 4 && version.Minor <= 8))) ||
                (version.Major == 4 && version.Minor == 8 && version.Build == 1) => TargetFrameworkFamily.NetFramework,
            ".NETCoreApp" when twoPartVersion && version.Major is >= 1 and <= 3 => TargetFrameworkFamily.NetCore,
            ".NETCoreApp" when twoPartVersion && version.Major is >= 5 and <= 11 && version.Minor == 0 => TargetFrameworkFamily.ModernDotNet,
            ".NETStandard" when twoPartVersion && ((version.Major == 1 && version.Minor <= 6) ||
                (version.Major == 2 && version.Minor <= 1)) => TargetFrameworkFamily.NetStandard,
            _ => TargetFrameworkFamily.Unrecognized,
        };
        return family == TargetFrameworkFamily.Unrecognized ? (family, null) : (family, version);
    }
}
