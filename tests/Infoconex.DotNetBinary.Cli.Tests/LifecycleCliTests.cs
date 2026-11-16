using Infoconex.DotNetBinary.Cli;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class LifecycleCliTests
{
    [Test]
    [Property("Scenario", "SC-LCY-007")]
    public void UnsupportedDeclaredTargetRemainsAdvisoryWhenInspectCapabilityIsIncomplete()
    {
        var directory = Path.Combine(Path.GetTempPath(), "lc-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var file = Path.Combine(directory, "App.dll");
            File.WriteAllBytes(file, AssemblyFixtureFactory.Managed(".NETCoreApp,Version=v8.0"));
            using var output = new StringWriter();
            using var error = new StringWriter();

            var exitCode = CliApplication.Run(["inspect", file], output, error);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(2)); // CAP-005–007 still unavailable, not a lifecycle failure.
                Assert.That(output.ToString(), Does.Contain("Lifecycle"));
                Assert.That(output.ToString(), Does.Contain("OutOfSupport"));
                Assert.That(error.ToString(), Does.Contain("not implemented"));
                Assert.That(output.ToString(), Does.Not.Contain("noncompliant"));
            });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    [Property("Scenario", "SC-LCY-004")]
    public void InspectDisplaysTargetAndRuntimeAdvisoriesSeparately()
    {
        var directory = Path.Combine(Path.GetTempPath(), "lc-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var file = Path.Combine(directory, "App.dll");
            File.WriteAllBytes(file, AssemblyFixtureFactory.Managed(".NETCoreApp,Version=v11.0"));
            File.WriteAllText(Path.Combine(directory, "App.runtimeconfig.json"),
                """{"runtimeOptions":{"framework":{"name":"Microsoft.NETCore.App","version":"8.0.0"}}}""");
            using var output = new StringWriter();
            using var error = new StringWriter();

            var exitCode = CliApplication.Run(["inspect", file], output, error);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(2));
                Assert.That(output.ToString(), Does.Contain("AssemblyTarget"));
                Assert.That(output.ToString(), Does.Contain("ConfiguredRuntime"));
                Assert.That(output.ToString(), Does.Contain("Supported"));
                Assert.That(output.ToString(), Does.Contain("OutOfSupport"));
            });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
