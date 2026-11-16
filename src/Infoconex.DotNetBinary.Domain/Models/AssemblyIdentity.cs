namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Assembly identity declared in metadata, independent of the file version.</summary>
/// <param name="Name">Declared simple name.</param>
/// <param name="Version">Declared assembly version.</param>
/// <param name="Culture">Declared culture, if non-neutral.</param>
/// <param name="PublicKeyToken">Lowercase public-key token, if present.</param>
public sealed record AssemblyIdentity(string Name, Version Version, string? Culture, string? PublicKeyToken)
{
    /// <summary>Combines the declared identity fields in the .NET display-name form.</summary>
    public string FullName => $"{Name}, Version={Version}, Culture={Culture ?? "neutral"}, PublicKeyToken={PublicKeyToken ?? "null"}";
}
