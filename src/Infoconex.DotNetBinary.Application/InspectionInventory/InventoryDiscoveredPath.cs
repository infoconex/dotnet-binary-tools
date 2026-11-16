namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>One lazily discovered entry with resolved physical identity supplied by the filesystem.</summary>
/// <param name="Path">Logical path under the selected root.</param>
/// <param name="PhysicalPath">Resolved physical path used only for duplicate and cycle control.</param>
/// <param name="IsDirectory">Whether the entry is a directory.</param>
/// <param name="IsLink">Whether it is a symbolic link or junction.</param>
/// <param name="FileLength">Known file length, if already available without a content read.</param>
public sealed record InventoryDiscoveredPath(string Path, string PhysicalPath, bool IsDirectory,
    bool IsLink, long? FileLength);
