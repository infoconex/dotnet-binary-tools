using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Domain.Tests;

[TestFixture]
[Category("Unit")]
public sealed class TargetFrameworkIdentityTests
{
    [Test]
    public void DeclaredMonikerIsPreservedWithoutGuessingOrNormalization()
    {
        const string Moniker = ".NETFramework,Version=v4.8";

        var identity = new TargetFrameworkIdentity(Moniker);

        Assert.That(identity.Moniker, Is.EqualTo(Moniker));
    }

    [Test]
    public void NullMonikerThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TargetFrameworkIdentity(null!));
    }

    [TestCase("")]
    [TestCase("  ")]
    public void BlankMonikerThrowsArgumentException(string moniker)
    {
        Assert.Throws<ArgumentException>(() => new TargetFrameworkIdentity(moniker));
    }

    [TestCase(".NETFramework,Version=v4.5.1")]
    [TestCase(".NETFramework,Version=v4.5.2")]
    [TestCase(".NETFramework,Version=v4.6.1")]
    [TestCase(".NETFramework,Version=v4.6.2")]
    [TestCase(".NETFramework,Version=v4.7.1")]
    [TestCase(".NETFramework,Version=v4.7.2")]
    [TestCase(".NETFramework,Version=v4.8.1")]
    public void RecognizesPublishedFrameworkServicingVersionsForLifecycleAssessment(string moniker)
    {
        var identity = new TargetFrameworkIdentity(moniker);
        Assert.Multiple(() =>
        {
            Assert.That(identity.Family, Is.EqualTo(TargetFrameworkFamily.NetFramework));
            Assert.That(identity.Version, Is.Not.Null);
            Assert.That(identity.Moniker, Is.EqualTo(moniker));
        });
    }
}
