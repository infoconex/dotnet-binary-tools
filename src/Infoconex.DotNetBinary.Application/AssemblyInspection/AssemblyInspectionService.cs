using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.AssemblyInspection;

/// <summary>Coordinates inspection of one already validated CAP-001 file.</summary>
public sealed class AssemblyInspectionService(IAssemblyMetadataReader metadataReader)
{
    private readonly IAssemblyMetadataReader _metadataReader = metadataReader ?? throw new ArgumentNullException(nameof(metadataReader));

    /// <summary>Inspects one selected file, preserving its separate classification and evidence.</summary>
    /// <param name="input">An individual CAP-001 selected file.</param>
    /// <param name="options">Optional explicit hash and resource policy.</param>
    /// <returns>A classified, evidence-backed result.</returns>
    public AssemblyInspectionResult Inspect(SelectedInput input, AssemblyInspectionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Kind != InputKind.File)
        {
            throw new ArgumentException("Assembly inspection requires a selected file.", nameof(input));
        }

        options ??= new AssemblyInspectionOptions();
        options.Validate();
        return _metadataReader.Read(input, options);
    }
}
