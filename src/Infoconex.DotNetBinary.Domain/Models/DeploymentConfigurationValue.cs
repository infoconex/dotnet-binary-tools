namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>A declared value and its exact XML element/attribute or JSON property origin.</summary>
public sealed record DeploymentConfigurationValue
{
    /// <summary>Creates one declared value without normalizing it.</summary>
    /// <param name="field">Absolute field path within the selected configuration file.</param>
    /// <param name="rawValue">The declared value, without runtime selection or normalization.</param>
    public DeploymentConfigurationValue(string field, string rawValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(rawValue);
        Field = field;
        RawValue = rawValue;
    }

    /// <summary>Absolute field path within the source document.</summary>
    public string Field { get; }

    /// <summary>Value as declared, never interpreted as an installed or running runtime.</summary>
    public string RawValue { get; }
}
