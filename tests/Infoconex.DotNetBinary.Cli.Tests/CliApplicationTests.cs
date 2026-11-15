using Infoconex.DotNetBinary.Cli;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class CliApplicationTests
{
    [TestCase("--help")]
    [TestCase("-h")]
    [TestCase("help")]
    public void HelpClearlyStatesFeatureStatus(string option)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(new[] { option }, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(code, Is.Zero);
            Assert.That(output.ToString(), Does.Contain("not implemented yet"));
            Assert.That(error.ToString(), Is.Empty);
        });
    }

    [Test]
    public void HelpReflectsSingleFileAndMultipleDirectoryInputs()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(new[] { "--help" }, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(code, Is.Zero);
            Assert.That(output.ToString(), Does.Contain("Usage: dotnet binary <command> [options]"));
            Assert.That(output.ToString(), Does.Contain("inspect <file>"));
            Assert.That(output.ToString(), Does.Contain("any extension"));
            Assert.That(output.ToString(), Does.Contain("scan <directory> [<directory> ...]"));
            Assert.That(error.ToString(), Is.Empty);
        });
    }

    [Test]
    public void InspectionCommandMustNotPretendToWork()
    {
        var file = Path.GetTempFileName();
        using var output = new StringWriter();
        using var error = new StringWriter();
        try
        {
            var code = CliApplication.Run(new[] { "inspect", file }, output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(2));
                Assert.That(output.ToString(), Is.Empty);
                Assert.That(error.ToString(), Does.Contain("selection succeeded"));
                Assert.That(error.ToString(), Does.Contain("not implemented"));
            });
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Test]
    public void InvalidScanRootReturnsFailureWithoutReportingACompletedInventory()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var missing = Path.Combine(Path.GetTempPath(), "missing-" + Guid.NewGuid().ToString("N"));

        var code = CliApplication.Run(new[] { "scan", missing }, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(code, Is.EqualTo(2));
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Does.Contain(missing));
            Assert.That(error.ToString(), Does.Contain("does not exist"));
        });
    }

    [Test]
    public void InspectWithMultipleFilesFailsCardinalityBeforeFilesystemAccess()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(new[] { "inspect", "a.dll", "b.dll" }, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(code, Is.EqualTo(2));
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Does.Contain("exactly one"));
        });
    }

    [Test]
    public void VersionReportsAnAssemblyVersion()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(new[] { "--version" }, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(code, Is.Zero);
            Assert.That(output.ToString().Trim(), Is.Not.Empty);
            Assert.That(error.ToString(), Is.Empty);
        });
    }
}
