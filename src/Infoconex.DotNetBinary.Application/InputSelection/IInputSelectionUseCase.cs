namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>Application boundary for validating one complete explicit input selection.</summary>
public interface IInputSelectionUseCase
{
    /// <summary>Returns either all validated inputs or the complete set of validation failures.</summary>
    InputSelectionResult Select(InputSelectionMode mode, IReadOnlyList<string> requestedPaths);
}
