using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Domain.Tests;

[TestFixture]
[Category("Unit")]
public sealed class TargetFrameworkFamilyTests
{
    [TestCase(".NETFramework,Version=v4.8", TargetFrameworkFamily.NetFramework)]
    [TestCase(".NETCoreApp,Version=v3.1", TargetFrameworkFamily.NetCore)]
    [TestCase(".NETCoreApp,Version=v8.0", TargetFrameworkFamily.ModernDotNet)]
    [TestCase(".NETStandard,Version=v2.1", TargetFrameworkFamily.NetStandard)]
    [TestCase(".NETCoreApp,Version=v99.0", TargetFrameworkFamily.Unrecognized)]
    [TestCase("CompletelyUnknown,Version=v1.0", TargetFrameworkFamily.Unrecognized)]
    public void OnlyRecognizesDocumentedDeclaredFamilies(string moniker, TargetFrameworkFamily expected)
    {
        var identity = new TargetFrameworkIdentity(moniker);
        Assert.That(identity.Family, Is.EqualTo(expected));
    }
}
