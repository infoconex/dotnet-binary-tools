namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Why a discovered path was not traversed or inspected again.</summary>
public enum InventorySkipReason
{
    /// <summary>Policy does not follow discovered directory links.</summary>
    LinkedDirectory,
    /// <summary>Physical directory already visited (including a possible link cycle).</summary>
    DuplicateDirectory,
    /// <summary>Physical candidate already recorded.</summary>
    DuplicateCandidate,
    /// <summary>Traversal depth would exceed the configured maximum.</summary>
    DepthLimit,
    /// <summary>Candidate count would exceed the configured maximum.</summary>
    CandidateLimit,
    /// <summary>Candidate file is too large to inspect.</summary>
    FileSizeLimit,
    /// <summary>Bound on pending or visited directories reached.</summary>
    DirectoryLimit,
}
