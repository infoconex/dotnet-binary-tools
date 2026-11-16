using Infoconex.DotNetBinary.Application.InspectionInventory;

namespace Infoconex.DotNetBinary.Cli.Tests;

internal sealed class FakeInventoryFileSystem : IInventoryFileSystem
{
    private readonly Dictionary<string, List<InventoryDiscoveredPath>> _entries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _links = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _sizes = new(StringComparer.Ordinal);

    public HashSet<string> DeniedDirectories { get; } = new(StringComparer.Ordinal);

    public void AddFile(string path, long length)
    {
        var parent = Parent(path);
        EnsureDirectory(parent);
        _sizes[path] = length;
        _entries[parent].Add(new InventoryDiscoveredPath(path, path, IsDirectory: false, IsLink: false, length));
    }

    public void AddDirectoryLink(string path, string target)
    {
        EnsureDirectory(target);
        var parent = Parent(path);
        EnsureDirectory(parent);
        _links[path] = target;
        _entries[parent].Add(new InventoryDiscoveredPath(path, target, IsDirectory: true, IsLink: true, null));
    }

    public string GetPhysicalDirectoryPath(string directoryPath) => _links.GetValueOrDefault(directoryPath, directoryPath);

    public long GetFileLength(string filePath) => _sizes[GetPhysicalFilePath(filePath)];

    public string GetPhysicalFilePath(string filePath) => _links.GetValueOrDefault(filePath, filePath);

    public IEnumerable<InventoryDiscoveredPath> EnumerateDirectory(string logicalPath, string physicalPath)
    {
        if (DeniedDirectories.Contains(physicalPath)) throw new UnauthorizedAccessException();
        if (!_entries.TryGetValue(physicalPath, out var paths)) yield break;
        foreach (var entry in paths)
        {
            var path = logicalPath + "/" + entry.Path.Split('/').Last();
            yield return entry with { Path = path };
        }
    }

    private void EnsureDirectory(string path)
    {
        if (_entries.ContainsKey(path)) return;
        _entries[path] = [];
        if (path == "/") return;
        var parent = Parent(path);
        EnsureDirectory(parent);
        _entries[parent].Add(new InventoryDiscoveredPath(path, path, IsDirectory: true, IsLink: false, null));
    }

    private static string Parent(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash <= 0 ? "/" : path[..slash];
    }
}
