using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.AssemblyInspection;
using Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;
using Infoconex.DotNetBinary.Infrastructure.InputSelection;
using Infoconex.DotNetBinary.Infrastructure.InspectionInventory;
using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>
/// Adapts selected inspect and scan arguments to the CAP-005 inventory boundary.
/// Complete reporting and command-line functionality remain pending.
/// </summary>
public static class CliApplication
{
    /// <summary>
    /// Runs informational operations and minimal inspect/scan inventory entry points.
    /// </summary>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="output">Destination for normal output.</param>
    /// <param name="error">Destination for diagnostic output.</param>
    /// <returns>Zero for valid inventories, even incomplete; two for invalid commands or roots.</returns>
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (arguments.Count == 0 ||
            (arguments.Count == 1 && arguments[0] is "--help" or "-h" or "help"))
        {
            output.WriteLine("dotnet-binary: static .NET binary inspection");
            output.WriteLine("Usage: dotnet binary <command> [options]");
            output.WriteLine("Input selection: inspect <file> [--config <file>] (one regular file, any extension)");
            output.WriteLine("                 scan <directory> [<directory> ...] [--recursive] [--follow-links]");
            output.WriteLine("Inventory bounds: --max-depth N, --max-candidates N, --max-file-mib N,");
            output.WriteLine("                  --max-elapsed-seconds N, --max-cpu-seconds N");
            output.WriteLine("Lifecycle reference update: lifecycle refresh (explicit network request)");
            output.WriteLine("Status: inventory available; complete JSON reporting and CLI options not implemented yet.");
            return 0;
        }

        if (arguments.Count == 1 && arguments[0] == "--version")
        {
            output.WriteLine(typeof(CliApplication).Assembly.GetName().Version?.ToString() ?? "unknown");
            return 0;
        }

        if (arguments.Count == 2 && arguments[0] == "lifecycle" && arguments[1] == "refresh")
        {
            using var client = MicrosoftLifecycleReleaseSource.CreateHttpClient();
            var refresh = new LifecycleRefreshService(new FileLifecycleDatasetStore(),
                new MicrosoftLifecycleReleaseSource(client), new MicrosoftReleaseIndexReader(), TimeProvider.System)
                .RefreshAsync().GetAwaiter().GetResult();
            if (!refresh.Succeeded)
            {
                error.WriteLine(refresh.Warning);
                error.WriteLine($"Retained lifecycle dataset version: {SafeDisplay(refresh.Dataset.Version)}");
                return 2;
            }

            output.WriteLine($"Lifecycle dataset refreshed: {SafeDisplay(refresh.Dataset.Version)}");
            return 0;
        }

        if (arguments.Count > 0 && arguments[0] is "inspect" or "scan")
        {
            var mode = arguments[0] == "inspect" ? InputSelectionMode.Inspect : InputSelectionMode.Scan;
            if (!TryParseInventoryArguments(arguments, mode, error, out var paths, out var options,
                out var configurationPath))
            {
                return 2;
            }

            var selection = new InputSelectionService(new HostFileSystemInputPathProbe());
            var result = selection.Select(mode, paths);
            if (!result.IsSuccess)
            {
                foreach (var failure in result.Failures)
                {
                    error.WriteLine(failure.Message);
                }

                return 2;
            }

            SelectedInput? configurationInput = null;
            if (configurationPath is not null)
            {
                var selectedConfiguration = selection.Select(InputSelectionMode.Inspect, [configurationPath]);
                if (!selectedConfiguration.IsSuccess)
                {
                    foreach (var failure in selectedConfiguration.Failures)
                    {
                        error.WriteLine(failure.Message);
                    }

                    return 2;
                }

                configurationInput = selectedConfiguration.Inputs[0];
            }

            using var cpuClock = new HostInventoryCpuClock();
            var inventoryService = new InventoryService(new HostInventoryFileSystem(),
                new InventoryCandidateInspectionService(
                    new AssemblyInspectionService(new PortableExecutableAssemblyReader()),
                    new DeploymentConfigurationService(new FileDeploymentConfigurationReader()),
                    new LifecycleAssessmentService(new FileLifecycleDatasetStore(), TimeProvider.System)),
                TimeProvider.System, cpuClock);
            var inventory = mode == InputSelectionMode.Inspect
                ? inventoryService.Inspect(result.Inputs[0], configurationInput, options)
                : inventoryService.Scan(result.Inputs, options);
            output.WriteLine($"Inventory roots: {inventory.Roots.Count}");
            output.WriteLine($"Inventory candidates: {inventory.Summary.TotalCandidates}");
            output.WriteLine($"Inventory coverage: {inventory.Completion.State}");
            foreach (var reason in inventory.Completion.Reasons)
            {
                output.WriteLine($"Inventory coverage limitation: {reason}");
            }

            output.WriteLine($"Inventory distribution: managed={inventory.Summary.Managed}; native={inventory.Summary.Native}; " +
                $"unknown-target={inventory.Summary.UnknownTargets}; failed={inventory.Summary.Failed}; " +
                $"skipped={inventory.Summary.Skipped}; duplicate={inventory.Summary.Duplicates}; " +
                $"traversal-errors={inventory.Summary.TraversalErrors}");
            foreach (var item in inventory.Candidates)
            {
                output.WriteLine($"Candidate ({item.Assembly.Outcome}): {SafeDisplay(item.Input.FullPath)}");
            }

            foreach (var skipped in inventory.Skipped)
            {
                output.WriteLine($"Skipped ({skipped.Reason}) under {SafeDisplay(skipped.Root)}: {SafeDisplay(skipped.Path)}");
            }

            foreach (var traversalError in inventory.Errors)
            {
                output.WriteLine($"Traversal error ({traversalError.Code}) under {SafeDisplay(traversalError.Root)}: " +
                    SafeDisplay(traversalError.Path));
            }

            if (mode == InputSelectionMode.Inspect)
            {
                var candidate = inventory.Candidates[0];
                var configuration = candidate.Configuration;
                if (configuration is not null && configuration.Outcome != DeploymentConfigurationOutcome.NotFound)
                {
                    output.WriteLine($"Configuration: {configuration.Outcome} ({configuration.Association})");
                    output.WriteLine($"Configuration source: {SafeDisplay(configuration.Path)}");
                    foreach (var value in configuration.Values)
                    {
                        output.WriteLine($"Configured {value.Field}={SafeDisplay(value.RawValue)}");
                    }

                    foreach (var request in configuration.RuntimeRequests)
                    {
                        foreach (var value in request.Values)
                        {
                            output.WriteLine($"Configured {value.Field}={SafeDisplay(value.RawValue)}");
                        }
                    }

                    foreach (var diagnostic in configuration.Diagnostics)
                    {
                        error.WriteLine($"Configuration {diagnostic.Code} ({diagnostic.Field}).");
                    }

                }

                var assembly = candidate.Assembly;
                if (assembly.Metadata?.DeclaredTarget is { } declaredTarget)
                {
                    output.WriteLine($"Assembly target ({declaredTarget.EvidenceSource}): {SafeDisplay(declaredTarget.Moniker)}");
                }

                var lifecycle = candidate.Lifecycle;
                if (lifecycle?.DatasetWarning is not null)
                {
                    error.WriteLine(lifecycle.DatasetWarning);
                }

                foreach (var finding in lifecycle?.Findings ?? [])
                {
                    output.WriteLine($"Lifecycle ({finding.EvidenceKind}, {SafeDisplay(finding.EvidenceSource)}): {finding.Status}");
                    output.WriteLine($"Lifecycle declaration: {SafeDisplay(finding.DeclaredIdentity)}");
                    output.WriteLine($"Lifecycle assessment date: {finding.AssessmentDate:yyyy-MM-dd}; dataset: {SafeDisplay(finding.DatasetVersion)}");
                    output.WriteLine($"Lifecycle source: {finding.SourceIdentity}; {SafeDisplay(finding.SourceUrl ?? "unavailable")}; " +
                        $"publication: {finding.SourcePublishedOn?.ToString("yyyy-MM-dd") ?? "unknown"}; " +
                        $"retrieval: {finding.SourceRetrievedOn:yyyy-MM-dd}");
                    if (finding.EndOfSupport is { } end)
                    {
                        output.WriteLine($"Lifecycle published end date: {end:yyyy-MM-dd}");
                    }

                    output.WriteLine($"Lifecycle limitation: {SafeDisplay(finding.Limitation)}");
                    if (finding.IsStale)
                    {
                        output.WriteLine("Lifecycle warning: local reference data is stale or future-dated; no automatic refresh occurred.");
                    }
                }
            }

            // Incomplete coverage is an honest successful inventory, not an invalid command.
            // CAP-006 JSON and full CAP-007 parsing remain separate future capabilities.
            return 0;
        }

        error.WriteLine("The requested command is not implemented yet.");
        return 2;
    }

    private static bool TryParseInventoryArguments(IReadOnlyList<string> arguments, InputSelectionMode mode,
        TextWriter error, out IReadOnlyList<string> paths, out InventoryOptions options, out string? configurationPath)
    {
        var selectedPaths = new List<string>();
        configurationPath = null;
        options = new InventoryOptions();
        var literalPaths = false;
        for (var index = 1; index < arguments.Count; index++)
        {
            var token = arguments[index];
            if (!literalPaths && token == "--")
            {
                literalPaths = true;
                continue;
            }

            if (!literalPaths && token is "--recursive" or "--follow-links")
            {
                if (mode != InputSelectionMode.Scan)
                {
                    error.WriteLine($"{token} is supported only for scan.");
                    paths = [];
                    return false;
                }

                options = token == "--recursive" ? options with { Recursive = true } :
                    options with { FollowLinks = true };
                continue;
            }

            if (!literalPaths && token == "--config")
            {
                if (mode != InputSelectionMode.Inspect || configurationPath is not null || ++index == arguments.Count)
                {
                    error.WriteLine("--config is supported only for one inspect request with one configuration file.");
                    paths = [];
                    return false;
                }

                configurationPath = arguments[index];
                continue;
            }

            if (!literalPaths && token is "--max-depth" or "--max-candidates" or "--max-file-mib" or
                "--max-elapsed-seconds" or "--max-cpu-seconds")
            {
                if (++index == arguments.Count || !int.TryParse(arguments[index], out var number))
                {
                    error.WriteLine($"{token} requires an integer value.");
                    paths = [];
                    return false;
                }

                options = token switch
                {
                    "--max-depth" => options with { MaxDepth = number },
                    "--max-candidates" => options with { MaxCandidates = number },
                    "--max-file-mib" => options with { MaxFileBytes = (long)number * 1024 * 1024 },
                    "--max-elapsed-seconds" => options with { MaxElapsedSeconds = number },
                    _ => options with { MaxCpuSeconds = number },
                };
                continue;
            }

            if (!literalPaths && token.StartsWith("--", StringComparison.Ordinal))
            {
                error.WriteLine($"Unsupported inventory option: {SafeDisplay(token)}");
                paths = [];
                return false;
            }

            selectedPaths.Add(token);
        }

        try
        {
            options.Validate();
        }
        catch (ArgumentOutOfRangeException exception)
        {
            var option = exception.ParamName switch
            {
                nameof(InventoryOptions.MaxDepth) => "--max-depth",
                nameof(InventoryOptions.MaxCandidates) => "--max-candidates",
                nameof(InventoryOptions.MaxFileBytes) => "--max-file-mib",
                nameof(InventoryOptions.MaxElapsedSeconds) => "--max-elapsed-seconds",
                _ => "--max-cpu-seconds",
            };
            error.WriteLine($"Invalid {option} value; see --help for supported resource limits.");
            paths = [];
            return false;
        }

        paths = selectedPaths;
        return true;
    }

    private static string SafeDisplay(string? content)
    {
        if (content is null)
        {
            return "";
        }

        const int MaxLength = 256;
        var limited = string.Concat(content.Take(MaxLength).Select(character =>
            char.IsControl(character) || character == '\u001b' ? '?' : character));
        return content.Length > MaxLength ? limited + "..." : limited;
    }
}
