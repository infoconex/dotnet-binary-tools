namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Advisory published support classification, never a compliance or vulnerability verdict.</summary>
public enum LifecycleStatus
{
    /// <summary>Published support date has not yet been reached, subject to applicable limitations.</summary>
    Supported,
    /// <summary>A published support end date has been reached.</summary>
    OutOfSupport,
    /// <summary>Insufficient applicable source evidence to classify.</summary>
    Unknown,
    /// <summary>The declaration is not a runtime lifecycle identity.</summary>
    NotApplicable,
}
