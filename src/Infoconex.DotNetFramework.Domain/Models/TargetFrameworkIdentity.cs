namespace Infoconex.DotNetFramework.Domain.Models;

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

    /// <summary>
    /// Initializes an immutable framework identity from a declared moniker.
    /// </summary>
    /// <param name="moniker">The nonempty moniker read from metadata.</param>
    /// <exception cref="ArgumentException">The moniker is null, empty, or whitespace.</exception>
    public TargetFrameworkIdentity(string moniker)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moniker);
        Moniker = moniker;
    }
}
