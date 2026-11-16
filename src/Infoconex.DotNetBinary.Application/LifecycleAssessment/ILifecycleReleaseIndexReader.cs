using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Port for strictly validating and converting official release metadata.</summary>
public interface ILifecycleReleaseIndexReader
{
    /// <summary>Validates and transforms Microsoft metadata while retaining independently sourced Framework facts.</summary>
    LifecycleReferenceDataset Read(byte[] contents, LifecycleReferenceDataset current, DateOnly retrievedOn);
}
