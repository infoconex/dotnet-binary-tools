using System.Reflection;
using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Application.ResultsReporting;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>Translates command-line intent to existing Application use cases.</summary>
public static class CliApplication
{
    /// <summary>Parses, validates, dispatches and presents one CLI invocation.</summary>
    /// <param name="arguments">Literal command-line arguments after the executable name.</param>
    /// <param name="output">Report-only output (or offline help/version responses).</param>
    /// <param name="error">Actionable, control-character-safe diagnostic output.</param>
    /// <param name="useCases">Optional substituted Application ports for deterministic verification.</param>
    /// <returns>0 for completed reports/help/version; 2 for usage/input errors; 1 for execution/output failures.</returns>
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error,
        CliUseCases? useCases = null)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (!CliArgumentParser.TryParse(arguments, out var invocation, out var diagnostic))
        {
            error.WriteLine(diagnostic);
            return 2;
        }

        if (invocation!.Command == "help")
        {
            WriteHelp(output);
            return 0;
        }

        if (invocation.Command == "version")
        {
            var assembly = typeof(CliApplication).Assembly;
            output.WriteLine(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
                assembly.GetName().Version?.ToString() ?? "unknown");
            return 0;
        }

        return useCases is null
            ? CliCompositionRoot.Execute(invocation, output, error)
            : Execute(invocation, output, error, useCases);
    }

    internal static int Execute(CliInvocation invocation, TextWriter output, TextWriter error, CliUseCases useCases)
    {
        try
        {
            if (invocation.Command == "refresh")
            {
                return Refresh(useCases, error, output, announceSuccess: true);
            }

            var mode = invocation.Command == "inspect" ? InputSelectionMode.Inspect : InputSelectionMode.Scan;
            var selected = useCases.Selection.Select(mode, invocation.Paths);
            if (!selected.IsSuccess)
            {
                foreach (var failure in selected.Failures) error.WriteLine(SafeDisplay(failure.Message));
                return 2;
            }

            Infoconex.DotNetBinary.Domain.Models.SelectedInput? configuration = null;
            if (invocation.ConfigurationPath is not null)
            {
                var selectedConfig = useCases.Selection.Select(InputSelectionMode.Inspect, [invocation.ConfigurationPath]);
                if (!selectedConfig.IsSuccess)
                {
                    foreach (var failure in selectedConfig.Failures) error.WriteLine(SafeDisplay(failure.Message));
                    return 2;
                }

                configuration = selectedConfig.Inputs[0];
            }

            if (invocation.RefreshLifecycle && Refresh(useCases, error, output, announceSuccess: false) != 0)
            {
                return 1;
            }

            var inventory = mode == InputSelectionMode.Inspect
                ? useCases.Inventory.Inspect(selected.Inputs[0], configuration, invocation.Inventory)
                : useCases.Inventory.Scan(selected.Inputs, invocation.Inventory);
            useCases.Reporting.Write(inventory, invocation.Command, invocation.Reporting, output, error);
            // Partial inventory coverage and per-candidate errors do not invalidate a completed report.
            return 0;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            error.WriteLine("Command orchestration, refresh, or reporting failed. No completed report can be claimed.");
            return 1;
        }
    }

    private static int Refresh(CliUseCases useCases, TextWriter error, TextWriter output, bool announceSuccess)
    {
        var result = useCases.RefreshLifecycle();
        if (!result.Succeeded)
        {
            error.WriteLine("Lifecycle refresh failed; retained the last valid offline reference data.");
            return 1;
        }

        if (announceSuccess)
        {
            output.WriteLine("Lifecycle dataset refreshed: " + SafeDisplay(result.Dataset.Version));
        }

        return 0;
    }

    private static void WriteHelp(TextWriter output)
    {
        output.WriteLine("dotnet-binary: static, metadata-only .NET binary inspection");
        output.WriteLine("Usage: dotnet binary <command> [options] (or dotnet-binary <command> [options])");
        output.WriteLine("Commands:");
        output.WriteLine("  inspect <file> [options]                 Exactly one explicit regular file, any extension");
        output.WriteLine("  scan <directory> [<directory> ...] [options]  One or more explicit directories");
        output.WriteLine("  lifecycle refresh                       Explicit online lifecycle reference refresh");
        output.WriteLine("  --help, -h, help                         Show this offline help (also inspect/scan --help)");
        output.WriteLine("  --version                                Show the assembly/package informational version");
        output.WriteLine("Scan switches: --recursive (off by default); --follow-links (off by default)");
        output.WriteLine("Inspect only: --config <file> (explicitly selected configuration file)");
        output.WriteLine("Reports: --format human|json (human default), --details (human only),");
        output.WriteLine("         --fields <comma-separated assembly fields>, --output <new-file>");
        output.WriteLine("Assembly fields: " + string.Join(",", ReportOptions.SupportedFields));
        output.WriteLine("--details conflicts with --fields and --format json; duplicate options are rejected.");
        output.WriteLine("Optional: --sha256 (off by default); --refresh-lifecycle (explicit HTTPS access before inspection)");
        output.WriteLine("Inventory resource limits (defaults; inclusive override ranges):");
        output.WriteLine("  --max-depth            32       0-128");
        output.WriteLine("  --max-candidates       10000    1-100000");
        output.WriteLine("  --max-file-mib         512      1-1024");
        output.WriteLine("  --max-elapsed-seconds  300      1-3600");
        output.WriteLine("  --max-cpu-seconds      300      1-3600");
        output.WriteLine("Paths are literal and may contain spaces (quote in your shell). Use -- before option-like paths.");
        output.WriteLine("Exit codes: 0 completed report (including partial coverage), 2 usage/selected-input error,");
        output.WriteLine("            1 orchestration, refresh, reporting, or output failure.");
        output.WriteLine("Reports use stdout, diagnostics use stderr; --output creates a new file exclusively.");
        output.WriteLine("Safety: never executes or loads inspected binaries; no installed runtime detection.");
        output.WriteLine("Lifecycle assessment is offline by default. Explicit refresh requires network access to");
        output.WriteLine("approved Microsoft HTTPS lifecycle sources and updates a validated local cache.");
        output.WriteLine("Limits are cooperative; inspection is not an OS-isolated hostile-input sandbox.");
    }

    private static string SafeDisplay(string? content)
    {
        if (content is null) return "";
        const int MaxLength = 256;
        var limited = string.Concat(content.Take(MaxLength).Select(character =>
            char.IsControl(character) || character == '\u001b' ? '?' : character));
        return content.Length > MaxLength ? limited + "..." : limited;
    }
}
