using System.Text.Json;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class ResultsReportingCliTests
{
    [Test]
    public void InspectAndScanShareJsonContractWithActualCandidateInspection()
    {
        var folder = NewDirectory();
        try
        {
            var path = Path.Combine(folder, "sample.dll");
            File.WriteAllBytes(path, AssemblyFixtureFactory.Managed());
            using var inspect = new StringWriter();
            using var scan = new StringWriter();
            using var errors = new StringWriter();
            Assert.That(CliApplication.Run(["inspect", path, "--format", "json"], inspect, errors), Is.Zero);
            Assert.That(CliApplication.Run(["scan", folder, "--format", "json"], scan, errors), Is.Zero);
            using var a = JsonDocument.Parse(inspect.ToString());
            using var b = JsonDocument.Parse(scan.ToString());
            Assert.Multiple(() =>
            {
                Assert.That(a.RootElement.GetProperty("schemaVersion").GetString(), Is.EqualTo("1.0.0"));
                Assert.That(b.RootElement.GetProperty("schemaVersion").GetString(), Is.EqualTo("1.0.0"));
                Assert.That(a.RootElement.GetProperty("candidates").GetArrayLength(), Is.EqualTo(1));
                Assert.That(b.RootElement.GetProperty("candidates").GetArrayLength(), Is.EqualTo(1));
                Assert.That(a.RootElement.GetProperty("candidates")[0].GetProperty("assembly")
                    .GetProperty("targetFramework").ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined));
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void ValidationRejectsConflictingProjectionWithoutCreatingOutput()
    {
        var folder = NewDirectory();
        try
        {
            var path = Path.Combine(folder, "output.json");
            using var output = new StringWriter();
            using var errors = new StringWriter();
            var code = CliApplication.Run(["inspect", "missing.dll", "--details", "--fields", "name",
                "--output", path], output, errors);
            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(2));
                Assert.That(errors.ToString(), Does.Contain("--details and --fields"));
                Assert.That(File.Exists(path), Is.False);
                Assert.That(output.ToString(), Is.Empty);
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void FileOutputIsExclusiveAndNeverPollutesStdout()
    {
        var folder = NewDirectory();
        try
        {
            var path = Path.Combine(folder, "candidate.dll");
            var reportPath = Path.Combine(folder, "report.json");
            File.WriteAllBytes(path, AssemblyFixtureFactory.Managed());
            File.WriteAllText(reportPath, "Original content");
            using var output = new StringWriter();
            using var errors = new StringWriter();
            Assert.That(CliApplication.Run(["inspect", path, "--format", "json", "--output", reportPath],
                output, errors), Is.Zero);
            var reports = Directory.GetFiles(folder, "report*.json");
            Assert.Multiple(() =>
            {
                Assert.That(File.ReadAllText(reportPath), Is.EqualTo("Original content"));
                Assert.That(reports.Length, Is.EqualTo(2));
                Assert.That(output.ToString(), Is.Empty);
            });
            using var json = JsonDocument.Parse(File.ReadAllText(reports.Single(file => file != reportPath)));
            Assert.That(json.RootElement.GetProperty("command").GetString(), Is.EqualTo("inspect"));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void IncompleteJsonInventoryKeepsSuccessfulCodeAndWarningsOnStderr()
    {
        var folder = NewDirectory();
        try
        {
            File.WriteAllBytes(Path.Combine(folder, "one.dll"), AssemblyFixtureFactory.Managed());
            File.WriteAllBytes(Path.Combine(folder, "two.dll"), AssemblyFixtureFactory.Managed());
            using var output = new StringWriter();
            using var errors = new StringWriter();
            var code = CliApplication.Run(["scan", folder, "--max-candidates", "1", "--format", "json"],
                output, errors);
            using var json = JsonDocument.Parse(output.ToString());
            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero);
                Assert.That(json.RootElement.GetProperty("completion").GetProperty("state").GetString(), Is.EqualTo("Incomplete"));
                Assert.That(errors.ToString(), Does.Contain("CandidateLimit"));
                Assert.That(output.ToString(), Does.Not.Contain("Inventory coverage warning:"));
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void UnwritableOutputFailsWithoutRawExceptionDetails()
    {
        var folder = NewDirectory();
        try
        {
            var path = Path.Combine(folder, "sample.dll");
            File.WriteAllBytes(path, AssemblyFixtureFactory.Managed());
            using var output = new StringWriter();
            using var errors = new StringWriter();
            var code = CliApplication.Run(["inspect", path, "--format", "json", "--output", folder],
                output, errors);
            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(2));
                Assert.That(errors.ToString(), Does.Contain("Report write or serialization failed"));
                Assert.That(errors.ToString(), Does.Not.Contain(folder));
                Assert.That(output.ToString(), Is.Empty);
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static string NewDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "cap006-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
