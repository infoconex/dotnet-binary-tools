namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>A bounded, non-sensitive reason a field or inspection was incomplete.</summary>
/// <param name="Code">Stable machine-readable diagnostic identifier.</param>
/// <param name="Field">Affected metadata field or file.</param>
public sealed record InspectionDiagnostic(string Code, string Field);
