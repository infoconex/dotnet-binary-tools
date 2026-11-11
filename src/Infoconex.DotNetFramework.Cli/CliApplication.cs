using System.Reflection;

namespace Infoconex.DotNetFramework.Cli;

/// <summary>
/// Provides the foundational command-line entry point.
/// Inspection and scanning commands are intentionally not implemented yet.
/// </summary>
public static class CliApplication
{
    /// <summary>
    /// Runs the current CLI baseline without modifying any inspected files.
    /// </summary>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="output">Destination for normal output.</param>
    /// <param name="error">Destination for diagnostic output.</param>
    /// <returns>Zero for recognized informational commands; two for unsupported commands.</returns>
    public static int Run(IReadOnlyList<string> arguments, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (arguments.Count == 0 ||
            (arguments.Count == 1 && arguments[0] is "--help" or "-h" or "help"))
        {
            output.WriteLine("dotnet-framework: .NET assembly target-framework inventory");
            output.WriteLine("Usage: dotnet framework <command> [options]");
            output.WriteLine("Planned: inspect <assembly>, scan <directory>");
            output.WriteLine("Status: engineering foundation; inspection commands not implemented yet.");
            return 0;
        }

        if (arguments.Count == 1 && arguments[0] == "--version")
        {
            output.WriteLine(typeof(CliApplication).Assembly.GetName().Version?.ToString() ?? "unknown");
            return 0;
        }

        error.WriteLine("The requested command is not implemented in this engineering foundation.");
        return 2;
    }
}
