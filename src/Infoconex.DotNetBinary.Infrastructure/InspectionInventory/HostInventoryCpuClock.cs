using System.Diagnostics;
using Infoconex.DotNetBinary.Application.InspectionInventory;

namespace Infoconex.DotNetBinary.Infrastructure.InspectionInventory;

/// <summary>Provides observed process CPU time for bounded cooperative inventory checks.</summary>
public sealed class HostInventoryCpuClock : IInventoryCpuClock, IDisposable
{
    private readonly Process _process = Process.GetCurrentProcess();

    /// <inheritdoc />
    public TimeSpan Used => _process.TotalProcessorTime;

    /// <inheritdoc />
    public void Dispose() => _process.Dispose();
}
