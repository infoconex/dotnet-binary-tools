using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>Validates a complete explicit input request before any downstream work.</summary>
public sealed class InputSelectionService(IInputPathProbe pathProbe) : IInputSelectionUseCase
{
    private readonly IInputPathProbe _pathProbe = pathProbe ?? throw new ArgumentNullException(nameof(pathProbe));

    /// <summary>Validates every literal root, rejecting all roots if any fails.</summary>
    /// <param name="mode">The intended operation's input-kind rule.</param>
    /// <param name="requestedPaths">Literal supplied paths, without wildcard expansion.</param>
    /// <returns>A complete input selection or actionable failures; never partial success.</returns>
    public InputSelectionResult Select(InputSelectionMode mode, IReadOnlyList<string> requestedPaths)
    {
        ArgumentNullException.ThrowIfNull(requestedPaths);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        if (mode == InputSelectionMode.Inspect && requestedPaths.Count != 1)
        {
            return InputSelectionResult.Rejected([new(null, InputSelectionFailureReason.InvalidCount,
                "inspect requires exactly one explicit file path.")]);
        }

        if (mode == InputSelectionMode.Scan && requestedPaths.Count == 0)
        {
            return InputSelectionResult.Rejected([new(null, InputSelectionFailureReason.InvalidCount,
                "scan requires one or more explicit directory paths.")]);
        }

        var expectedKind = mode == InputSelectionMode.Inspect ? InputKind.File : InputKind.Directory;
        var valid = new List<SelectedInput>();
        var failures = new List<InputSelectionFailure>();

        foreach (var path in requestedPaths)
        {
            var probe = string.IsNullOrWhiteSpace(path)
                ? InputPathProbeResult.Failed(InputSelectionFailureReason.InvalidPath)
                : _pathProbe.Probe(path);
            if (probe.Failure != InputSelectionFailureReason.None)
            {
                failures.Add(new InputSelectionFailure(path, probe.Failure, DescribeFailure(path, probe.Failure)));
            }
            else if (probe.Kind != expectedKind)
            {
                failures.Add(new InputSelectionFailure(path, InputSelectionFailureReason.WrongKind,
                    $"Input '{DescribePath(path)}' is not a {(expectedKind == InputKind.File ? "regular file" : "directory")} for {mode.ToString().ToLowerInvariant()}."));
            }
            else
            {
                valid.Add(new SelectedInput(path, probe.FullPath!, expectedKind));
            }
        }

        return failures.Count != 0 ? InputSelectionResult.Rejected(failures) : InputSelectionResult.Accepted(valid);
    }

    private static string DescribeFailure(string path, InputSelectionFailureReason reason)
    {
        var description = reason switch
        {
            InputSelectionFailureReason.Missing => "does not exist",
            InputSelectionFailureReason.BrokenLink => "is a broken symbolic link or junction",
            InputSelectionFailureReason.Inaccessible => "cannot be accessed with current permissions",
            InputSelectionFailureReason.InvalidPath => "is not a valid literal filesystem path",
            InputSelectionFailureReason.WrongKind => "is not a regular file or directory",
            _ => "could not be validated due to a filesystem error",
        };
        return $"Input '{DescribePath(path)}' {description}.";
    }

    private static string DescribePath(string? path)
    {
        // Avoid multiline/control-character injection into diagnostic channels.
        return path is null ? "" : string.Concat(path.Select(character => char.IsControl(character) ? '?' : character));
    }
}
