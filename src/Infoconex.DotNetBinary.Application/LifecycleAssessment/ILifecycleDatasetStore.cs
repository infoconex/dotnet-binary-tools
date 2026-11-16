using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Port for validated offline reference loading and explicit replacement.</summary>
public interface ILifecycleDatasetStore
{
    /// <summary>Loads verified cache or bundled data without accessing a network.</summary>
    LifecycleDatasetLoad Load();

    /// <summary>Atomically installs validated replacement data, preserving previous data on failure.</summary>
    void Replace(LifecycleReferenceDataset replacement);
}
