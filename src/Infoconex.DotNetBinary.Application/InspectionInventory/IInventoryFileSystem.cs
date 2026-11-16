namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Constrained filesystem enumeration, size and physical-identity port.</summary>
public interface IInventoryFileSystem
{
    /// <summary>Host-aware physical path comparison policy.</summary>
    StringComparer PathComparer { get; }

    /// <summary>Resolves a validated, explicitly selected directory, including a linked root.</summary>
    string GetPhysicalDirectoryPath(string directoryPath);

    /// <summary>Resolves a selected file identity, including a direct file link.</summary>
    string GetPhysicalFilePath(string filePath);

    /// <summary>Reads a candidate's length without opening or parsing its contents.</summary>
    long GetFileLength(string filePath);

    /// <summary>Enumerates one directory lazily, without implicit recursion or error suppression.</summary>
    IEnumerable<InventoryDiscoveredPath> EnumerateDirectory(string logicalPath, string physicalPath);
}
