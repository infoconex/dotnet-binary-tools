using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Domain.Tests;

[TestFixture]
[Category("Unit")]
public sealed class DeploymentConfigurationEvidenceTests
{
    [Test]
    public void ConfiguredRuntimeIsNotTheAssemblyTargetOrRuntimeObservation()
    {
        var target = new TargetFrameworkIdentity(".NETCoreApp,Version=v11.0");
        var declaredVersion = new DeploymentConfigurationValue("runtimeOptions.framework.version", "8.0.0");
        var request = new DeploymentRuntimeRequest("runtimeOptions.framework", [declaredVersion]);
        var outcome = new DeploymentConfigurationResult(DeploymentConfigurationOutcome.Parsed,
            "/app/App.runtimeconfig.json", DeploymentConfigurationAssociation.Automatic, [request], [], []);

        Assert.Multiple(() =>
        {
            Assert.That(target.Moniker, Is.EqualTo(".NETCoreApp,Version=v11.0"));
            Assert.That(outcome.RuntimeRequests.Single().Values.Single().RawValue, Is.EqualTo("8.0.0"));
            Assert.That(outcome.RuntimeRequests.Single().Values.Single().Field,
                Is.EqualTo("runtimeOptions.framework.version"));
        });
    }

    [Test]
    public void RejectsMissingFieldOriginsAndNullRuntimeRequestValues()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new DeploymentConfigurationValue("", "8.0.0"));
            Assert.Throws<ArgumentNullException>(() => new DeploymentConfigurationValue("runtimeOptions.tfm", null!));
            Assert.Throws<ArgumentException>(() => new DeploymentRuntimeRequest("", []));
            Assert.Throws<ArgumentNullException>(() => new DeploymentRuntimeRequest("runtimeOptions.framework", null!));
        });
    }
}
