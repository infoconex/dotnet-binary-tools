using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.AssemblyInspection;
using Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;
using Infoconex.DotNetBinary.Infrastructure.InputSelection;
using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>
/// Adapts selected inspect and scan arguments to available capability boundaries.
/// Complete reporting, traversal and command-line functionality remain pending.
/// </summary>
public static class CliApplication
{
    /// <summary>
    /// Runs the CLI's informational and explicit input-selection operations.
    /// </summary>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="output">Destination for normal output.</param>
    /// <param name="error">Destination for diagnostic output.</param>
    /// <returns>Zero for informational commands; two for failures or unavailable downstream operations.</returns>
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (arguments.Count == 0 ||
            (arguments.Count == 1 && arguments[0] is "--help" or "-h" or "help"))
        {
            output.WriteLine("dotnet-binary: static .NET binary inspection (planned)");
            output.WriteLine("Usage: dotnet binary <command> [options]");
            output.WriteLine("Input selection: inspect <file> [--config <file>] (one regular file, any extension)");
            output.WriteLine("                 scan <directory> [<directory> ...]");
            output.WriteLine("Lifecycle reference update: lifecycle refresh (explicit network request)");
            output.WriteLine("Status: selected metadata/configuration evidence available; complete inspection, reporting and traversal not implemented yet.");
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
            if (mode == InputSelectionMode.Scan && arguments.Skip(1).Contains("--config", StringComparer.Ordinal))
            {
                error.WriteLine("--config is supported only for one inspect request; scan does not accept a global configuration.");
                return 2;
            }

            var hasExplicitConfiguration = mode == InputSelectionMode.Inspect &&
                arguments.Count == 4 && arguments[2] == "--config";
            if (mode == InputSelectionMode.Inspect && arguments.Contains("--config", StringComparer.Ordinal) &&
                !hasExplicitConfiguration)
            {
                error.WriteLine("inspect requires exactly one file followed by optional --config <file>.");
                return 2;
            }

            var paths = hasExplicitConfiguration ? new[] { arguments[1] } : arguments.Skip(1).ToArray();
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

            if (mode == InputSelectionMode.Inspect)
            {
                SelectedInput? configurationInput = null;
                if (hasExplicitConfiguration)
                {
                    var selectedConfiguration = selection.Select(InputSelectionMode.Inspect, [arguments[3]]);
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

                var configuration = new DeploymentConfigurationService(new FileDeploymentConfigurationReader())
                    .Inspect(result.Inputs[0], configurationInput);
                if (configuration.Outcome != DeploymentConfigurationOutcome.NotFound)
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

                var assembly = new AssemblyInspectionService(new PortableExecutableAssemblyReader())
                    .Inspect(result.Inputs[0]);
                if (assembly.Metadata?.DeclaredTarget is { } declaredTarget)
                {
                    output.WriteLine($"Assembly target ({declaredTarget.EvidenceSource}): {SafeDisplay(declaredTarget.Moniker)}");
                }

                var lifecycle = new LifecycleAssessmentService(new FileLifecycleDatasetStore(), TimeProvider.System)
                    .Assess(assembly.Metadata?.DeclaredTarget, configuration);
                if (lifecycle.DatasetWarning is not null)
                {
                    error.WriteLine(lifecycle.DatasetWarning);
                }

                foreach (var finding in lifecycle.Findings)
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

            // Do not claim successful inspection or a completed inventory until
            // downstream capabilities are implemented and coordinated.
            error.WriteLine("Input selection succeeded; complete inspection and traversal are not implemented yet.");
            return 2;
        }

        error.WriteLine("The requested command is not implemented yet.");
        return 2;
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
