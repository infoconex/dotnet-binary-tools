using System.Security;
using Infoconex.DotNetBinary.Application.InspectionInventory;

namespace Infoconex.DotNetBinary.Infrastructure.InspectionInventory;

/// <summary>Lazily enumerates one host directory and resolves physical links without reading file contents.</summary>
public sealed class HostInventoryFileSystem : IInventoryFileSystem
{
    /// <inheritdoc />
    public StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    /// <inheritdoc />
    public string GetPhysicalDirectoryPath(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        var absolute = Path.GetFullPath(directoryPath);
        var root = Path.GetPathRoot(absolute) ?? throw new ArgumentException("Directory must be rooted.", nameof(directoryPath));
        var remainder = absolute[root.Length..];
        var canonical = root;
        foreach (var segment in remainder.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries))
        {
            canonical = Path.Combine(canonical, segment);
            canonical = Directory.ResolveLinkTarget(canonical, returnFinalTarget: true)?.FullName ?? canonical;
        }

        return Path.GetFullPath(canonical);
    }

    /// <inheritdoc />
    public string GetPhysicalFilePath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var fullPath = Path.GetFullPath(filePath);
        var parent = Path.GetDirectoryName(fullPath) ?? throw new ArgumentException("File must have a parent directory.", nameof(filePath));
        var canonical = Path.Combine(GetPhysicalDirectoryPath(parent), Path.GetFileName(fullPath));
        return Path.GetFullPath(File.ResolveLinkTarget(canonical, returnFinalTarget: true)?.FullName ?? canonical);
    }

    /// <inheritdoc />
    public long GetFileLength(string filePath) => new FileInfo(filePath).Length;

    /// <inheritdoc />
    public IEnumerable<InventoryDiscoveredPath> EnumerateDirectory(string logicalPath, string physicalPath)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = false,
            IgnoreInaccessible = false,
            AttributesToSkip = 0,
            ReturnSpecialDirectories = false,
        };
        foreach (var entry in new DirectoryInfo(physicalPath).EnumerateFileSystemInfos("*", options))
        {
            var logical = Path.Combine(logicalPath, entry.Name);
            FileAttributes attributes;
            try
            {
                attributes = entry.Attributes;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
            {
                // A disappearing entry is still surfaced as a candidate rather than silently ignored.
                yield return new InventoryDiscoveredPath(logical, entry.FullName, false, false, null);
                continue;
            }

            var linked = (attributes & FileAttributes.ReparsePoint) != 0;
            var directory = (attributes & FileAttributes.Directory) != 0 || Directory.Exists(entry.FullName);
            string physical;
            try
            {
                physical = directory ? GetPhysicalDirectoryPath(entry.FullName) : GetPhysicalFilePath(entry.FullName);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
            {
                // Preserve the entry for a later, isolated failure rather than abandoning its siblings.
                physical = entry.FullName;
            }

            yield return new InventoryDiscoveredPath(logical, physical, directory, linked, null);
        }
    }
}
