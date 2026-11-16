namespace Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

/// <summary>Uses a same-directory temporary file and atomic OS rename to install validated data.</summary>
public sealed class AtomicLifecycleCacheWriter : IAtomicLifecycleCacheWriter
{
    /// <inheritdoc />
    public void Replace(string destination, byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(bytes);
        var directory = Path.GetDirectoryName(Path.GetFullPath(destination))!;
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, ".lifecycle-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, bufferSize: 4096, FileOptions.WriteThrough))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporary, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
