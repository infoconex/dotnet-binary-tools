namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Recognized families of explicitly declared framework monikers.</summary>
public enum TargetFrameworkFamily
{
    /// <summary>An unfamiliar or unsupported declaration.</summary>
    Unrecognized,
    /// <summary>Classic .NET Framework.</summary>
    NetFramework,
    /// <summary>.NET Core 1–3.x.</summary>
    NetCore,
    /// <summary>Modern .NET 5–11.</summary>
    ModernDotNet,
    /// <summary>.NET Standard API contract, not a runtime.</summary>
    NetStandard,
}
