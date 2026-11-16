namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class FixedTimeProvider(DateOnly date) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(date.Year, date.Month, date.Day, 12, 0, 0, TimeSpan.Zero);
}
