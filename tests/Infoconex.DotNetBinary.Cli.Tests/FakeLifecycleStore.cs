using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class FakeLifecycleStore(LifecycleReferenceDataset dataset) : ILifecycleDatasetStore
{
    public int LoadCount { get; private set; }

    public LifecycleReferenceDataset Current { get; private set; } = dataset;

    public bool FailReplacement { get; set; }

    public LifecycleDatasetLoad Load()
    {
        LoadCount++;
        return new LifecycleDatasetLoad(Current, false, null);
    }

    public void Replace(LifecycleReferenceDataset replacement)
    {
        if (FailReplacement)
        {
            throw new IOException("synthetic atomic replacement failure");
        }

        Current = replacement;
    }
}
