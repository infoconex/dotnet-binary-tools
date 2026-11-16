namespace Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

/// <summary>Infrastructure seam for testing atomic cache replacement failure.</summary>
public interface IAtomicLifecycleCacheWriter
{
    /// <summary>Stages and atomically moves complete bytes into a cache path.</summary>
    void Replace(string destination, byte[] bytes);
}
