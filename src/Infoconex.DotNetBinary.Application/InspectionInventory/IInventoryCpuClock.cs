namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Read-only host process CPU consumption port for cooperative traversal budgets.</summary>
public interface IInventoryCpuClock
{
    /// <summary>CPU time consumed by the current process.</summary>
    TimeSpan Used { get; }
}
