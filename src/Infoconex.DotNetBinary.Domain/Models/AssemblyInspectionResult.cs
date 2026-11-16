namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>One classified file inspection and only its trustworthy independent evidence.</summary>
/// <param name="Outcome">Result classification.</param>
/// <param name="FileFacts">Observed facts, if file access succeeded.</param>
/// <param name="Metadata">Verified managed metadata, or null for other classifications.</param>
/// <param name="Sha256">Optional content digest, never produced without explicit request.</param>
/// <param name="Diagnostics">Safe field-specific diagnostics.</param>
public sealed record AssemblyInspectionResult(
    AssemblyInspectionOutcome Outcome,
    InspectedFileFacts? FileFacts,
    AssemblyMetadata? Metadata,
    string? Sha256,
    IReadOnlyList<InspectionDiagnostic> Diagnostics);
