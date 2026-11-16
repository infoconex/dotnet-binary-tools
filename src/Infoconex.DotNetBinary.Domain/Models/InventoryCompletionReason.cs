namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Typed and auditable reasons an otherwise valid inventory is incomplete.</summary>
public enum InventoryCompletionReason
{
    /// <summary>A candidate could not be classified reliably.</summary>
    CandidateFailure,
    /// <summary>Direct deployment configuration evidence could not be interpreted reliably.</summary>
    ConfigurationFailure,
    /// <summary>A selected directory could not be fully enumerated.</summary>
    TraversalError,
    /// <summary>A discovered linked directory was omitted by the default policy.</summary>
    LinkedDirectory,
    /// <summary>Recursion would exceed the configured depth.</summary>
    DepthLimit,
    /// <summary>Additional unique candidates exceed the configured count.</summary>
    CandidateLimit,
    /// <summary>A file exceeds the configured maximum file length.</summary>
    FileSizeLimit,
    /// <summary>CAP-002 metadata parsing met its intrinsic resource policy.</summary>
    ParsingLimit,
    /// <summary>Directory traversal reached its fixed work-queue bound.</summary>
    DirectoryLimit,
    /// <summary>The wall-clock traversal budget was reached.</summary>
    ElapsedTimeLimit,
    /// <summary>The observed process CPU budget was reached.</summary>
    CpuTimeLimit,
}
