namespace Infoconex.DotNetBinary.Application.InputSelection;

/// <summary>
/// Resolves and classifies a single explicit filesystem path using host semantics.
/// This port does not expose content inspection or directory enumeration.
/// </summary>
public interface IInputPathProbe
{
    /// <summary>Returns a host-qualified path and its existing kind, or a safe failure reason.</summary>
    /// <param name="path">The literal, unexpanded path to validate.</param>
    /// <returns>A successful path classification or an input-specific failure.</returns>
    InputPathProbeResult Probe(string path);
}
