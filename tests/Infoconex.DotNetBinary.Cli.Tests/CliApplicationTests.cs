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
        using var output = new StringWriter();
        using var error = new StringWriter();

        var code = CliApplication.Run(new[] { "inspect", "Sample.dll" }, output, error);

        Assert.Multiple(() =>
        {
            Assert.That(code, Is.EqualTo(2));
            Assert.That(output.ToString(), Is.Empty);
            Assert.That(error.ToString(), Does.Contain("not implemented"));
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
