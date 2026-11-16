namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Common inspect/scan coverage state with typed reasons.</summary>
public sealed class InventoryCompletion
{
    /// <summary>Creates completion status from distinct coverage limitations.</summary>
    public InventoryCompletion(IEnumerable<InventoryCompletionReason> reasons)
    {
        ArgumentNullException.ThrowIfNull(reasons);
        Reasons = Array.AsReadOnly(reasons.Distinct().Order().ToArray());
    }

    /// <summary>Gets complete/incomplete coverage, not command success/failure.</summary>
    public InventoryCompletionState State => Reasons.Count == 0
        ? InventoryCompletionState.Complete : InventoryCompletionState.Incomplete;

    /// <summary>Gets every independently observed limitation.</summary>
    public IReadOnlyList<InventoryCompletionReason> Reasons { get; }
}
