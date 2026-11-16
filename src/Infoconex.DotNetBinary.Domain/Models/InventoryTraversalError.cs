namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>A safe traversal error that never includes exception messages or file content.</summary>
/// <param name="Root">Validated root to which the error belongs.</param>
/// <param name="Path">Directory which could not be fully enumerated.</param>
/// <param name="Code">A stable, non-sensitive error category.</param>
public sealed record InventoryTraversalError(string Root, string Path, string Code);
