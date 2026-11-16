using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.ResultsReporting;

/// <summary>Concise text with optional expanded metadata from exactly the same inventory facts as JSON.</summary>
public sealed class HumanInventoryReportRenderer : IInventoryReportRenderer
{
    /// <inheritdoc />
    public void Render(InventoryReport inventory, string command, DateTimeOffset generatedUtc,
        ReportOptions options, TextWriter destination)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(destination);
        options.Validate();
        destination.WriteLine($"Inventory roots: {inventory.Roots.Count}");
        destination.WriteLine($"Inventory candidates: {inventory.Summary.TotalCandidates}");
        destination.WriteLine($"Inventory coverage: {inventory.Completion.State}");
        foreach (var reason in inventory.Completion.Reasons)
        {
            destination.WriteLine($"Inventory coverage limitation: {reason}");
        }

        destination.WriteLine($"Inventory distribution: managed={inventory.Summary.Managed}; native={inventory.Summary.Native}; " +
            $"unknown-target={inventory.Summary.UnknownTargets}; failed={inventory.Summary.Failed}; " +
            $"skipped={inventory.Summary.Skipped}; duplicate={inventory.Summary.Duplicates}; " +
            $"traversal-errors={inventory.Summary.TraversalErrors}");

        foreach (var candidate in inventory.Candidates)
        {
            destination.WriteLine($"Candidate ({candidate.Assembly.Outcome}): {Safe(candidate.Input.FullPath)}");
            WriteAssembly(destination, candidate.Assembly.Metadata, options);
            WriteConfiguration(destination, candidate.Configuration, options.Details);
            WriteLifecycle(destination, candidate.Lifecycle, options.Details);
            if (options.Details)
            {
                foreach (var diagnostic in candidate.Assembly.Diagnostics)
                {
                    destination.WriteLine($"Assembly diagnostic: {Safe(diagnostic.Code)} ({Safe(diagnostic.Field)})");
                }
            }
        }

        foreach (var skipped in inventory.Skipped)
        {
            destination.WriteLine($"Skipped ({skipped.Reason}) under {Safe(skipped.Root)}: {Safe(skipped.Path)}");
        }

        foreach (var error in inventory.Errors)
        {
            destination.WriteLine($"Traversal error ({Safe(error.Code)}) under {Safe(error.Root)}: {Safe(error.Path)}");
        }
    }

    private static void WriteAssembly(TextWriter writer, AssemblyMetadata? metadata, ReportOptions options)
    {
        if (options.Fields is { } projection)
        {
            foreach (var field in projection)
            {
                var value = AssemblyField(metadata, field);
                writer.WriteLine($"Assembly {field}: {Safe(value ?? "unknown")}");
            }

            return;
        }

        if (metadata?.DeclaredTarget is { } target)
        {
            writer.WriteLine($"Assembly target ({target.EvidenceSource}): {Safe(target.Moniker)}");
        }

        if (metadata?.Identity is { } identity)
        {
            writer.WriteLine($"Assembly: {Safe(identity.Name)} {Safe(identity.Version.ToString())}");
        }

        if (!options.Details || metadata is null) return;

        foreach (var field in ReportOptions.SupportedFields)
        {
            if (field is "name" or "version" or "targetFramework") continue;
            var value = AssemblyField(metadata, field);
            if (value is not null) writer.WriteLine($"Assembly {field}: {Safe(value)}");
        }
    }

    private static string? AssemblyField(AssemblyMetadata? metadata, string field) => field switch
    {
        "name" => metadata?.Identity?.Name,
        "version" => metadata?.Identity?.Version.ToString(),
        "targetFramework" => metadata?.DeclaredTarget?.Moniker,
        "fileVersion" => metadata?.FileVersion,
        "informationalVersion" => metadata?.InformationalVersion,
        "company" => metadata?.Company,
        "product" => metadata?.Product,
        "description" => metadata?.Description,
        "references" => metadata is null ? null : string.Join("; ", metadata.References.Select(item => item.FullName)),
        "clrMetadataVersion" => metadata?.ClrMetadataVersion,
        "peMachine" => metadata?.PeMachine,
        "corFlags" => metadata?.CorFlags,
        "hasEntryPoint" => metadata?.HasEntryPoint.ToString(),
        _ => throw new ArgumentException("Unsupported projected field.", nameof(field)),
    };

    private static void WriteConfiguration(TextWriter writer, DeploymentConfigurationResult? configuration, bool details)
    {
        if (configuration is null || configuration.Outcome == DeploymentConfigurationOutcome.NotFound) return;
        writer.WriteLine($"Configuration: {configuration.Outcome} ({configuration.Association})");
        writer.WriteLine($"Configuration source: {Safe(configuration.Path)}");
        foreach (var value in configuration.Values)
        {
            writer.WriteLine($"Configured {Safe(value.Field)}={Safe(value.RawValue)}");
        }

        foreach (var request in configuration.RuntimeRequests)
        {
            foreach (var value in request.Values)
            {
                writer.WriteLine($"Configured {Safe(value.Field)}={Safe(value.RawValue)}");
            }
        }

        if (!details) return;
        foreach (var diagnostic in configuration.Diagnostics)
        {
            writer.WriteLine($"Configuration diagnostic: {Safe(diagnostic.Code)} ({Safe(diagnostic.Field)})");
        }
    }

    private static void WriteLifecycle(TextWriter writer, LifecycleAssessmentResult? lifecycle, bool details)
    {
        if (lifecycle?.DatasetWarning is not null)
        {
            writer.WriteLine("Lifecycle warning: local dataset cache was bypassed.");
        }

        foreach (var finding in lifecycle?.Findings ?? [])
        {
            writer.WriteLine($"Lifecycle ({finding.EvidenceKind}, {Safe(finding.EvidenceSource)}): {finding.Status}");
            writer.WriteLine($"Lifecycle declaration: {Safe(finding.DeclaredIdentity)}");
            if (finding.IsStale)
            {
                writer.WriteLine("Lifecycle warning: local reference data is stale; no automatic refresh occurred.");
            }

            if (!details) continue;
            writer.WriteLine($"Lifecycle assessment date: {finding.AssessmentDate:yyyy-MM-dd}; " +
                $"dataset: {Safe(finding.DatasetVersion)}");
            writer.WriteLine($"Lifecycle source: {Safe(finding.SourceIdentity)}; {Safe(finding.SourceUrl ?? "unavailable")}; " +
                $"publication: {finding.SourcePublishedOn?.ToString("yyyy-MM-dd") ?? "unknown"}; " +
                $"retrieval: {finding.SourceRetrievedOn:yyyy-MM-dd}");
            if (finding.EndOfSupport is { } end)
            {
                writer.WriteLine($"Lifecycle published end date: {end:yyyy-MM-dd}");
            }

            writer.WriteLine($"Lifecycle limitation: {Safe(finding.Limitation)}");
        }
    }

    private static string Safe(string? content)
    {
        if (content is null) return "";
        const int MaxLength = 256;
        var limited = string.Concat(content.Take(MaxLength).Select(character =>
            char.IsControl(character) || character == '\u001b' ? '?' : character));
        return content.Length > MaxLength ? limited + "..." : limited;
    }
}
