using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class DeploymentConfigurationReaderTests
{
    private string _directory = null!;
    private readonly DeploymentConfigurationService _service = new(new FileDeploymentConfigurationReader());

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "deployment-configuration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, recursive: true);

    [Test]
    [Property("Scenario", "SC-CFG-005")]
    public void ReadsAllFrameworkRuntimeDeclarationsWithExactValues()
    {
        var executable = Input("MyApp.exe");
        Write("MyApp.exe.config", """
            <?xml version="1.0"?>
            <configuration><startup useLegacyV2RuntimeActivationPolicy="true">
              <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
              <supportedRuntime version="v2.0.50727" />
            </startup></configuration>
            """);

        var result = _service.Inspect(executable);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Parsed));
            Assert.That(result.RuntimeRequests, Has.Count.EqualTo(2));
            Assert.That(result.RuntimeRequests[0].Values.Select(value => value.RawValue),
                Is.EqualTo(new[] { "v4.0", ".NETFramework,Version=v4.8" }));
            Assert.That(result.RuntimeRequests[1].Values.Single().RawValue, Is.EqualTo("v2.0.50727"));
            Assert.That(result.RuntimeRequests[0].Values[1].Field, Is.EqualTo("configuration.startup.supportedRuntime[0].@sku"));
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-006")]
    public void ReadsTfmFrameworkAndRollForwardAsDistinctDeclarationFacts()
    {
        var file = Input("App.dll");
        Write("App.runtimeconfig.json", """
            {"runtimeOptions":{"tfm":"net11.0","rollForward":"LatestMajor","applyPatches":false,
              "framework":{"name":"Microsoft.NETCore.App","version":"11.0.1","rollForward":"Minor"}}}
            """);

        var result = _service.Inspect(file);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Parsed));
            Assert.That(result.Values.Select(value => value.RawValue), Does.Contain("net11.0"));
            Assert.That(result.Values.Select(value => value.RawValue), Does.Contain("LatestMajor"));
            Assert.That(result.Values.Select(value => value.RawValue), Does.Contain("false"));
            Assert.That(result.RuntimeRequests, Has.Count.EqualTo(1));
            Assert.That(result.RuntimeRequests[0].Values.Select(value => value.RawValue),
                Is.EqualTo(new[] { "Microsoft.NETCore.App", "11.0.1", "Minor" }));
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-007")]
    public void PreservesBothFrameworkArrayRequestsAndTheirOrigins()
    {
        var file = Input("App.dll");
        Write("App.runtimeconfig.json", """
            {"runtimeOptions":{"frameworks":[
              {"name":"Microsoft.NETCore.App","version":"11.0.0"},
              {"name":"Microsoft.AspNetCore.App","version":"11.0.2","applyPatches":true}]}}
            """);

        var result = _service.Inspect(file);

        Assert.Multiple(() =>
        {
            Assert.That(result.RuntimeRequests, Has.Count.EqualTo(2));
            Assert.That(result.RuntimeRequests[0].Origin, Is.EqualTo("runtimeOptions.frameworks[0]"));
            Assert.That(result.RuntimeRequests[1].Origin, Is.EqualTo("runtimeOptions.frameworks[1]"));
            Assert.That(result.RuntimeRequests[1].Values[0].RawValue, Is.EqualTo("Microsoft.AspNetCore.App"));
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-009")]
    public void MalformedJsonIsContainedWithoutInventingRuntimeRequests()
    {
        var file = Input("Corrupt.dll");
        Write("Corrupt.runtimeconfig.json", "{" + "\"runtimeOptions\":{");

        var result = _service.Inspect(file);

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Malformed));
            Assert.That(result.RuntimeRequests, Is.Empty);
            Assert.That(result.Diagnostics, Is.Not.Empty);
        });
    }

    [Test]
    [Property("Scenario", "SC-CFG-012")]
    public void PreservesSourceFileAssociationAndExactFieldProvenance()
    {
        var file = Input("Demo.dll");
        var configuration = Write("Custom.runtimeconfig.json", """
            {"runtimeOptions":{"framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}
            """);

        var result = _service.Inspect(file, new SelectedInput(configuration, configuration, InputKind.File));

        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo(configuration));
            Assert.That(result.Association, Is.EqualTo(DeploymentConfigurationAssociation.Explicit));
            Assert.That(result.RuntimeRequests[0].Values[1].Field, Is.EqualTo("runtimeOptions.framework.version"));
            Assert.That(result.RuntimeRequests[0].Values[1].RawValue, Is.EqualTo("8.0.0"));
        });
    }

    [Test]
    public void RejectsDtdAndExcessiveXmlNestingWithoutExternalResolution()
    {
        var file = Input("Legacy.exe");
        Write("Legacy.exe.config", """
            <!DOCTYPE configuration [<!ENTITY remote SYSTEM "file:///etc/passwd">]>
            <configuration><startup><supportedRuntime version="&remote;" /></startup></configuration>
            """);
        var rejectedDtd = _service.Inspect(file);
        Write("Legacy.exe.config", "<configuration>" + new string(' ', 3) +
            string.Concat(Enumerable.Repeat("<nested>", 45)) +
            string.Concat(Enumerable.Repeat("</nested>", 45)) + "</configuration>");
        var rejectedNesting = _service.Inspect(file);

        Assert.Multiple(() =>
        {
            Assert.That(rejectedDtd.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Malformed));
            Assert.That(rejectedDtd.RuntimeRequests, Is.Empty);
            Assert.That(rejectedNesting.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.ResourceLimited));
        });
    }

    [Test]
    public void RejectsOversizedDocumentsBeforeJsonParsingAndDeepJson()
    {
        var file = Input("Large.dll");
        Write("Large.runtimeconfig.json", "{" + new string('x', 300) + "}");
        var oversized = _service.Inspect(file, maxFileBytes: 100);
        Write("Large.runtimeconfig.json", string.Concat(Enumerable.Repeat("{\"inner\":", 40)) +
            "null" + string.Concat(Enumerable.Repeat("}", 40)));
        var deep = _service.Inspect(file);

        Assert.Multiple(() =>
        {
            Assert.That(oversized.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.ResourceLimited));
            Assert.That(deep.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.ResourceLimited));
        });
    }

    [Test]
    public void UnsupportedShapeAndUnreadablePathAreDistinctFromMalformedContent()
    {
        var file = Input("Odd.dll");
        Write("Odd.runtimeconfig.json", """{"runtimeOptions":{"framework":"not-an-object"}}""");
        var unsupported = _service.Inspect(file);
        var missing = _service.Inspect(Input("Missing.dll"));
        var reader = new FileDeploymentConfigurationReader();
        var unreadable = reader.Read(_directory, DeploymentConfigurationAssociation.Explicit, 1000);

        Assert.Multiple(() =>
        {
            Assert.That(unsupported.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Unsupported));
            Assert.That(missing.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.NotFound));
            Assert.That(unreadable.Outcome, Is.EqualTo(DeploymentConfigurationOutcome.Unreadable));
        });
    }

    private SelectedInput Input(string name)
    {
        var path = Path.Combine(_directory, name);
        return new SelectedInput(path, path, InputKind.File);
    }

    private string Write(string name, string content)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, content);
        return path;
    }
}
