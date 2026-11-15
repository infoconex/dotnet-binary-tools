using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Infrastructure.InputSelection;

namespace Infoconex.DotNetBinary.Cli;

/// <summary>
/// Adapts explicit inspect and scan arguments into input selection while the
/// inspection and inventory capabilities remain unimplemented.
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
            output.WriteLine("Input selection: inspect <file> (one regular file, any extension)");
            output.WriteLine("                 scan <directory> [<directory> ...]");
            output.WriteLine("Status: input selection available; inspection and traversal not implemented yet.");
            return 0;
        }

        if (arguments.Count == 1 && arguments[0] == "--version")
        {
            output.WriteLine(typeof(CliApplication).Assembly.GetName().Version?.ToString() ?? "unknown");
            return 0;
        }

        if (arguments.Count > 0 && arguments[0] is "inspect" or "scan")
        {
            var mode = arguments[0] == "inspect" ? InputSelectionMode.Inspect : InputSelectionMode.Scan;
            var paths = arguments.Skip(1).ToArray();
            var result = new InputSelectionService(new HostFileSystemInputPathProbe()).Select(mode, paths);
            if (!result.IsSuccess)
            {
                foreach (var failure in result.Failures)
                {
                    error.WriteLine(failure.Message);
                }

                return 2;
            }

            // Do not claim successful inspection or a completed inventory until
            // downstream capabilities are implemented and coordinated.
            error.WriteLine("Input selection succeeded; inspection and traversal are not implemented yet.");
            return 2;
        }

        error.WriteLine("The requested command is not implemented yet.");
        return 2;
    }
}
