namespace Infoconex.DotNetBinary.Application.ResultsReporting;

/// <summary>Validated presentation options; these never alter inventory facts.</summary>
/// <param name="Format">Human (default) or machine-readable JSON.</param>
/// <param name="Details">Expand human metadata; incompatible with Fields.</param>
/// <param name="Fields">Optional approved per-assembly field projection.</param>
/// <param name="OutputPath">Optional exclusive output file; null directs the report to stdout.</param>
public sealed record ReportOptions(ReportFormat Format = ReportFormat.Human, bool Details = false,
    IReadOnlyList<string>? Fields = null, string? OutputPath = null)
{
    /// <summary>Public assembly fields supported in --fields (case-sensitive contract names).</summary>
    public static IReadOnlyList<string> SupportedFields { get; } = Array.AsReadOnly(
        new[] { "name", "version", "targetFramework", "fileVersion", "informationalVersion", "company", "product",
            "description", "references", "clrMetadataVersion", "peMachine", "corFlags", "hasEntryPoint" });

    /// <summary>Rejects incompatible options before any rendering or output-file creation.</summary>
    public void Validate()
    {
        if (!Enum.IsDefined(Format)) throw new ArgumentException("Unsupported --format value.");
        if (Details && Format == ReportFormat.Json)
        {
            throw new ArgumentException("--details is available only for human reports.");
        }

        if (Fields is not null)
        {
            if (Details) throw new ArgumentException("--details and --fields cannot be combined.");
            if (Fields.Count == 0) throw new ArgumentException("--fields requires one or more field names.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in Fields)
            {
                if (field is null || !SupportedFields.Contains(field, StringComparer.Ordinal))
                {
                    throw new ArgumentException("Unsupported --fields name. See --help for supported fields.");
                }

                if (!seen.Add(field)) throw new ArgumentException("Duplicate --fields name.");
            }
        }

        if (OutputPath is not null && string.IsNullOrWhiteSpace(OutputPath))
        {
            throw new ArgumentException("--output requires a nonempty file path.");
        }
    }
}
