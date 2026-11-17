using System.Globalization;
using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Application.ResultsReporting;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>Parses the small, explicitly versioned command grammar without external I/O.</summary>
internal static class CliArgumentParser
{
    internal static bool TryParse(IReadOnlyList<string> arguments, out CliInvocation? invocation, out string error)
    {
        invocation = null;
        error = "";
        if (arguments.Count == 0 || (arguments.Count == 1 && arguments[0] is "--help" or "-h" or "help"))
        {
            invocation = new CliInvocation("help", [], new InventoryOptions(), new ReportOptions());
            return true;
        }

        if (arguments.Count == 1 && arguments[0] == "--version")
        {
            invocation = new CliInvocation("version", [], new InventoryOptions(), new ReportOptions());
            return true;
        }

        if (arguments.Count == 2 && arguments[0] == "lifecycle" && arguments[1] == "refresh")
        {
            invocation = new CliInvocation("refresh", [], new InventoryOptions(), new ReportOptions());
            return true;
        }

        if (arguments[0] is not ("inspect" or "scan"))
        {
            error = "Unknown command. Use --help for supported commands.";
            return false;
        }

        var command = arguments[0];
        if (arguments.Count == 2 && arguments[1] is "--help" or "-h")
        {
            invocation = new CliInvocation("help", [], new InventoryOptions(), new ReportOptions());
            return true;
        }

        var inventory = new InventoryOptions();
        var reporting = new ReportOptions();
        var paths = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        string? configuration = null;
        var refresh = false;
        var positionalOnly = false;
        for (var index = 1; index < arguments.Count; index++)
        {
            var token = arguments[index];
            if (positionalOnly)
            {
                paths.Add(token);
                continue;
            }

            if (token == "--")
            {
                positionalOnly = true;
                continue;
            }

            if (!token.StartsWith("-", StringComparison.Ordinal))
            {
                paths.Add(token);
                continue;
            }

            if (!IsKnownOption(token))
            {
                error = "Unknown option. Use --help for supported options, or -- before an option-like path.";
                return false;
            }

            if (!seen.Add(token))
            {
                error = "Duplicate option is not supported: " + token;
                return false;
            }

            if (token is "--recursive" or "--follow-links")
            {
                if (command != "scan")
                {
                    error = token + " is supported only for scan.";
                    return false;
                }

                inventory = token == "--recursive" ? inventory with { Recursive = true } :
                    inventory with { FollowLinks = true };
                continue;
            }

            if (token is "--details" or "--sha256" or "--refresh-lifecycle")
            {
                if (token == "--details") reporting = reporting with { Details = true };
                if (token == "--sha256") inventory = inventory with { ComputeSha256 = true };
                if (token == "--refresh-lifecycle") refresh = true;
                continue;
            }

            if (token == "--config" && command != "inspect")
            {
                error = "--config is supported only for inspect.";
                return false;
            }

            if (index + 1 == arguments.Count || arguments[index + 1].StartsWith("--", StringComparison.Ordinal) ||
                (arguments[index + 1].StartsWith("-", StringComparison.Ordinal) && !IsNumericOption(token)))
            {
                error = token + " requires a value.";
                return false;
            }

            var value = arguments[++index];
            switch (token)
            {
                case "--config":
                    configuration = value;
                    break;
                case "--format":
                    if (value is not ("human" or "json"))
                    {
                        error = "--format must be human or json.";
                        return false;
                    }

                    reporting = reporting with { Format = value == "json" ? ReportFormat.Json : ReportFormat.Human };
                    break;
                case "--fields":
                    reporting = reporting with { Fields = value.Split(',') };
                    break;
                case "--output":
                    reporting = reporting with { OutputPath = value };
                    break;
                default:
                    if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
                    {
                        error = token + " requires a nonnegative integer value.";
                        return false;
                    }

                    inventory = token switch
                    {
                        "--max-depth" => inventory with { MaxDepth = number },
                        "--max-candidates" => inventory with { MaxCandidates = number },
                        "--max-file-mib" => inventory with { MaxFileBytes = (long)number * 1024 * 1024 },
                        "--max-elapsed-seconds" => inventory with { MaxElapsedSeconds = number },
                        _ => inventory with { MaxCpuSeconds = number },
                    };
                    break;
            }
        }

        if (command == "inspect" && paths.Count != 1)
        {
            error = "inspect requires exactly one explicit file path.";
            return false;
        }

        if (command == "scan" && paths.Count == 0)
        {
            error = "scan requires one or more explicit directory paths.";
            return false;
        }

        try
        {
            inventory.Validate();
            reporting.Validate();
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
            error = "Invalid " + option + " value; see --help for supported ranges.";
            return false;
        }
        catch (ArgumentException exception)
        {
            error = exception.Message;
            return false;
        }

        invocation = new CliInvocation(command, paths, inventory, reporting, configuration, refresh);
        return true;
    }

    private static bool IsNumericOption(string token) => token is "--max-depth" or "--max-candidates" or
        "--max-file-mib" or "--max-elapsed-seconds" or "--max-cpu-seconds";

    private static bool IsKnownOption(string token) => token is "--recursive" or "--follow-links" or
        "--config" or "--format" or "--details" or "--fields" or "--output" or "--max-depth" or
        "--max-candidates" or "--max-file-mib" or "--max-elapsed-seconds" or "--max-cpu-seconds" or
        "--sha256" or "--refresh-lifecycle";
}
