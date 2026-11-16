using System.Text;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class LifecycleRefreshServiceTests
{
    private const string ValidIndex = """
        {"releases-index":[
          {"channel-version":"11.0","product":".NET","support-phase":"active", "latest-release-date":"2026-11-10","eol-date":"2028-11-14"},
          {"channel-version":"10.0","product":".NET","support-phase":"active", "latest-release-date":"2026-11-10","eol-date":"2028-11-14"},
          {"channel-version":"8.0","product":".NET","support-phase":"eol", "latest-release-date":"2026-11-10","eol-date":"2026-11-10"},
          {"channel-version":"3.1","product":"Core","support-phase":"eol", "latest-release-date":"2022-12-13","eol-date":"2022-12-13"}
        ]}
        """;

    [Test]
    [Property("Scenario", "SC-LCY-009")]
    public void NormalAssessmentNeverUsesNetworkRefreshPort()
    {
        var store = new FakeLifecycleStore(LifecycleAssessmentServiceTests.Dataset());
        var source = new FakeReleaseSource(ValidIndex);
        _ = new LifecycleAssessmentService(store, new FixedTimeProvider(new DateOnly(2026, 11, 15)))
            .Assess(new TargetFrameworkIdentity(".NETCoreApp,Version=v8.0"), null);
        Assert.That(source.Attempts, Is.Zero);
    }

    [Test]
    [Property("Scenario", "SC-LCY-010")]
    public async Task ExplicitValidRefreshReplacesDatasetAndRetainsFrameworkReference()
    {
        var store = new FakeLifecycleStore(LifecycleAssessmentServiceTests.Dataset());
        var source = new FakeReleaseSource(ValidIndex);
        var service = new LifecycleRefreshService(store, source, new MicrosoftReleaseIndexReader(),
            new FixedTimeProvider(new DateOnly(2026, 11, 15)));

        var result = await service.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.True);
            Assert.That(source.Attempts, Is.EqualTo(1));
            Assert.That(store.Current.Entries.Any(entry => entry.Family == TargetFrameworkFamily.NetCore &&
                entry.Version == "3.1"), Is.True);
            Assert.That(store.Current.Entries.Any(entry => entry.Family == TargetFrameworkFamily.NetFramework &&
                entry.Version == "4.8"), Is.True);
            Assert.That(result.Dataset.RetrievedOn, Is.EqualTo(new DateOnly(2026, 11, 15)));
        });
    }

    [TestCase("{not-json")]
    [TestCase("{\"releases-index\":[]}")]
    [TestCase("{\"releases-index\":[{\"channel-version\":\"11.0\",\"eol-date\":\"not-a-date\"}]}")]
    [TestCase("{\"releases-index\":[{\"channel-version\":\"11.0\",\"product\":\".NET\",\"support-phase\":\"active\",\"latest-release-date\":\"2026-11-10\",\"eol-date\":\"2028-11-14\"},{\"channel-version\":\"11.0\",\"product\":\".NET\",\"support-phase\":\"active\",\"latest-release-date\":\"2026-11-10\",\"eol-date\":\"2028-11-14\"}]}")]
    [Property("Scenario", "SC-LCY-011")]
    public async Task InvalidReferenceDoesNotInstallAnyPartialDataset(string reference)
    {
        var store = new FakeLifecycleStore(LifecycleAssessmentServiceTests.Dataset());
        var original = store.Current;
        var refresh = new LifecycleRefreshService(store, new FakeReleaseSource(reference),
            new MicrosoftReleaseIndexReader(), new FixedTimeProvider(new DateOnly(2026, 11, 15)));

        var result = await refresh.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(store.Current, Is.SameAs(original));
            Assert.That(result.Dataset.Version, Is.EqualTo(original.Version));
            Assert.That(result.Warning, Is.Not.Empty);
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-011")]
    public async Task FailedDownloadKeepsLastVerifiedReferenceData()
    {
        var store = new FakeLifecycleStore(LifecycleAssessmentServiceTests.Dataset());
        var source = new FakeReleaseSource(ValidIndex) { FailDownload = true };
        var refresh = new LifecycleRefreshService(store, source, new MicrosoftReleaseIndexReader(),
            new FixedTimeProvider(new DateOnly(2026, 11, 15)));

        var result = await refresh.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(store.Current.Version, Is.EqualTo("2026-11-10"));
            Assert.That(result.Warning, Does.Contain("retained"));
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-011")]
    public async Task FailedAtomicReplacementDoesNotCorruptCurrentDataset()
    {
        var store = new FakeLifecycleStore(LifecycleAssessmentServiceTests.Dataset()) { FailReplacement = true };
        var original = store.Current;
        var refresh = new LifecycleRefreshService(store, new FakeReleaseSource(ValidIndex),
            new MicrosoftReleaseIndexReader(), new FixedTimeProvider(new DateOnly(2026, 11, 15)));

        var result = await refresh.RefreshAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Succeeded, Is.False);
            Assert.That(store.Current, Is.SameAs(original));
            Assert.That(result.Dataset.Version, Is.EqualTo(original.Version));
        });
    }

    [Test]
    public void ReleaseIndexRejectsFutureDateAndUntrustedProduct()
    {
        var reader = new MicrosoftReleaseIndexReader();
        var altered = ValidIndex.Replace("2028-11-14", "2020-01-01", StringComparison.Ordinal);
        Assert.Throws<InvalidDataException>(() => reader.Read(Encoding.UTF8.GetBytes(altered),
            LifecycleAssessmentServiceTests.Dataset(), new DateOnly(2026, 11, 15)));
        altered = ValidIndex.Replace("\"product\":\".NET\"", "\"product\":\"UNTRUSTED\"", StringComparison.Ordinal);
        Assert.Throws<InvalidDataException>(() => reader.Read(Encoding.UTF8.GetBytes(altered),
            LifecycleAssessmentServiceTests.Dataset(), new DateOnly(2026, 11, 15)));
    }
}

internal sealed class FakeReleaseSource(string json) : ILifecycleReleaseSource
{
    public bool FailDownload { get; set; }

    public int Attempts { get; private set; }

    public Task<byte[]> FetchAsync(CancellationToken cancellationToken = default)
    {
        Attempts++;
        return FailDownload ? Task.FromException<byte[]>(new HttpRequestException("synthetic error")) :
            Task.FromResult(Encoding.UTF8.GetBytes(json));
    }
}
