using Infoconex.DotNetFramework.Core.Models;
using NUnit.Framework;

namespace Infoconex.DotNetFramework.Core.Tests;

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

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void MissingMonikerIsRejected(string? moniker)
    {
        Assert.Throws<ArgumentException>(() => new TargetFrameworkIdentity(moniker!));
    }
}
