namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>The independent source category of an assessment.</summary>
public enum LifecycleEvidenceKind
{
    /// <summary>Assembly metadata target, not a runtime observation.</summary>
    AssemblyTarget,
    /// <summary>Deployment configuration runtime request, not an installed runtime.</summary>
    ConfiguredRuntime,
}
