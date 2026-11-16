namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>A single declared runtime request, independent of assembly target and runtime availability.</summary>
public sealed record DeploymentRuntimeRequest
{
    /// <summary>Creates a runtime request from independently sourced, declared fields.</summary>
    /// <param name="origin">The supportedRuntime element or framework object, including its index.</param>
    /// <param name="values">Only fields actually declared for this request, each with its source.</param>
    public DeploymentRuntimeRequest(string origin, IReadOnlyList<DeploymentConfigurationValue> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentNullException.ThrowIfNull(values);
        Origin = origin;
        Values = Array.AsReadOnly(values.ToArray());
    }

    /// <summary>The source element or framework object, with any ordinal index.</summary>
    public string Origin { get; }

    /// <summary>Declared fields and their individual origins.</summary>
    public IReadOnlyList<DeploymentConfigurationValue> Values { get; }
}
