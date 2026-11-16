namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Dated, source-backed advisory about a declaration, not observed deployment.</summary>
/// <param name="EvidenceKind">Assembly target or separate configured runtime request.</param>
/// <param name="DeclaredIdentity">Original evidence identity, not the executing runtime.</param>
/// <param name="EvidenceSource">CAP-002 or CAP-003 origin of the declaration.</param>
/// <param name="Family">Product family, if recognized.</param>
/// <param name="Status">Advisory lifecycle classification.</param>
/// <param name="EndOfSupport">Published date, never inferred for unknowns.</param>
/// <param name="AssessmentDate">Date the assessment was performed in UTC.</param>
/// <param name="DatasetVersion">Local reference dataset version.</param>
/// <param name="SourceUrl">Official lifecycle evidence URL when available.</param>
/// <param name="SourcePublishedOn">Source publication date when known.</param>
/// <param name="SourceRetrievedOn">Reference retrieval date.</param>
/// <param name="IsStale">Whether reference retrieval is older than freshness policy.</param>
/// <param name="Limitation">What the declaration or source cannot establish.</param>
public sealed record LifecycleFinding(LifecycleEvidenceKind EvidenceKind, string DeclaredIdentity,
    string EvidenceSource, TargetFrameworkFamily Family, LifecycleStatus Status, DateOnly? EndOfSupport,
    DateOnly AssessmentDate, string DatasetVersion, string? SourceUrl, DateOnly? SourcePublishedOn,
    DateOnly SourceRetrievedOn, bool IsStale, string Limitation)
{
    /// <summary>Publisher/document identity, separate from its complete URL.</summary>
    public string SourceIdentity => SourceUrl switch
    {
        "https://dotnet.microsoft.com/en-us/platform/support/policy" => "Microsoft .NET Support Policy",
        "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework" => "Microsoft .NET Framework Lifecycle",
        "https://learn.microsoft.com/en-us/lifecycle/faq/dotnet-framework" => "Microsoft .NET Framework Lifecycle FAQ",
        _ => "Unknown source",
    };
}
