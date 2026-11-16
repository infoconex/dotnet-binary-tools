using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

/// <summary>Offline embedded reference with an optional validated per-user atomic cache.</summary>
public sealed class FileLifecycleDatasetStore : ILifecycleDatasetStore
{
    private const string ResourceName = "Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment.lifecycle-reference.json";
    private readonly string _cachePath;
    private readonly IAtomicLifecycleCacheWriter _writer;

    /// <summary>Constructs the local store; no remote retrieval is possible on this path.</summary>
    /// <param name="cachePath">Optional explicit test cache path; otherwise per-user application data.</param>
    /// <param name="writer">Optional atomic writer for deterministic fault-injection testing.</param>
    public FileLifecycleDatasetStore(string? cachePath = null, IAtomicLifecycleCacheWriter? writer = null)
    {
        _cachePath = cachePath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Infoconex", "DotNetBinary", "lifecycle-reference.json");
        _writer = writer ?? new AtomicLifecycleCacheWriter();
    }

    /// <inheritdoc />
    public LifecycleDatasetLoad Load()
    {
        if (File.Exists(_cachePath))
        {
            try
            {
                return new LifecycleDatasetLoad(LifecycleDatasetCodec.Parse(ReadBoundedCache()), false, null);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
                InvalidDataException or System.Security.SecurityException)
            {
                return new LifecycleDatasetLoad(Bundled(), true,
                    "Local lifecycle cache is unusable; the verified bundled dataset was used instead.");
            }
        }

        return new LifecycleDatasetLoad(Bundled(), false, null);
    }

    private byte[] ReadBoundedCache()
    {
        using var stream = new FileStream(_cachePath, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        if (stream.Length > LifecycleDatasetCodec.MaxDatasetBytes)
        {
            throw new InvalidDataException("Oversized lifecycle reference cache.");
        }

        var bytes = new byte[LifecycleDatasetCodec.MaxDatasetBytes + 1];
        var count = 0;
        while (count < bytes.Length)
        {
            var read = stream.Read(bytes, count, bytes.Length - count);
            if (read == 0)
            {
                break;
            }

            count += read;
        }

        if (count > LifecycleDatasetCodec.MaxDatasetBytes)
        {
            throw new InvalidDataException("Oversized lifecycle reference cache.");
        }

        return bytes[..count];
    }

    /// <inheritdoc />
    public void Replace(LifecycleReferenceDataset replacement)
    {
        var bytes = LifecycleDatasetCodec.Serialize(replacement);
        _writer.Replace(_cachePath, bytes);
    }

    private static LifecycleReferenceDataset Bundled()
    {
        using var resource = typeof(FileLifecycleDatasetStore).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException("Bundled lifecycle reference is missing.");
        using var stream = new MemoryStream();
        resource.CopyTo(stream);
        return LifecycleDatasetCodec.Parse(stream.ToArray());
    }
}
