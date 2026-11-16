using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class InspectionInventoryServiceTests
{
    [Test]
    [Property("Scenario", "SC-INV-001")]
    public void SingleFileInspectionHasOneCandidateAndSharedCompletion()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/one.unusual", 25);
        var inventory = Service(fs).Inspect(File("/one.unusual"));

        Assert.Multiple(() =>
        {
            Assert.That(inventory.Roots, Has.Count.EqualTo(1));
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Candidates[0].Input.FullPath, Is.EqualTo("/one.unusual"));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Complete));
            Assert.That(inventory.Summary.TotalCandidates, Is.EqualTo(1));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-002")]
    public void MultiRootScanProducesOneInventoryWithEveryValidatedRoot()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/a/a.dll", 10);
        fs.AddFile("/b/b.exe", 10);
        var inventory = Service(fs).Scan([DirectoryRoot("/a"), DirectoryRoot("/b")]);

        Assert.Multiple(() =>
        {
            Assert.That(inventory.Roots.Select(r => r.FullPath), Is.EquivalentTo(new[] { "/a", "/b" }));
            Assert.That(inventory.Candidates, Has.Count.EqualTo(2));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Complete));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-003")]
    public void ScanSelectsOnlyAsciiCaseInsensitiveDllAndExeWithoutReadingUnrelatedFiles()
    {
        var fs = new FakeInventoryFileSystem();
        foreach (var path in new[] { "/r/a.dll", "/r/b.DLL", "/r/c.exe", "/r/d.EXE", "/r/e.txt", "/r/f.bin", "/r/g.dℓl" })
        {
            fs.AddFile(path, 10);
        }

        var inspector = new FakeInventoryInspector();
        var inventory = Service(fs, inspector).Scan([DirectoryRoot("/r")]);
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(4));
            Assert.That(inspector.Visited, Is.EquivalentTo(new[] { "/r/a.dll", "/r/b.DLL", "/r/c.exe", "/r/d.EXE" }));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-004")]
    public void NonRecursiveScanDoesNotEnterNestedDirectory()
    {
        var fs = NestedFiles();
        var inventory = Service(fs).Scan([DirectoryRoot("/r")]);
        Assert.That(inventory.Candidates.Select(c => c.Input.FullPath), Is.EquivalentTo(new[] { "/r/top.dll" }));
    }

    [Test]
    [Property("Scenario", "SC-INV-005")]
    public void RecursiveScanIncludesNestedCandidatesWithinScope()
    {
        var inventory = Service(NestedFiles()).Scan([DirectoryRoot("/r")], new InventoryOptions(Recursive: true));
        Assert.That(inventory.Candidates.Select(c => c.Input.FullPath), Is.EquivalentTo(new[] { "/r/top.dll", "/r/sub/inner.dll" }));
    }

    [Test]
    [Property("Scenario", "SC-INV-006")]
    public void EachNativeManagedAndMalformedCandidateKeepsItsOwnClassification()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/r/managed.dll", 10);
        fs.AddFile("/r/host.exe", 10);
        fs.AddFile("/r/bad.dll", 10);
        var inspector = new FakeInventoryInspector();
        inspector.Outcomes["/r/host.exe"] = AssemblyInspectionOutcome.NativeBinary;
        inspector.Outcomes["/r/bad.dll"] = AssemblyInspectionOutcome.CorruptOrUnrecognized;

        var inventory = Service(fs, inspector).Scan([DirectoryRoot("/r")]);
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(3));
            Assert.That(inventory.Summary.Managed, Is.EqualTo(1));
            Assert.That(inventory.Summary.Native, Is.EqualTo(1));
            Assert.That(inventory.Summary.Corrupt, Is.EqualTo(1));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-007")]
    public void CandidateFailuresDoNotAbandonNeighboringValidFindings()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/r/a.dll", 10);
        fs.AddFile("/r/b.dll", 10);
        fs.AddFile("/r/c.dll", 10);
        var inspector = new FakeInventoryInspector();
        inspector.ThrowPaths.Add("/r/b.dll");

        var inventory = Service(fs, inspector).Scan([DirectoryRoot("/r")]);
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Summary.Managed, Is.EqualTo(2));
            Assert.That(inventory.Summary.Unreadable, Is.EqualTo(1));
            Assert.That(inventory.Candidates, Has.Count.EqualTo(3));
            Assert.That(inventory.Completion.Reasons, Does.Contain(InventoryCompletionReason.CandidateFailure));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-008")]
    public void DiscoveredDirectoryLinksAreTrackedAndSkippedByDefault()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddDirectoryLink("/r/link", "/elsewhere");
        fs.AddFile("/elsewhere/hidden.dll", 10);
        var inventory = Service(fs).Scan([DirectoryRoot("/r")], new InventoryOptions(Recursive: true));

        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Is.Empty);
            Assert.That(inventory.Skipped.Single().Reason, Is.EqualTo(InventorySkipReason.LinkedDirectory));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-009")]
    public void ExplicitlySelectedLinkedDirectoryIsTraversedWithoutFollowLinks()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddDirectoryLink("/alias", "/physical");
        fs.AddFile("/physical/a.dll", 10);

        var inventory = Service(fs).Scan([DirectoryRoot("/alias")]);
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Roots[0].FullPath, Is.EqualTo("/alias"));
            Assert.That(inventory.Candidates[0].Input.FullPath, Is.EqualTo("/alias/a.dll"));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-010")]
    public void FollowLinksDetectsCyclesAndDuplicateSubtrees()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddDirectoryLink("/a/to-b", "/b");
        fs.AddDirectoryLink("/b/to-a", "/a");
        fs.AddFile("/b/data.dll", 10);

        var inventory = Service(fs).Scan([DirectoryRoot("/a")], new InventoryOptions(Recursive: true, FollowLinks: true));
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Skipped.Any(s => s.Reason == InventorySkipReason.DuplicateDirectory), Is.True);
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-011")]
    public void CandidateCountLimitStopsAtFirstAdditionalUniqueCandidate()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/r/a.dll", 10);
        fs.AddFile("/r/b.dll", 10);
        var inventory = Service(fs).Scan([DirectoryRoot("/r")], new InventoryOptions(MaxCandidates: 1));
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Completion.Reasons, Does.Contain(InventoryCompletionReason.CandidateLimit));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-012")]
    public void OversizedCandidateIsRecordedWithoutInvokingParser()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/r/large.dll", 2 * 1024 * 1024);
        var inspector = new FakeInventoryInspector();
        var inventory = Service(fs, inspector).Scan([DirectoryRoot("/r")], new InventoryOptions(MaxFileBytes: 1024 * 1024));
        Assert.Multiple(() =>
        {
            Assert.That(inspector.Visited, Is.Empty);
            Assert.That(inventory.Candidates[0].Assembly.Outcome, Is.EqualTo(AssemblyInspectionOutcome.ResourceLimited));
            Assert.That(inventory.Completion.Reasons, Does.Contain(InventoryCompletionReason.FileSizeLimit));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-013")]
    public void DepthLimitProducesIncompleteInventoryWithoutDiscardingOtherFindings()
    {
        var inventory = Service(NestedFiles()).Scan([DirectoryRoot("/r")], new InventoryOptions(Recursive: true, MaxDepth: 0));
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
            Assert.That(inventory.Completion.Reasons, Does.Contain(InventoryCompletionReason.DepthLimit));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-014")]
    public void SummaryReconcilesExclusiveClassificationsUnknownsAndSeparateLifecycleEvidence()
    {
        var fs = new FakeInventoryFileSystem();
        foreach (var path in new[] { "/r/a.dll", "/r/b.exe", "/r/c.dll" }) fs.AddFile(path, 10);
        var inspector = new FakeInventoryInspector();
        inspector.Outcomes["/r/b.exe"] = AssemblyInspectionOutcome.NativeBinary;
        inspector.Outcomes["/r/c.dll"] = AssemblyInspectionOutcome.Unreadable;
        var inventory = Service(fs, inspector).Scan([DirectoryRoot("/r")]);

        Assert.Multiple(() =>
        {
            Assert.That(inventory.Summary.TotalCandidates, Is.EqualTo(3));
            Assert.That(inventory.Summary.Managed + inventory.Summary.Native + inventory.Summary.Corrupt +
                inventory.Summary.Unreadable + inventory.Summary.ResourceLimited, Is.EqualTo(3));
            Assert.That(inventory.Summary.UnknownTargets, Is.EqualTo(1));
            Assert.That(inventory.Summary.Failed, Is.EqualTo(1));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-015")]
    public void OverlappingRootsYieldEachPhysicalCandidateOnlyOnce()
    {
        var fs = NestedFiles();
        var inventory = Service(fs).Scan([DirectoryRoot("/r"), DirectoryRoot("/r/sub")], new InventoryOptions(Recursive: true));
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(2));
            Assert.That(inventory.Skipped.Any(s => s.Reason == InventorySkipReason.DuplicateDirectory ||
                s.Reason == InventorySkipReason.DuplicateCandidate), Is.True);
            Assert.That(inventory.Roots, Has.Count.EqualTo(2));
        });
    }

    [Test]
    [Property("Scenario", "SC-INV-016")]
    public void TraversalErrorPreservesOtherRootAndIdentifiesIncompleteCoverage()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/good/a.dll", 10);
        fs.DeniedDirectories.Add("/denied");
        var inventory = Service(fs).Scan([DirectoryRoot("/denied"), DirectoryRoot("/good")]);
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Errors[0].Root, Is.EqualTo("/denied"));
            Assert.That(inventory.Completion.Reasons, Does.Contain(InventoryCompletionReason.TraversalError));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
        });
    }

    [TestCase(129, 100, 1024 * 1024L, 300, 300)]
    [TestCase(32, 0, 1024 * 1024L, 300, 300)]
    [TestCase(32, 100, 0, 300, 300)]
    [TestCase(32, 100, 1024 * 1024L, 0, 300)]
    [TestCase(32, 100, 1024 * 1024L, 300, 3601)]
    public void PolicyRejectsOutOfRangeLimits(int depth, int count, long bytes, int elapsed, int cpu)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new InventoryOptions(MaxDepth: depth,
            MaxCandidates: count, MaxFileBytes: bytes, MaxElapsedSeconds: elapsed, MaxCpuSeconds: cpu).Validate());
    }

    private static InventoryService Service(FakeInventoryFileSystem fs, FakeInventoryInspector? inspector = null) =>
        new(fs, inspector ?? new FakeInventoryInspector(), TimeProvider.System, new FixedInventoryCpuClock());

    private static SelectedInput File(string path) => new(path, path, InputKind.File);

    private static SelectedInput DirectoryRoot(string path) => new(path, path, InputKind.Directory);

    private static FakeInventoryFileSystem NestedFiles()
    {
        var fs = new FakeInventoryFileSystem();
        fs.AddFile("/r/top.dll", 10);
        fs.AddFile("/r/sub/inner.dll", 10);
        return fs;
    }
}
