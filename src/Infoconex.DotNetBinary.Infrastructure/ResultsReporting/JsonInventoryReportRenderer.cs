using System.Globalization;
using System.Text.Json;
using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.ResultsReporting;

/// <summary>Writes JSON v1 directly from CAP-005 facts using reflection-free, bounded UTF-8 streaming.</summary>
public sealed class JsonInventoryReportRenderer : IInventoryReportRenderer
{
    /// <summary>Published version of the stable machine-readable report contract.</summary>
    public const string SchemaVersion = "1.0.0";

    /// <inheritdoc />
    public void Render(InventoryReport inventory, string command, DateTimeOffset generatedUtc,
        ReportOptions options, TextWriter destination)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(destination);
        options.Validate();
        using var stream = new Utf8TextWriterStream(destination);
        using (var json = new Utf8JsonWriter(stream))
        {
            json.WriteStartObject();
            json.WriteString("schemaVersion", SchemaVersion);
            json.WriteString("command", command);
            WriteInputs(json, inventory.Roots);
            json.WriteString("generatedAtUtc", generatedUtc.ToUniversalTime());
            WriteCompletion(json, inventory.Completion);
            WriteSummary(json, inventory.Summary);
            WriteLifecycleDatasets(json, inventory.Candidates);
            json.WritePropertyName("candidates");
            json.WriteStartArray();
            foreach (var candidate in inventory.Candidates)
            {
                WriteCandidate(json, candidate, options.Fields);
                // Utf8JsonWriter buffers only the current chunk; force delivery as inventory grows.
                json.Flush();
            }

            json.WriteEndArray();
            WriteSkipped(json, inventory.Skipped);
            WriteErrors(json, inventory.Errors);
            json.WriteEndObject();
            json.Flush();
        }

        stream.Complete();
    }

    private static void WriteInputs(Utf8JsonWriter json, IReadOnlyList<SelectedInput> roots)
    {
        json.WritePropertyName("inputs");
        json.WriteStartArray();
        foreach (var input in roots)
        {
            json.WriteStartObject();
            json.WriteString("requestedPath", input.RequestedPath);
            json.WriteString("fullPath", input.FullPath);
            json.WriteString("kind", input.Kind.ToString());
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static void WriteCompletion(Utf8JsonWriter json, InventoryCompletion completion)
    {
        json.WritePropertyName("completion");
        json.WriteStartObject();
        json.WriteString("state", completion.State.ToString());
        json.WritePropertyName("reasons");
        json.WriteStartArray();
        foreach (var reason in completion.Reasons) json.WriteStringValue(reason.ToString());
        json.WriteEndArray();
        json.WriteEndObject();
    }

    private static void WriteSummary(Utf8JsonWriter json, InventorySummary summary)
    {
        json.WritePropertyName("summary");
        json.WriteStartObject();
        json.WriteNumber("totalCandidates", summary.TotalCandidates);
        json.WriteNumber("managed", summary.Managed);
        json.WriteNumber("native", summary.Native);
        json.WriteNumber("corrupt", summary.Corrupt);
        json.WriteNumber("unreadable", summary.Unreadable);
        json.WriteNumber("resourceLimited", summary.ResourceLimited);
        json.WriteNumber("failed", summary.Failed);
        json.WriteNumber("configurationFailures", summary.ConfigurationFailures);
        json.WriteNumber("unknownTargets", summary.UnknownTargets);
        json.WriteNumber("skipped", summary.Skipped);
        json.WriteNumber("duplicates", summary.Duplicates);
        json.WriteNumber("traversalErrors", summary.TraversalErrors);
        json.WritePropertyName("lifecycle");
        json.WriteStartArray();
        foreach (var count in summary.Lifecycle)
        {
            json.WriteStartObject();
            json.WriteString("evidenceKind", count.EvidenceKind.ToString());
            json.WriteString("status", count.Status.ToString());
            json.WriteNumber("count", count.Count);
            json.WriteEndObject();
        }

        json.WriteEndArray();
        json.WriteEndObject();
    }

    private static void WriteLifecycleDatasets(Utf8JsonWriter json, IReadOnlyList<InventoryCandidate> candidates)
    {
        json.WritePropertyName("lifecycleDatasets");
        json.WriteStartArray();
        var versions = new HashSet<(string, string?, DateOnly?, DateOnly)>();
        foreach (var finding in candidates.SelectMany(item => item.Lifecycle?.Findings ?? []))
        {
            if (!versions.Add((finding.DatasetVersion, finding.SourceUrl,
                finding.SourcePublishedOn, finding.SourceRetrievedOn))) continue;
            json.WriteStartObject();
            json.WriteString("version", finding.DatasetVersion);
            NullableString(json, "sourceUrl", finding.SourceUrl);
            NullableDate(json, "sourcePublishedOn", finding.SourcePublishedOn);
            json.WriteString("sourceRetrievedOn", Date(finding.SourceRetrievedOn));
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static void WriteCandidate(Utf8JsonWriter json, InventoryCandidate candidate,
        IReadOnlyList<string>? selectedFields)
    {
        json.WriteStartObject();
        json.WriteString("path", candidate.Input.FullPath);
        json.WriteString("outcome", candidate.Assembly.Outcome.ToString());
        json.WritePropertyName("fileFacts");
        if (candidate.Assembly.FileFacts is { } facts)
        {
            json.WriteStartObject();
            json.WriteNumber("length", facts.Length);
            json.WriteString("lastModifiedUtc", facts.LastModifiedUtc.ToUniversalTime());
            json.WriteEndObject();
        }
        else json.WriteNullValue();

        json.WritePropertyName("assembly");
        json.WriteStartObject();
        foreach (var field in selectedFields ?? ReportOptions.SupportedFields)
        {
            WriteAssemblyField(json, field, candidate.Assembly.Metadata);
        }

        json.WriteEndObject();
        NullableString(json, "sha256", candidate.Assembly.Sha256);
        WriteDiagnostics(json, "diagnostics", candidate.Assembly.Diagnostics);
        WriteConfiguration(json, candidate.Configuration);
        WriteLifecycle(json, candidate.Lifecycle);
        json.WriteEndObject();
    }

    private static void WriteAssemblyField(Utf8JsonWriter json, string field, AssemblyMetadata? metadata)
    {
        switch (field)
        {
            case "name": NullableString(json, field, metadata?.Identity?.Name); break;
            case "version": NullableString(json, field, metadata?.Identity?.Version.ToString()); break;
            case "targetFramework": NullableString(json, field, metadata?.DeclaredTarget?.Moniker); break;
            case "fileVersion": NullableString(json, field, metadata?.FileVersion); break;
            case "informationalVersion": NullableString(json, field, metadata?.InformationalVersion); break;
            case "company": NullableString(json, field, metadata?.Company); break;
            case "product": NullableString(json, field, metadata?.Product); break;
            case "description": NullableString(json, field, metadata?.Description); break;
            case "clrMetadataVersion": NullableString(json, field, metadata?.ClrMetadataVersion); break;
            case "peMachine": NullableString(json, field, metadata?.PeMachine); break;
            case "corFlags": NullableString(json, field, metadata?.CorFlags); break;
            case "hasEntryPoint":
                if (metadata is null) json.WriteNull(field);
                else json.WriteBoolean(field, metadata.HasEntryPoint);
                break;
            case "references":
                json.WritePropertyName(field);
                if (metadata is null) json.WriteNullValue();
                else
                {
                    json.WriteStartArray();
                    foreach (var reference in metadata.References)
                    {
                        json.WriteStartObject();
                        json.WriteString("name", reference.Name);
                        json.WriteString("version", reference.Version.ToString());
                        NullableString(json, "culture", reference.Culture);
                        NullableString(json, "publicKeyToken", reference.PublicKeyToken);
                        json.WriteEndObject();
                    }

                    json.WriteEndArray();
                }

                break;
            default: throw new ArgumentException("Unsupported projected field.", nameof(field));
        }
    }

    private static void WriteConfiguration(Utf8JsonWriter json, DeploymentConfigurationResult? configuration)
    {
        json.WritePropertyName("configuration");
        if (configuration is null)
        {
            json.WriteNullValue();
            return;
        }

        json.WriteStartObject();
        json.WriteString("outcome", configuration.Outcome.ToString());
        NullableString(json, "path", configuration.Path);
        NullableString(json, "association", configuration.Association?.ToString());
        json.WritePropertyName("values");
        WriteConfigValues(json, configuration.Values);
        json.WritePropertyName("runtimeRequests");
        json.WriteStartArray();
        foreach (var request in configuration.RuntimeRequests)
        {
            json.WriteStartObject();
            json.WriteString("origin", request.Origin);
            json.WritePropertyName("values");
            WriteConfigValues(json, request.Values);
            json.WriteEndObject();
        }

        json.WriteEndArray();
        WriteDiagnostics(json, "diagnostics", configuration.Diagnostics);
        json.WriteEndObject();
    }

    private static void WriteConfigValues(Utf8JsonWriter json, IReadOnlyList<DeploymentConfigurationValue> values)
    {
        json.WriteStartArray();
        foreach (var value in values)
        {
            json.WriteStartObject();
            json.WriteString("field", value.Field);
            json.WriteString("rawValue", value.RawValue);
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static void WriteLifecycle(Utf8JsonWriter json, LifecycleAssessmentResult? lifecycle)
    {
        json.WritePropertyName("lifecycle");
        if (lifecycle is null)
        {
            json.WriteNullValue();
            return;
        }

        json.WriteStartObject();
        NullableString(json, "datasetWarning", lifecycle.DatasetWarning);
        json.WritePropertyName("findings");
        json.WriteStartArray();
        foreach (var finding in lifecycle.Findings)
        {
            json.WriteStartObject();
            json.WriteString("evidenceKind", finding.EvidenceKind.ToString());
            json.WriteString("declaredIdentity", finding.DeclaredIdentity);
            json.WriteString("evidenceSource", finding.EvidenceSource);
            json.WriteString("family", finding.Family.ToString());
            json.WriteString("status", finding.Status.ToString());
            NullableDate(json, "endOfSupport", finding.EndOfSupport);
            json.WriteString("assessmentDate", Date(finding.AssessmentDate));
            json.WriteString("datasetVersion", finding.DatasetVersion);
            NullableString(json, "sourceUrl", finding.SourceUrl);
            json.WriteString("sourceIdentity", finding.SourceIdentity);
            NullableDate(json, "sourcePublishedOn", finding.SourcePublishedOn);
            json.WriteString("sourceRetrievedOn", Date(finding.SourceRetrievedOn));
            json.WriteBoolean("isStale", finding.IsStale);
            json.WriteString("limitation", finding.Limitation);
            json.WriteEndObject();
        }

        json.WriteEndArray();
        json.WriteEndObject();
    }

    private static void WriteSkipped(Utf8JsonWriter json, IReadOnlyList<InventorySkippedPath> skipped)
    {
        json.WritePropertyName("skipped");
        json.WriteStartArray();
        foreach (var item in skipped)
        {
            json.WriteStartObject();
            json.WriteString("root", item.Root);
            json.WriteString("path", item.Path);
            json.WriteString("reason", item.Reason.ToString());
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static void WriteErrors(Utf8JsonWriter json, IReadOnlyList<InventoryTraversalError> errors)
    {
        json.WritePropertyName("errors");
        json.WriteStartArray();
        foreach (var error in errors)
        {
            json.WriteStartObject();
            json.WriteString("root", error.Root);
            json.WriteString("path", error.Path);
            json.WriteString("code", error.Code);
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static void WriteDiagnostics(Utf8JsonWriter json, string name, IReadOnlyList<InspectionDiagnostic> diagnostics)
    {
        json.WritePropertyName(name);
        json.WriteStartArray();
        foreach (var diagnostic in diagnostics)
        {
            json.WriteStartObject();
            json.WriteString("code", diagnostic.Code);
            json.WriteString("field", diagnostic.Field);
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static void NullableString(Utf8JsonWriter json, string name, string? value)
    {
        if (value is null) json.WriteNull(name);
        else json.WriteString(name, value);
    }

    private static void NullableDate(Utf8JsonWriter json, string name, DateOnly? value)
    {
        if (value is null) json.WriteNull(name);
        else json.WriteString(name, Date(value.Value));
    }

    private static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
