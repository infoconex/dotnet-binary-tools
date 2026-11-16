namespace Infoconex.DotNetBinary.Domain.Models;

/// <summary>A discovered, deliberately skipped path with its selected root provenance.</summary>
/// <param name="Root">Validated root under which the path was encountered.</param>
/// <param name="Path">Observed logical path.</param>
/// <param name="Reason">Typed skip reason.</param>
public sealed record InventorySkippedPath(string Root, string Path, InventorySkipReason Reason);
