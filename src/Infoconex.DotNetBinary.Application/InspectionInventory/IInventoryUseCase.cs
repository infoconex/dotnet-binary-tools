using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Application boundary for producing inventories from CAP-001 selected inputs.</summary>
public interface IInventoryUseCase
{
    /// <summary>Inspects one prevalidated file.</summary>
    InventoryReport Inspect(SelectedInput file, SelectedInput? explicitConfiguration = null,
        InventoryOptions? options = null);

    /// <summary>Scans one or more prevalidated directories.</summary>
    InventoryReport Scan(IReadOnlyList<SelectedInput> roots, InventoryOptions? options = null);
}
