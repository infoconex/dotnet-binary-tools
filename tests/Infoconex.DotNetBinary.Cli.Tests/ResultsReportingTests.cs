using System.Text.Json;
using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.ResultsReporting;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class ResultsReportingTests
{
    [Test]
    public void SC_RPT_001_InspectAndScanShareJsonEnvelopeAndSchema()
    {
        using var inspect = RenderJson("inspect", Sample());
        using var scan = RenderJson("scan", Sample());
        Assert.That(inspect.RootElement.GetProperty("schemaVersion").GetString(), Is.EqualTo("1.0.0"));
        Assert.That(scan.RootElement.EnumerateObject().Select(item => item.Name),
            Is.EqualTo(inspect.RootElement.EnumerateObject().Select(item => item.Name)));
        Assert.That(scan.RootElement.GetProperty("command").GetString(), Is.EqualTo("scan"));
    }

    [Test]
    public void SC_RPT_002_ConciseHumanReportOmitsOptionalMetadata()
    {
        var human = RenderHuman(Sample());
        Assert.Multiple(() =>
        {
            Assert.That(human, Does.Contain("Inventory candidates: 1"));
            Assert.That(human, Does.Contain("sample.dll"));
            Assert.That(human, Does.Not.Contain("Example Company"));
        });
    }

    [Test]
    public void SC_RPT_003_DetailsExpandEvidenceWithoutChangingInventory()
    {
        var inventory = Sample();
        var concise = RenderHuman(inventory);
        var detailed = RenderHuman(inventory, new ReportOptions(Details: true));
        Assert.Multiple(() =>
        {
            Assert.That(detailed, Does.Contain("Example Company"));
            Assert.That(detailed, Does.Contain("FileVersion"));
            Assert.That(detailed, Does.Contain("Lifecycle"));
            Assert.That(detailed, Does.Contain("Inventory candidates: 1"));
            Assert.That(concise, Does.Not.Contain("Example Company"));
            Assert.That(inventory.Summary.TotalCandidates, Is.EqualTo(1));
        });
    }

    [Test]
    public void SC_RPT_004_JsonEscapesInputAndEmitsOneDocumentWithoutDiagnosticProse()
    {
        using var result = RenderJson("scan", Sample(path: "line\n\"quote.dll"));
        Assert.That(result.RootElement.GetProperty("candidates")[0].GetProperty("path").GetString(),
            Is.EqualTo("line\n\"quote.dll"));
        Assert.That(result.RootElement.GetProperty("summary").GetProperty("totalCandidates").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public void SC_RPT_005_JsonProjectionPreservesMandatoryEnvelopeAndEvidence()
    {
        var report = Sample();
        using var full = RenderJson("inspect", report);
        using var projected = RenderJson("inspect", report, new ReportOptions(ReportFormat.Json,
            Fields: ["name", "version", "targetFramework"]));
        var candidate = projected.RootElement.GetProperty("candidates")[0];
        Assert.Multiple(() =>
        {
            Assert.That(candidate.GetProperty("assembly").EnumerateObject().Select(field => field.Name),
                Is.EquivalentTo(new[] { "name", "version", "targetFramework" }));
            Assert.That(candidate.GetProperty("outcome").GetString(), Is.EqualTo("ManagedAssembly"));
            Assert.That(candidate.TryGetProperty("configuration", out _), Is.True);
            Assert.That(candidate.TryGetProperty("lifecycle", out _), Is.True);
            Assert.That(projected.RootElement.GetProperty("summary").GetRawText(),
                Is.EqualTo(full.RootElement.GetProperty("summary").GetRawText()));
            Assert.That(projected.RootElement.GetProperty("completion").GetRawText(),
                Is.EqualTo(full.RootElement.GetProperty("completion").GetRawText()));
        });
    }

    [Test]
    public void SC_RPT_006_HumanProjectionOmitsUnselectedAssemblyFields()
    {
        var projected = RenderHuman(Sample(), new ReportOptions(Fields: ["name", "version"]));
        Assert.Multiple(() =>
        {
            Assert.That(projected, Does.Contain("sample"));
            Assert.That(projected, Does.Contain("1.2.3.4"));
            Assert.That(projected, Does.Not.Contain("Example Company"));
            Assert.That(projected, Does.Not.Contain(".NETCoreApp,Version=v8.0"));
        });
    }

    [Test]
    public void SC_RPT_007_ConflictingAndUnsupportedOptionsAreRejectedBeforeRendering()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new ReportOptions(Details: true, Fields: ["name"]).Validate());
            Assert.Throws<ArgumentException>(() => new ReportOptions(Fields: ["unapproved"]).Validate());
            Assert.Throws<ArgumentException>(() => new ReportOptions(Fields: ["name", "name"]).Validate());
        });
    }

    [Test]
    public void SC_RPT_008_UnknownNativeAndFailedItemsAndIncompleteCoverageAreNotLost()
    {
        var root = new SelectedInput("root", "root", InputKind.Directory);
        var native = new InventoryCandidate(new SelectedInput("native.dll", "native.dll", InputKind.File),
            new InventoryEvidence(new AssemblyInspectionResult(AssemblyInspectionOutcome.NativeBinary, null, null,
                null, []), null, null));
        var unreadable = new InventoryCandidate(new SelectedInput("bad.dll", "bad.dll", InputKind.File),
            new InventoryEvidence(new AssemblyInspectionResult(AssemblyInspectionOutcome.Unreadable, null, null,
                null, [new InspectionDiagnostic("CandidateReadFailure", "File")]), null, null));
        var inventory = new InventoryReport([root], [Sample().Candidates[0], native, unreadable],
            [new InventorySkippedPath("root", "dup.dll", InventorySkipReason.DuplicateCandidate)],
            [new InventoryTraversalError("root", "restricted", "AccessDenied")],
            [InventoryCompletionReason.CandidateLimit]);
        using var json = RenderJson("scan", inventory, new ReportOptions(ReportFormat.Json, Fields: ["name"]));
        Assert.Multiple(() =>
        {
            Assert.That(json.RootElement.GetProperty("summary").GetProperty("native").GetInt32(), Is.EqualTo(1));
            Assert.That(json.RootElement.GetProperty("summary").GetProperty("unreadable").GetInt32(), Is.EqualTo(1));
            Assert.That(json.RootElement.GetProperty("completion").GetProperty("state").GetString(), Is.EqualTo("Incomplete"));
            Assert.That(json.RootElement.GetProperty("skipped").GetArrayLength(), Is.EqualTo(1));
            Assert.That(json.RootElement.GetProperty("errors").GetArrayLength(), Is.EqualTo(1));
            Assert.That(json.RootElement.GetProperty("candidates")[2].GetProperty("diagnostics").GetArrayLength(), Is.EqualTo(1));
        });
    }

    private static JsonDocument RenderJson(string command, InventoryReport inventory, ReportOptions? options = null)
    {
        using var output = new StringWriter();
        new JsonInventoryReportRenderer().Render(inventory, command, DateTimeOffset.UnixEpoch,
            options ?? new ReportOptions(ReportFormat.Json), output);
        return JsonDocument.Parse(output.ToString());
    }

    private static string RenderHuman(InventoryReport inventory, ReportOptions? options = null)
    {
        using var output = new StringWriter();
        new HumanInventoryReportRenderer().Render(inventory, "inspect", DateTimeOffset.UnixEpoch,
            options ?? new ReportOptions(), output);
        return output.ToString();
    }

    internal static InventoryReport Sample(string path = "sample.dll")
    {
        var input = new SelectedInput(path, path, InputKind.File);
        var metadata = new AssemblyMetadata
        {
            Identity = new AssemblyIdentity("sample", new Version(1, 2, 3, 4), null, null),
            DeclaredTarget = new TargetFrameworkIdentity(".NETCoreApp,Version=v8.0"),
            FileVersion = "2.0", Company = "Example Company", PeMachine = "I386",
        };
        var assembly = new AssemblyInspectionResult(AssemblyInspectionOutcome.ManagedAssembly,
            new InspectedFileFacts(123, DateTimeOffset.UnixEpoch), metadata, null, []);
        var finding = new LifecycleFinding(LifecycleEvidenceKind.AssemblyTarget, ".NETCoreApp,Version=v8.0",
            "TargetFrameworkAttribute", TargetFrameworkFamily.ModernDotNet, LifecycleStatus.Supported,
            new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 16), "2026-11",
            "https://dotnet.microsoft.com/en-us/platform/support/policy", new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 10), false, "Advisory only");
        var candidate = new InventoryCandidate(input, new InventoryEvidence(assembly, null,
            new LifecycleAssessmentResult([finding], null)));
        return new InventoryReport([input], [candidate], [], [], []);
    }
}
