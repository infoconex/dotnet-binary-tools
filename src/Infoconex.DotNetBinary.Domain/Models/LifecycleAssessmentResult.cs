namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Independent lifecycle advisories and cache provenance warnings.</summary>
/// <param name="Findings">One finding per independently sourced declaration.</param>
/// <param name="DatasetWarning">Non-fatal warning when an invalid cache was bypassed.</param>
public sealed record LifecycleAssessmentResult(IReadOnlyList<LifecycleFinding> Findings, string? DatasetWarning);
