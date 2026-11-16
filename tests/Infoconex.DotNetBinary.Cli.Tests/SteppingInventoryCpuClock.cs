using Infoconex.DotNetBinary.Application.InspectionInventory;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class SteppingInventoryCpuClock(TimeSpan step) : IInventoryCpuClock
{
    private int _calls;

    public TimeSpan Used => TimeSpan.FromTicks(step.Ticks * _calls++);
}
