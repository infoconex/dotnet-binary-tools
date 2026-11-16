using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class DeploymentConfigurationServiceTests
{
    [Test]
    [Property("Scenario", "SC-CFG-001")]
    public void DiscoversExactDllRuntimeconfigWithoutSearchingNeighbors()
    {
        var file = File("/apps/MyApp.dll");
        var reader = new RecordingReader(["/apps/MyApp.runtimeconfig.json"]);

        var result = new DeploymentConfigurationService(reader).Inspect(file);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Parsed));
            Assert.That(result.Path, Is.EqualTo("/apps/MyApp.runtimeconfig.json"));
            Assert.That(result.Association, Is.EqualTo(DeploymentConfigurationAssociation.Automatic));
            Assert.That(reader.ReadPaths, Is.EqualTo(new[] { "/apps/MyApp.runtimeconfig.json" }));
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-002")]
    public void NeverAssociatesAnotherApplicationsRuntimeconfigOrDllHost()
    {
        var reader = new RecordingReader(["/apps/OtherApp.runtimeconfig.json", "/apps/MyApp.exe.config"]);
        var service = new DeploymentConfigurationService(reader);

        var exe = service.Inspect(File("/apps/MyApp.exe"));
        var dll = service.Inspect(File("/apps/Another.dll"));

        Assert.Multiple(() =>
        {
            Assert.That(exe.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Parsed));
            Assert.That(exe.Path, Is.EqualTo("/apps/MyApp.exe.config"));
            Assert.That(dll.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.NotFound));
            Assert.That(reader.ReadPaths, Does.Not.Contain("/apps/OtherApp.runtimeconfig.json"));
            Assert.That(reader.ReadPaths, Does.Not.Contain("/apps/Another.exe.config"));
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-003")]
    public void ExplicitSelectionOverridesDiscoveryAndRecordsItsSource()
    {
        var selected = File("/custom/prod.runtimeconfig.json");
        var reader = new RecordingReader([selected.FullPath]);

        var result = new DeploymentConfigurationService(reader).Inspect(File("/apps/MyApp.dll"), selected);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Parsed));
            Assert.That(result.Path, Is.EqualTo(selected.FullPath));
            Assert.That(result.Association, Is.EqualTo(DeploymentConfigurationAssociation.Explicit));
            Assert.That(reader.ReadPaths, Is.EqualTo(new[] { selected.FullPath }));
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-008")]
    public void MissingConfigurationNeverInventsRuntimeRequests()
    {
        var result = new DeploymentConfigurationService(new RecordingReader([])).Inspect(File("/apps/Library.dll"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.NotFound));
            Assert.That(result.RuntimeRequests, Is.Empty);
            Assert.That(result.Values, Is.Empty);
            Assert.That(result.Path, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-010")]
    public void DoesNotOpenUnrelatedOrUnsupportedConfigurationSources()
    {
        var reader = new RecordingReader(["/apps/web.config", "/apps/MyApp.deps.json"]);
        var service = new DeploymentConfigurationService(reader);

        var result = service.Inspect(File("/apps/MyApp.dll"));
        var unsupported = service.Inspect(File("/apps/MyApp.dll"), File("/apps/web.config"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.NotFound));
            Assert.That(unsupported.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Unsupported));
            Assert.That(reader.ReadPaths, Does.Not.Contain("/apps/web.config"));
            Assert.That(reader.ReadPaths, Does.Not.Contain("/apps/MyApp.deps.json"));
        });
    }

    [Test]
    public void RejectsUnvalidatedDirectoryInputsAndInvalidLimits()
    {
        var reader = new RecordingReader([]);
        var service = new DeploymentConfigurationService(reader);
        Assert.Throws<ArgumentException>(() => service.Inspect(new SelectedInput("/apps", "/apps", InputKind.Directory)));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Inspect(File("/apps/MyApp.dll"), maxFileBytes: 0));
        Assert.That(reader.ReadPaths, Is.Empty);
    }

    private static SelectedInput File(string path) => new(path, path, InputKind.File);

    private sealed class RecordingReader(IEnumerable<string> existing) : IDeploymentConfigurationReader
    {
        private readonly HashSet<string> _existing = new(existing, StringComparer.Ordinal);
        public List<string> ReadPaths { get; } = [];

        public DeploymentConfigurationResult Read(string path, DeploymentConfigurationAssociation association, long maxFileBytes)
        {
            ReadPaths.Add(path);
            return new DeploymentConfigurationResult(
                _existing.Contains(path) ? DeploymentConfigurationOutcome.Parsed : DeploymentConfigurationOutcome.NotFound,
                path, association, [], [], []);
        }
    }
}
