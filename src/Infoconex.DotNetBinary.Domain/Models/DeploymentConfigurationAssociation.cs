namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Whether configuration was selected by the caller or by exact filename association.</summary>
public enum DeploymentConfigurationAssociation
{
    /// <summary>Discovered from an exact supported filename associated with the inspected file.</summary>
    Automatic,
    /// <summary>Explicitly supplied by the individual inspect request.</summary>
    Explicit,
}
