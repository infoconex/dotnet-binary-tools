namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Outcome of reading only supported, non-executed deployment configuration data.</summary>
public enum DeploymentConfigurationOutcome
{
    /// <summary>Supported deployment declarations were parsed.</summary>
    Parsed,
    /// <summary>No directly associated supported file exists.</summary>
    NotFound,
    /// <summary>The configuration has invalid XML or JSON syntax.</summary>
    Malformed,
    /// <summary>The selected format or declaration shape is not supported.</summary>
    Unsupported,
    /// <summary>The configuration could not be read reliably.</summary>
    Unreadable,
    /// <summary>Reading or parsing exceeded a bounded resource policy.</summary>
    ResourceLimited,
}
