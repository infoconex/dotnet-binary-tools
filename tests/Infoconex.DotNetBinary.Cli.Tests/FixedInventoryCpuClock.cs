using Infoconex.DotNetBinary.Application.InspectionInventory;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class FixedInventoryCpuClock : IInventoryCpuClock
{
    public TimeSpan Used => TimeSpan.Zero;
}
