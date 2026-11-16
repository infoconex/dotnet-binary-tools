namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>Whether all discoverable candidates within the selected scope were assessed.</summary>
public enum InventoryCompletionState
{
    /// <summary>No coverage limitation was encountered.</summary>
    Complete,
    /// <summary>One or more candidates or traversed paths could not be fully assessed.</summary>
    Incomplete,
}
