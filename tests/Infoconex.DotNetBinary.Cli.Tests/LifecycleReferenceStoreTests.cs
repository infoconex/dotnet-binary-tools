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

    private static string TemporaryCachePath() => Path.Combine(Path.GetTempPath(),
        "lifecycle-" + Guid.NewGuid().ToString("N") + ".json");
}
