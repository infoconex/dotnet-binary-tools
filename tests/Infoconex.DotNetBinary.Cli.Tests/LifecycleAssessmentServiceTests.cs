using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class LifecycleAssessmentServiceTests
{
    private static readonly DateOnly ReferenceDate = new(2026, 11, 15);

    [Test]
    [Property("Scenario", "SC-LCY-001")]
    public void UsesBundledDatasetWithoutConsultingNetwork()
    {
        var store = new FakeLifecycleStore(Dataset());
        var result = Service(store).Assess(new TargetFrameworkIdentity(".NETCoreApp,Version=v10.0"), null);

        Assert.Multiple(() =>
        {
            Assert.That(store.LoadCount, Is.EqualTo(1));
            Assert.That(result.Findings, Has.Count.EqualTo(1));
            Assert.That(result.Findings[0].DatasetVersion, Is.EqualTo("2026-11-10"));
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-002")]
    public void LifecycleBoundaryUsesSourceEndDateWithoutGuessing()
    {
        var store = new FakeLifecycleStore(Dataset());
        var target = new TargetFrameworkIdentity(".NETCoreApp,Version=v8.0");

        Assert.Multiple(() =>
        {
            Assert.That(Service(store, new DateOnly(2026, 11, 9)).Assess(target, null).Findings[0].Status,
                Is.EqualTo(LifecycleStatus.Supported));
            Assert.That(Service(store, new DateOnly(2026, 11, 10)).Assess(target, null).Findings[0].Status,
                Is.EqualTo(LifecycleStatus.OutOfSupport));
            Assert.That(Service(store, new DateOnly(2026, 11, 11)).Assess(target, null).Findings[0].Status,
                Is.EqualTo(LifecycleStatus.OutOfSupport));
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-003")]
    public void FrameworkSupportIsQualifiedWhenHostOperatingSystemIsUnknown()
    {
        var finding = Service(new FakeLifecycleStore(Dataset()))
            .Assess(new TargetFrameworkIdentity(".NETFramework,Version=v4.8"), null).Findings.Single();

        Assert.Multiple(() =>
        {
            Assert.That(finding.Status, Is.EqualTo(LifecycleStatus.Unknown));
            Assert.That(finding.Limitation, Does.Contain("Windows"));
            Assert.That(finding.EndOfSupport, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-004")]
    public void DifferingTargetAndConfiguredRuntimeRetainSeparateEvidence()
    {
        var config = Configuration("Microsoft.NETCore.App", "8.0.0");
        var findings = Service(new FakeLifecycleStore(Dataset()))
            .Assess(new TargetFrameworkIdentity(".NETCoreApp,Version=v11.0"), config).Findings;

        Assert.Multiple(() =>
        {
            Assert.That(findings, Has.Count.EqualTo(2));
            Assert.That(findings[0].Status, Is.EqualTo(LifecycleStatus.Supported));
            Assert.That(findings[0].EvidenceKind, Is.EqualTo(LifecycleEvidenceKind.AssemblyTarget));
            Assert.That(findings[1].Status, Is.EqualTo(LifecycleStatus.OutOfSupport));
            Assert.That(findings[1].EvidenceKind, Is.EqualTo(LifecycleEvidenceKind.ConfiguredRuntime));
            Assert.That(findings[1].EvidenceSource, Does.Contain("runtimeOptions.framework"));
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-005")]
    public void NetStandardIsAnApiSpecificationNotRuntimeSupport()
    {
        var finding = Service(new FakeLifecycleStore(Dataset()))
            .Assess(new TargetFrameworkIdentity(".NETStandard,Version=v2.0"), null).Findings.Single();
        Assert.Multiple(() =>
        {
            Assert.That(finding.Status, Is.EqualTo(LifecycleStatus.NotApplicable));
            Assert.That(finding.EndOfSupport, Is.Null);
            Assert.That(finding.Limitation, Does.Contain("API"));
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-006")]
    public void UnrecognizedAndMissingEntriesDoNotInventEndDates()
    {
        var service = Service(new FakeLifecycleStore(Dataset()));
        var unknown = service.Assess(new TargetFrameworkIdentity("Mystery,Version=v99.0"), null).Findings.Single();
        var missing = service.Assess(new TargetFrameworkIdentity(".NETCoreApp,Version=v7.0"), null).Findings.Single();
        Assert.Multiple(() =>
        {
            Assert.That(unknown.Status, Is.EqualTo(LifecycleStatus.Unknown));
            Assert.That(missing.Status, Is.EqualTo(LifecycleStatus.Unknown));
            Assert.That(unknown.EndOfSupport, Is.Null);
            Assert.That(missing.EndOfSupport, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-007")]
    public void EndedSupportIsAnAdvisoryWithoutComplianceOrSecurityVerdict()
    {
        var finding = Service(new FakeLifecycleStore(Dataset()))
            .Assess(new TargetFrameworkIdentity(".NETCoreApp,Version=v8.0"), null).Findings.Single();
        Assert.Multiple(() =>
        {
            Assert.That(finding.Status, Is.EqualTo(LifecycleStatus.OutOfSupport));
            Assert.That(finding.Limitation, Does.Not.Contain("vulnerable"));
            Assert.That(finding.Limitation, Does.Not.Contain("noncompliant"));
        });
    }

    [Test]
    [Property("Scenario", "SC-LCY-008")]
    public void StaleReferenceExposesVersionDatesAndSourceWithoutChangingStatus()
    {
        var date = new DateOnly(2027, 3, 10);
        var finding = Service(new FakeLifecycleStore(Dataset()), date)
            .Assess(new TargetFrameworkIdentity(".NETCoreApp,Version=v10.0"), null).Findings.Single();
        Assert.Multiple(() =>
        {
            Assert.That(finding.IsStale, Is.True);
            Assert.That(finding.DatasetVersion, Is.EqualTo("2026-11-10"));
            Assert.That(finding.SourceRetrievedOn, Is.EqualTo(ReferenceDate));
            Assert.That(finding.SourcePublishedOn, Is.EqualTo(new DateOnly(2026, 11, 10)));
            Assert.That(finding.SourceUrl, Does.StartWith("https://"));
            Assert.That(finding.AssessmentDate, Is.EqualTo(date));
            Assert.That(finding.Status, Is.EqualTo(LifecycleStatus.Supported));
        });
    }

    [Test]
    public void DistinctConfiguredFrameworkRequestsAreNeverCollapsed()
    {
        var first = new DeploymentRuntimeRequest("runtimeOptions.frameworks[0]", [
            new DeploymentConfigurationValue("runtimeOptions.frameworks[0].name", "Microsoft.NETCore.App"),
            new DeploymentConfigurationValue("runtimeOptions.frameworks[0].version", "10.0.0")]);
        var second = new DeploymentRuntimeRequest("runtimeOptions.frameworks[1]", [
            new DeploymentConfigurationValue("runtimeOptions.frameworks[1].name", "Microsoft.AspNetCore.App"),
            new DeploymentConfigurationValue("runtimeOptions.frameworks[1].version", "8.0.0")]);
        var configuration = new DeploymentConfigurationResult(DeploymentConfigurationOutcome.Parsed,
            "app.runtimeconfig.json", DeploymentConfigurationAssociation.Automatic, [first, second], [], []);

        var findings = Service(new FakeLifecycleStore(Dataset())).Assess(null, configuration).Findings;
        Assert.Multiple(() =>
        {
            Assert.That(findings, Has.Count.EqualTo(2));
            Assert.That(findings[0].Status, Is.EqualTo(LifecycleStatus.Supported));
            Assert.That(findings[1].Status, Is.EqualTo(LifecycleStatus.OutOfSupport));
        });
    }

    [Test]
    public void AbsentAssemblyTargetAndConfigurationProduceNoInventedFindings()
    {
        Assert.That(Service(new FakeLifecycleStore(Dataset())).Assess(null, null).Findings, Is.Empty);
    }

    private static LifecycleAssessmentService Service(FakeLifecycleStore store, DateOnly? date = null) =>
        new(store, new FixedTimeProvider(date ?? ReferenceDate));

    internal static LifecycleReferenceDataset Dataset() => new("2026-11-10", ReferenceDate, [
        new LifecycleReferenceEntry(TargetFrameworkFamily.ModernDotNet, "8.0", new DateOnly(2026, 11, 10), false,
            "https://dotnet.microsoft.com/en-us/platform/support/policy", new DateOnly(2026, 11, 10), "Major/minor channel only; patch level and OS support are not assessed."),
        new LifecycleReferenceEntry(TargetFrameworkFamily.ModernDotNet, "10.0", new DateOnly(2028, 11, 14), false,
            "https://dotnet.microsoft.com/en-us/platform/support/policy", new DateOnly(2026, 11, 10), "Major/minor channel only; patch level and OS support are not assessed."),
        new LifecycleReferenceEntry(TargetFrameworkFamily.ModernDotNet, "11.0", new DateOnly(2028, 11, 14), false,
            "https://dotnet.microsoft.com/en-us/platform/support/policy", new DateOnly(2026, 11, 10), "Major/minor channel only; patch level and OS support are not assessed."),
        new LifecycleReferenceEntry(TargetFrameworkFamily.NetFramework, "4.8", null, true,
            "https://learn.microsoft.com/en-us/lifecycle/faq/dotnet-framework", null,
            "Support depends on the underlying Windows operating system lifecycle.")]);

    internal static DeploymentConfigurationResult Configuration(string name, string version)
    {
        var request = new DeploymentRuntimeRequest("runtimeOptions.framework", [
            new DeploymentConfigurationValue("runtimeOptions.framework.name", name),
            new DeploymentConfigurationValue("runtimeOptions.framework.version", version)]);
        return new DeploymentConfigurationResult(DeploymentConfigurationOutcome.Parsed, "app.runtimeconfig.json",
            DeploymentConfigurationAssociation.Automatic, [request], [], []);
    }
}

internal sealed class FakeLifecycleStore(LifecycleReferenceDataset dataset) : ILifecycleDatasetStore
{
    public int LoadCount { get; private set; }

    public LifecycleReferenceDataset Current { get; private set; } = dataset;

    public bool FailReplacement { get; set; }

    public LifecycleDatasetLoad Load()
    {
        LoadCount++;
        return new LifecycleDatasetLoad(Current, false, null);
    }

    public void Replace(LifecycleReferenceDataset replacement)
    {
        if (FailReplacement)
        {
            throw new IOException("synthetic atomic replacement failure");
        }

        Current = replacement;
    }
}

internal sealed class FixedTimeProvider(DateOnly date) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(date.Year, date.Month, date.Day, 12, 0, 0, TimeSpan.Zero);
}
