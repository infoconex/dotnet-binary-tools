using Infoconex.DotNetBinary.Cli;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class InspectionInventoryCliTests
{
    [Test]
    public void ValidInspectAcceptsUnusualFileExtensionAndReturnsSharedInventorySummary()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(file, AssemblyFixtureFactory.Managed());
            using var output = new StringWriter();
            using var error = new StringWriter();
            var code = CliApplication.Run(["inspect", file], output, error);
            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero);
                Assert.That(output.ToString(), Does.Contain("Inventory candidates: 1"));
                Assert.That(output.ToString(), Does.Contain("Inventory coverage: Complete"));
                Assert.That(error.ToString(), Does.Not.Contain("not implemented"));
            });
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void ScanIsNonRecursiveByDefaultAndHonorsExplicitRecursiveOption()
    {
        var root = Path.Combine(Path.GetTempPath(), "inv-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        try
        {
            File.WriteAllBytes(Path.Combine(root, "one.dll"), AssemblyFixtureFactory.Managed());
            File.WriteAllBytes(Path.Combine(root, "nested", "two.EXE"), AssemblyFixtureFactory.Managed());
            using var shallow = new StringWriter();
            using var deep = new StringWriter();
            using var errors = new StringWriter();
            var shallowCode = CliApplication.Run(["scan", root], shallow, errors);
            var deepCode = CliApplication.Run(["scan", root, "--recursive"], deep, errors);
            Assert.Multiple(() =>
            {
                Assert.That(shallowCode, Is.Zero);
                Assert.That(deepCode, Is.Zero);
                Assert.That(shallow.ToString(), Does.Contain("Inventory candidates: 1"));
                Assert.That(deep.ToString(), Does.Contain("Inventory candidates: 2"));
                Assert.That(errors.ToString(), Is.Empty);
            });
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void ResourceLimitedScanRemainsSuccessfulAndReportsIncompleteCoverage()
    {
        var root = Path.Combine(Path.GetTempPath(), "inv-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllBytes(Path.Combine(root, "one.dll"), AssemblyFixtureFactory.Managed());
            File.WriteAllBytes(Path.Combine(root, "two.dll"), AssemblyFixtureFactory.Managed());
            using var output = new StringWriter();
            using var error = new StringWriter();
            var code = CliApplication.Run(["scan", root, "--max-candidates", "1"], output, error);
            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero);
                Assert.That(output.ToString(), Does.Contain("Inventory candidates: 1"));
                Assert.That(output.ToString(), Does.Contain("Inventory coverage: Incomplete"));
                Assert.That(output.ToString(), Does.Contain("CandidateLimit"));
            });
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestCase("--max-depth", "129")]
    [TestCase("--max-candidates", "0")]
    [TestCase("--max-file-mib", "1025")]
    [TestCase("--max-elapsed-seconds", "3601")]
    [TestCase("--max-cpu-seconds", "0")]
    public void RejectsOutOfRangeLimitsBeforeInputTraversal(string option, string number)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var code = CliApplication.Run(["scan", ".", option, number], output, error);
        Assert.Multiple(() =>
        {
            Assert.That(code, Is.EqualTo(2));
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Does.Contain(option));
        });
    }
}
