using System.Collections.Concurrent;
using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Infrastructure.ResultsReporting;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class ReportDestinationTests
{
    [Test]
    public void SC_RPT_009_OutputPathWritesFileWithoutWritingReportToStdout()
    {
        var files = new InMemoryReportFileStore();
        var service = Service(files);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var path = service.Write(ResultsReportingTests.Sample(), "inspect",
            new ReportOptions(ReportFormat.Json, OutputPath: "report.json"), output, error);
        Assert.Multiple(() =>
        {
            Assert.That(path, Is.EqualTo("report.json"));
            Assert.That(files.Contents("report.json"), Does.Contain("\"schemaVersion\""));
            Assert.That(output.ToString(), Is.Empty);
        });
    }

    [Test]
    public void SC_RPT_010_StdoutContainsOnlyJsonAndStderrContainsCoverageWarnings()
    {
        var report = new Infoconex.DotNetBinary.Domain.Models.InventoryReport(
            ResultsReportingTests.Sample().Roots, ResultsReportingTests.Sample().Candidates,
            [], [], [Infoconex.DotNetBinary.Domain.Models.InventoryCompletionReason.CandidateLimit]);
        using var output = new StringWriter();
        using var error = new StringWriter();
        Service(new InMemoryReportFileStore()).Write(report, "inspect",
            new ReportOptions(ReportFormat.Json), output, error);
        using var json = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(json.RootElement.GetProperty("command").GetString(), Is.EqualTo("inspect"));
            Assert.That(error.ToString(), Does.Contain("CandidateLimit"));
            Assert.That(output.ToString(), Does.Not.Contain("Inventory coverage warning:"));
        });
    }

    [Test]
    public void SC_RPT_011_ExistingContentIsPreservedAndTimestampedAlternativeCreated()
    {
        var files = new InMemoryReportFileStore();
        files.Seed("report.json", "do not replace");
        using var output = new StringWriter();
        using var error = new StringWriter();
        var path = Service(files).Write(ResultsReportingTests.Sample(), "inspect",
            new ReportOptions(OutputPath: "report.json"), output, error);
        Assert.Multiple(() =>
        {
            Assert.That(files.Contents("report.json"), Is.EqualTo("do not replace"));
            Assert.That(path, Does.Match(@"^report_19700101T000000000Z_[a-z0-9]+\.json$"));
            Assert.That(files.Contents(path!), Does.Contain("Inventory candidates: 1"));
        });
    }

    [Test]
    public void SC_RPT_012_ConcurrentWritersCannotOverwriteEachOther()
    {
        var files = new InMemoryReportFileStore();
        var service = Service(files);
        var jobs = Enumerable.Range(0, 12).Select(_ => Task.Run(() =>
        {
            using var output = new StringWriter();
            using var error = new StringWriter();
            return service.Write(ResultsReportingTests.Sample(), "inspect",
                new ReportOptions(OutputPath: "same.txt"), output, error);
        })).ToArray();
        Task.WaitAll(jobs);
        Assert.That(jobs.Select(job => job.Result), Is.Unique);
        Assert.That(files.Count, Is.EqualTo(12));
    }

    [Test]
    public void SC_RPT_013_JsonRendersManyItemsInSmallWriterChunks()
    {
        var candidate = ResultsReportingTests.Sample().Candidates[0];
        var inventory = new Infoconex.DotNetBinary.Domain.Models.InventoryReport(
            ResultsReportingTests.Sample().Roots, Enumerable.Repeat(candidate, 12000), [], [], []);
        using var output = new CountingTextWriter();
        new JsonInventoryReportRenderer().Render(inventory, "scan", DateTimeOffset.UnixEpoch,
            new ReportOptions(ReportFormat.Json), output);
        Assert.Multiple(() =>
        {
            Assert.That(output.WriteCount, Is.GreaterThan(100));
            Assert.That(output.MaxWriteLength, Is.LessThanOrEqualTo(4096));
            Assert.That(output.CharacterCount, Is.GreaterThan(500_000));
        });
    }

    [Test]
    public void SC_RPT_014_WriteFailurePropagatesWithoutClaimingSuccess()
    {
        using var output = new ThrowingTextWriter();
        using var error = new StringWriter();
        Assert.Throws<IOException>(() => Service(new InMemoryReportFileStore()).Write(
            ResultsReportingTests.Sample(), "inspect", new ReportOptions(ReportFormat.Json), output, error));
        Assert.That(error.ToString(), Does.Not.Contain("success"));
    }

    [Test]
    public void FileCreationPermissionFailureIsNotTreatedAsAnExistingFileCollision()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        Assert.Throws<UnauthorizedAccessException>(() => Service(new DeniedReportFileStore()).Write(
            ResultsReportingTests.Sample(), "inspect", new ReportOptions(OutputPath: "denied"), output, error));
    }

    private static ReportService Service(IReportFileStore files) => new(
        new InventoryReportRenderer(), files, new FixedTimeProvider(), () => Guid.NewGuid().ToString("N"));

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch;
    }

    private sealed class InMemoryReportFileStore : IReportFileStore
    {
        private readonly ConcurrentDictionary<string, StringWriter> _files = new(StringComparer.Ordinal);

        public int Count => _files.Count;
        public void Seed(string path, string contents)
        {
            var writer = new StringWriter();
            writer.Write(contents);
            Assert.That(_files.TryAdd(path, writer), Is.True);
        }

        public string Contents(string path) => _files[path].ToString();
        public bool Exists(string path) => _files.ContainsKey(path);
        public TextWriter CreateNew(string path)
        {
            var writer = new StringWriter();
            if (!_files.TryAdd(path, writer)) throw new IOException("Already exists");
            return writer;
        }
    }

    private sealed class DeniedReportFileStore : IReportFileStore
    {
        public bool Exists(string path) => false;
        public TextWriter CreateNew(string path) => throw new UnauthorizedAccessException();
    }

    private sealed class CountingTextWriter : TextWriter
    {
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
        public int WriteCount { get; private set; }
        public int MaxWriteLength { get; private set; }
        public long CharacterCount { get; private set; }
        public override void Write(ReadOnlySpan<char> buffer)
        {
            WriteCount++;
            MaxWriteLength = Math.Max(MaxWriteLength, buffer.Length);
            CharacterCount += buffer.Length;
        }
        public override void Write(char value) => Write(stackalloc char[] { value });
    }

    private sealed class ThrowingTextWriter : TextWriter
    {
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
        public override void Write(ReadOnlySpan<char> buffer) => throw new IOException("simulated output failure");
        public override void Write(char value) => throw new IOException("simulated output failure");
    }
}
