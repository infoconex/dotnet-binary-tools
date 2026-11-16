using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.AssemblyInspection;

/// <summary>Outbound port for static PE/CLI and file inspection.</summary>
public interface IAssemblyMetadataReader
{
    /// <summary>Reads one selected file as untrusted data, without executing it.</summary>
    /// <param name="input">A CAP-001 validated individual file.</param>
    /// <param name="options">Bounded inspection options.</param>
    /// <returns>Classified evidence and any safe diagnostics.</returns>
    AssemblyInspectionResult Read(SelectedInput input, AssemblyInspectionOptions options);
}
