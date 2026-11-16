using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class AlwaysFailingCacheWriter : IAtomicLifecycleCacheWriter
{
    public void Replace(string destination, byte[] bytes) => throw new IOException("Injected atomic move failure");
}
