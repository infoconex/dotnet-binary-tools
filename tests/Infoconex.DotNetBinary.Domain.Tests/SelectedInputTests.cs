using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Domain.Tests;

[TestFixture]
[Category("Unit")]
public sealed class SelectedInputTests
{
    [Test]
    public void RetainsOriginalAndHostResolvedPath()
    {
        var input = new SelectedInput("relative/name.bin", "/absolute/name.bin", InputKind.File);

        Assert.Multiple(() =>
        {
            Assert.That(input.RequestedPath, Is.EqualTo("relative/name.bin"));
            Assert.That(input.FullPath, Is.EqualTo("/absolute/name.bin"));
            Assert.That(input.Kind, Is.EqualTo(InputKind.File));
        });
    }

    [TestCase("")]
    [TestCase("  ")]
    public void RejectsBlankResolvedPaths(string path)
    {
        Assert.Throws<ArgumentException>(() => new SelectedInput("name", path, InputKind.File));
    }
}
