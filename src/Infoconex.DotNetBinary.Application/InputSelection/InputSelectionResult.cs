using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>
/// Represents a complete selection: either all validated roots or only failures.
/// A failed result never exposes a partial subset of valid inputs.
/// </summary>
public sealed class InputSelectionResult
{
    private InputSelectionResult(IReadOnlyList<SelectedInput> inputs, IReadOnlyList<InputSelectionFailure> failures)
    {
        Inputs = inputs;
        Failures = failures;
    }

    /// <summary>Gets whether every explicitly requested path passed validation.</summary>
    public bool IsSuccess => Failures.Count == 0;

    /// <summary>Gets all validated roots, or an empty collection if validation failed.</summary>
    public IReadOnlyList<SelectedInput> Inputs { get; }

    /// <summary>Gets all input validation failures, or an empty collection on success.</summary>
    public IReadOnlyList<InputSelectionFailure> Failures { get; }

    /// <summary>Creates a successful, immutable selection of validated inputs.</summary>
    public static InputSelectionResult Accepted(IEnumerable<SelectedInput> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        var copy = inputs.ToArray();
        if (copy.Length == 0)
        {
            throw new ArgumentException("A successful input selection requires at least one root.", nameof(inputs));
        }

        return new(Array.AsReadOnly(copy), Array.Empty<InputSelectionFailure>());
    }

    /// <summary>Creates an unsuccessful result without leaking any validated roots.</summary>
    public static InputSelectionResult Rejected(IEnumerable<InputSelectionFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        var copy = failures.ToArray();
        if (copy.Length == 0)
        {
            throw new ArgumentException("A rejected input selection requires a failure.", nameof(failures));
        }

        return new(Array.Empty<SelectedInput>(), Array.AsReadOnly(copy));
    }
}
