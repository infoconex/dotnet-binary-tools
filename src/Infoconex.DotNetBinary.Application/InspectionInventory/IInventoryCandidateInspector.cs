using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Composes existing single-file capabilities without exposing outer adapters to the orchestrator.</summary>
public interface IInventoryCandidateInspector
{
    /// <summary>Inspects one CAP-001 selected file with an explicit maximum size.</summary>
    InventoryEvidence Inspect(SelectedInput file, SelectedInput? explicitConfiguration, long maxFileBytes);
}
