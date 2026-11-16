using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Application.ResultsReporting;
using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class CommandLineContractTests
{
    [Test]
    public void SC_CLI_001_SupportedCommandsDispatchThroughApplicationUseCases()
    {
        var services = new RecordingUseCases();
        Assert.That(Invoke(["inspect", "fake file.dll"], services).Code, Is.Zero);
        Assert.That(services.InspectCount, Is.EqualTo(1));
        Assert.That(Invoke(["scan", "one", "two"], services).Code, Is.Zero);
        Assert.That(services.ScanCount, Is.EqualTo(1));
        Assert.That(services.LastPaths, Is.EqualTo(new[] { "one", "two" }));
        Assert.That(Invoke(["--help"], services).Code, Is.Zero);
        Assert.That(Invoke(["--version"], services).Code, Is.Zero);
        Assert.That(services.SelectionCount, Is.EqualTo(2));
    }

    [TestCase("inspect")]
    [TestCase("scan")]
    [TestCase("inspect a.dll b.dll")]
    public void SC_CLI_002_InvalidArityIsRejectedBeforeSelection(string commandLine)
    {
        var services = new RecordingUseCases();
        var result = Invoke(commandLine.Split(' '), services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(2));
            Assert.That(result.StandardOutput, Is.Empty);
            Assert.That(services.SelectionCount, Is.Zero);
            Assert.That(services.InspectCount + services.ScanCount, Is.Zero);
        });
    }

    [Test]
    public void SC_CLI_003_ScanFlagsLimitsAndHashingReachTypedInventoryRequest()
    {
        var services = new RecordingUseCases();
        var result = Invoke(["scan", "first dir", "second dir", "--recursive", "--follow-links",
            "--max-depth", "3", "--max-candidates", "7", "--max-file-mib", "12",
            "--max-elapsed-seconds", "8", "--max-cpu-seconds", "9", "--sha256"], services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.Zero);
            Assert.That(services.LastPaths, Is.EqualTo(new[] { "first dir", "second dir" }));
            Assert.That(services.LastInventoryOptions?.Recursive, Is.True);
            Assert.That(services.LastInventoryOptions?.FollowLinks, Is.True);
            Assert.That(services.LastInventoryOptions?.MaxDepth, Is.EqualTo(3));
            Assert.That(services.LastInventoryOptions?.MaxCandidates, Is.EqualTo(7));
            Assert.That(services.LastInventoryOptions?.MaxFileBytes, Is.EqualTo(12L * 1024 * 1024));
            Assert.That(services.LastInventoryOptions?.MaxElapsedSeconds, Is.EqualTo(8));
            Assert.That(services.LastInventoryOptions?.MaxCpuSeconds, Is.EqualTo(9));
            Assert.That(services.LastInventoryOptions?.ComputeSha256, Is.True);
        });
    }

    [Test]
    public void SC_CLI_004_ReportConfigurationAndOptInRefreshAreBoundAfterValidation()
    {
        var services = new RecordingUseCases();
        var result = Invoke(["inspect", "fake.dll", "--config", "app.runtimeconfig.json", "--format", "json",
            "--fields", "name,targetFramework", "--output", "new report.json", "--refresh-lifecycle"], services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.Zero);
            Assert.That(services.ConfigurationPath, Is.EqualTo("app.runtimeconfig.json"));
            Assert.That(services.LastReportOptions?.Format, Is.EqualTo(ReportFormat.Json));
            Assert.That(services.LastReportOptions?.Fields, Is.EqualTo(new[] { "name", "targetFramework" }));
            Assert.That(services.LastReportOptions?.OutputPath, Is.EqualTo("new report.json"));
            Assert.That(services.RefreshCount, Is.EqualTo(1));
        });
    }

    [TestCase("inspect file.dll --unknown")]
    [TestCase("inspect file.dll -x")]
    [TestCase("inspect file.dll --format")]
    [TestCase("scan root --max-depth --recursive")]
    [TestCase("scan root --max-candidates 0")]
    [TestCase("scan root --max-file-mib 1025")]
    [TestCase("inspect file.dll --format json --format human")]
    [TestCase("scan root --config config.xml")]
    [TestCase("inspect file.dll --refresh-lifecycle --not-an-option")]
    [TestCase("nonsense file.dll")]
    public void SC_CLI_005_MalformedArgumentsNeverSelectOrRefresh(string commandLine)
    {
        var services = new RecordingUseCases();
        var result = Invoke(commandLine.Split(' '), services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(2));
            Assert.That(result.StandardOutput, Is.Empty);
            Assert.That(result.StandardError, Is.Not.Empty);
            Assert.That(services.SelectionCount + services.RefreshCount + services.ScanCount + services.InspectCount, Is.Zero);
        });
    }

    [TestCase("inspect file.dll --details --fields name")]
    [TestCase("inspect file.dll --details --format json")]
    public void SC_CLI_006_ConflictingPresentationOptionsFailEarly(string commandLine)
    {
        var services = new RecordingUseCases();
        var result = Invoke(commandLine.Split(' '), services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(2));
            Assert.That(result.StandardError, Does.Contain("--details"));
            Assert.That(services.SelectionCount, Is.Zero);
        });
    }

    [Test]
    public void SC_CLI_007_CompletedPartialInventoryRetainsSuccessfulExitCode()
    {
        var services = new RecordingUseCases { Partial = true };
        var result = Invoke(["scan", "root"], services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.Zero);
            Assert.That(result.StandardOutput, Does.Contain("REPORT"));
            Assert.That(services.LastReport?.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
        });
    }

    [Test]
    public void SC_CLI_008_FailedSelectedRootBlocksAllInventoryWork()
    {
        var services = new RecordingUseCases { RejectSelection = true };
        var result = Invoke(["scan", "present", "missing"], services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.EqualTo(2));
            Assert.That(result.StandardOutput, Is.Empty);
            Assert.That(services.SelectionCount, Is.EqualTo(1));
            Assert.That(services.ScanCount, Is.Zero);
        });
    }

    [Test]
    public void SC_CLI_009_ActualProcessProducesOnlyJsonOnStdoutAndWarningsOnStderr()
    {
        var folder = Path.Combine(Path.GetTempPath(), "cli-contract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllBytes(Path.Combine(folder, "malformed.dll"), [0, 1, 2]);
            var result = RunProcess("scan", folder, "--format", "json");
            using var document = JsonDocument.Parse(result.StandardOutput);
            Assert.Multiple(() =>
            {
                Assert.That(result.Code, Is.Zero);
                Assert.That(document.RootElement.GetProperty("schemaVersion").GetString(), Is.EqualTo("1.0.0"));
                Assert.That(document.RootElement.GetProperty("completion").GetProperty("state").GetString(),
                    Is.EqualTo("Incomplete"));
                Assert.That(result.StandardError, Does.Contain("warning"));
                Assert.That(result.StandardOutput, Does.Not.Contain("\u001b"));
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public void SC_CLI_010_HelpIsOfflineAndVersionMatchesAssemblyMetadata()
    {
        var services = new RecordingUseCases();
        var help = Invoke(["--help"], services);
        var version = Invoke(["--version"], services);
        var assembly = typeof(CliApplication).Assembly;
        var expected = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ??
            assembly.GetName().Version?.ToString();
        Assert.Multiple(() =>
        {
            Assert.That(help.StandardOutput, Does.Contain("--sha256"));
            Assert.That(help.StandardOutput, Does.Contain("--refresh-lifecycle"));
            Assert.That(help.StandardOutput, Does.Contain("512"));
            Assert.That(help.StandardOutput, Does.Contain("offline"));
            Assert.That(help.StandardOutput, Does.Not.Contain("not implemented yet"));
            Assert.That(version.StandardOutput.Trim(), Is.EqualTo(expected));
            Assert.That(services.SelectionCount + services.RefreshCount, Is.Zero);
        });
    }

    [Test]
    public void SC_CLI_011_SubstitutedPortsHandleOptionLikePathsWithoutTouchingHostFileSystem()
    {
        var services = new RecordingUseCases();
        var result = Invoke(["inspect", "--sha256", "--", "--imaginary.dll"], services);
        Assert.Multiple(() =>
        {
            Assert.That(result.Code, Is.Zero);
            Assert.That(services.LastPaths, Is.EqualTo(new[] { "--imaginary.dll" }));
            Assert.That(services.LastInventoryOptions?.ComputeSha256, Is.True);
            Assert.That(services.InspectCount, Is.EqualTo(1));
            Assert.That(services.ScanCount, Is.Zero);
            Assert.That(result.StandardOutput, Does.Contain("REPORT"));
        });
    }

    private static (int Code, string StandardOutput, string StandardError) Invoke(
        string[] arguments, RecordingUseCases useCases)
    {
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var exit = CliApplication.Run(arguments, stdout, stderr, useCases.Services);
        return (exit, stdout.ToString(), stderr.ToString());
    }

    private static (int Code, string StandardOutput, string StandardError) RunProcess(params string[] arguments)
    {
        var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "DotNetBinary.slnx")))
        {
            folder = folder.Parent;
        }

        Assert.That(folder, Is.Not.Null, "Test build must run from a repository checkout.");
        var configuration = TestContext.CurrentContext.TestDirectory.Contains("/Debug/", StringComparison.OrdinalIgnoreCase) ||
            TestContext.CurrentContext.TestDirectory.Contains("\\Debug\\", StringComparison.OrdinalIgnoreCase)
            ? "Debug" : "Release";
        var runtimeDirectory = Path.Combine(folder!.FullName, "src", "Infoconex.DotNetBinary.Cli", "bin",
            configuration, "net11.0");
        var executable = Path.Combine(runtimeDirectory, OperatingSystem.IsWindows() ? "dotnet-binary.exe" : "dotnet-binary");
        var info = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = Process.Start(info) ?? throw new InvalidOperationException("Could not launch the built CLI.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        Assert.That(process.WaitForExit(30_000), Is.True, "CLI process timed out.");
        return (process.ExitCode, stdout, stderr);
    }

    private sealed class RecordingUseCases : IInputSelectionUseCase, IInventoryUseCase, IReportUseCase
    {
        public int SelectionCount { get; private set; }
        public int InspectCount { get; private set; }
        public int ScanCount { get; private set; }
        public int RefreshCount { get; private set; }
        public bool RejectSelection { get; init; }
        public bool Partial { get; init; }
        public IReadOnlyList<string>? LastPaths { get; private set; }
        public InventoryOptions? LastInventoryOptions { get; private set; }
        public ReportOptions? LastReportOptions { get; private set; }
        public string? ConfigurationPath { get; private set; }
        public InventoryReport? LastReport { get; private set; }

        public CliUseCases Services => new(this, this, this, Refresh);

        public InputSelectionResult Select(InputSelectionMode mode, IReadOnlyList<string> requestedPaths)
        {
            SelectionCount++;
            LastPaths = requestedPaths;
            if (RejectSelection)
            {
                return InputSelectionResult.Rejected([new("missing", InputSelectionFailureReason.Missing,
                    "Selected directory does not exist.")]);
            }

            var kind = mode == InputSelectionMode.Inspect ? InputKind.File : InputKind.Directory;
            return InputSelectionResult.Accepted(requestedPaths.Select(path => new SelectedInput(path, path, kind)));
        }

        public InventoryReport Inspect(SelectedInput file, SelectedInput? explicitConfiguration = null,
            InventoryOptions? options = null)
        {
            InspectCount++;
            ConfigurationPath = explicitConfiguration?.RequestedPath;
            LastInventoryOptions = options;
            return MakeInventory([file]);
        }

        public InventoryReport Scan(IReadOnlyList<SelectedInput> roots, InventoryOptions? options = null)
        {
            ScanCount++;
            LastInventoryOptions = options;
            return MakeInventory(roots);
        }

        public string? Write(InventoryReport inventory, string command, ReportOptions options,
            TextWriter output, TextWriter diagnostics)
        {
            LastReport = inventory;
            LastReportOptions = options;
            output.WriteLine("REPORT");
            return null;
        }

        private InventoryReport MakeInventory(IReadOnlyList<SelectedInput> roots) => new(roots, [], [], [],
            Partial ? [InventoryCompletionReason.CandidateLimit] : []);

        private LifecycleRefreshResult Refresh()
        {
            RefreshCount++;
            return new LifecycleRefreshResult(true, null!, null);
        }
    }
}
