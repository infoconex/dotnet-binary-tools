using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class LifecycleReferenceStoreTests
{
    [Test]
    [Property("Scenario", "SC-LCY-001")]
    public void EmbeddedReferenceIncludesKnownMicrosoftLifecycleVersionsOffline()
    {
        var store = new FileLifecycleDatasetStore(TemporaryCachePath());
        var loaded = store.Load();
        Assert.Multiple(() =>
        {
            Assert.That(loaded.Dataset.Version, Is.Not.Empty);
            Assert.That(loaded.Dataset.Entries.Any(entry => entry.Version == "11.0"), Is.True);
            Assert.That(loaded.Dataset.Entries.Any(entry => entry.Version == "3.1"), Is.True);
            Assert.That(loaded.Dataset.Entries.Any(entry => entry.Version == "4.8.1"), Is.True);
            Assert.That(loaded.Warning, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-011")]
    public void InvalidUserCacheFallsBackToBundledDatasetWithoutNetwork()
    {
        var path = TemporaryCachePath();
        File.WriteAllText(path, "corrupt data");
        try
        {
            var result = new FileLifecycleDatasetStore(path).Load();
            Assert.Multiple(() =>
            {
                Assert.That(result.UsedBundledFallback, Is.True);
                Assert.That(result.Warning, Is.Not.Empty);
                Assert.That(result.Dataset.Entries, Is.Not.Empty);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    [Property("Scenario", "SC-LCY-010")]
    public void CacheReplacementIsReadableAndContainsVersionedReference()
    {
        var path = TemporaryCachePath();
        try
        {
            var store = new FileLifecycleDatasetStore(path);
            var current = store.Load().Dataset;
            store.Replace(current with { Version = "2026-11-15" });
            var loaded = new FileLifecycleDatasetStore(path).Load();
            Assert.Multiple(() =>
            {
                Assert.That(loaded.Dataset.Version, Is.EqualTo("2026-11-15"));
                Assert.That(loaded.UsedBundledFallback, Is.False);
            });
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ReplacementFailurePreservesExistingValidOnDiskCache()
    {
        var path = TemporaryCachePath();
        try
        {
            var store = new FileLifecycleDatasetStore(path);
            var original = store.Load().Dataset;
            store.Replace(original with { Version = "2026-11-15" });
            var failing = new FileLifecycleDatasetStore(path, new AlwaysFailingCacheWriter());

            Assert.Throws<IOException>(() => failing.Replace(original));
            Assert.That(new FileLifecycleDatasetStore(path).Load().Dataset.Version, Is.EqualTo("2026-11-15"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ForgedSourceAndDuplicateJsonKeysAreRejectedBeforeUse()
    {
        var bundle = new FileLifecycleDatasetStore(TemporaryCachePath()).Load().Dataset;
        var bytes = LifecycleDatasetCodec.Serialize(bundle);
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        var forged = json.Replace("https://dotnet.microsoft.com/en-us/platform/support/policy",
            "https://evil.example.net/false-support-data", StringComparison.Ordinal);
        var duplicate = json.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 1, \"schemaVersion\": 1",
            StringComparison.Ordinal);

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidDataException>(() =>
                LifecycleDatasetCodec.Parse(System.Text.Encoding.UTF8.GetBytes(forged)));
            Assert.Throws<InvalidDataException>(() =>
                LifecycleDatasetCodec.Parse(System.Text.Encoding.UTF8.GetBytes(duplicate)));
            Assert.Throws<InvalidDataException>(() =>
                LifecycleDatasetCodec.Parse(new byte[LifecycleDatasetCodec.MaxDatasetBytes + 1]));
        });
    }

    private static string TemporaryCachePath() => Path.Combine(Path.GetTempPath(),
        "lifecycle-" + Guid.NewGuid().ToString("N") + ".json");
}
