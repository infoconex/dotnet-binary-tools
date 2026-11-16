namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class SteppingInventoryTimeProvider(TimeSpan step) : TimeProvider
{
    private int _calls;

    public override DateTimeOffset GetUtcNow() =>
        DateTimeOffset.UnixEpoch + TimeSpan.FromTicks(step.Ticks * _calls++);
}
