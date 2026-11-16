using Infoconex.DotNetBinary.Cli;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class DeploymentConfigurationCliTests
{
    [Test]
    [Property("Scenario", "SC-CFG-004")]
    public void RejectsMissingExplicitConfigurationBeforeAnyDiscoveryFallback()
    {
        var folder = Path.Combine(Path.GetTempPath(), "cfg-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var assembly = Path.Combine(folder, "App.dll");
            File.WriteAllBytes(assembly, AssemblyFixtureFactory.Managed());
            File.WriteAllText(Path.Combine(folder, "App.runtimeconfig.json"),
                """{"runtimeOptions":{"tfm":"net11.0"}}""");
            var invalid = Path.Combine(folder, "missing.runtimeconfig.json");
            using var output = new StringWriter();
            using var error = new StringWriter();

            var code = CliApplication.Run(["inspect", assembly, "--config", invalid], output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(2));
                Assert.That(output.ToString(), Is.Empty);
                Assert.That(error.ToString(), Does.Contain("does not exist"));
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    [Property("Scenario", "SC-CFG-011")]
    public void CliDisplaysAssemblyTargetAndConflictingConfiguredRequestSeparately()
    {
        var folder = Path.Combine(Path.GetTempPath(), "cfg-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var assembly = Path.Combine(folder, "Different.dll");
            File.WriteAllBytes(assembly, AssemblyFixtureFactory.Managed(".NETCoreApp,Version=v11.0"));
            File.WriteAllText(Path.Combine(folder, "Different.runtimeconfig.json"), """
                {"runtimeOptions":{"tfm":"net8.0","framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}
                """);
            using var output = new StringWriter();
            using var error = new StringWriter();

            var code = CliApplication.Run(["inspect", assembly], output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero); // CAP-005 inventory is complete; CAP-006/007 remain pending.
                Assert.That(output.ToString(), Does.Contain(".NETCoreApp,Version=v11.0"));
                Assert.That(output.ToString(), Does.Contain("runtimeOptions.framework.version=8.0.0"));
                Assert.That(output.ToString(), Does.Contain("TargetFrameworkAttribute"));
                Assert.That(output.ToString(), Does.Not.Contain("installed"));
                Assert.That(output.ToString(), Does.Not.Contain("running"));
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void ScanCannotReceiveOneGlobalConfigurationOverride()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var code = CliApplication.Run(["scan", ".", "--config", "other.runtimeconfig.json"], output, error);
        Assert.Multiple(() =>
        {
            Assert.That(code, Is.EqualTo(2));
            Assert.That(error.ToString(), Does.Contain("--config"));
        });
    }
}
