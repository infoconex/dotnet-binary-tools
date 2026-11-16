using Infoconex.DotNetBinary.Application.InspectionInventory;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.InspectionInventory;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class HostInventoryFileSystemTests
{
    private string _folder = null!;

    [SetUp]
    public void SetUp()
    {
        _folder = Path.Combine(Path.GetTempPath(), "dotnet-binary-cap005-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_folder, recursive: true);

    [Test]
    public void HostEnumerationFindsOnlyLiteralRootAndDoesNotReadUnrelatedFiles()
    {
        var nested = Path.Combine(_folder, "nested");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(_folder, "managed.DLL"), "test");
        File.WriteAllText(Path.Combine(_folder, "unrelated.txt"), "test");
        File.WriteAllText(Path.Combine(nested, "nested.exe"), "test");
        var inspector = new FakeInventoryInspector();
        var inventory = Service(inspector).Scan([Root(_folder)]);

        Assert.Multiple(() =>
        {
            Assert.That(inspector.Visited, Has.Count.EqualTo(1));
            Assert.That(inspector.Visited[0], Does.EndWith("managed.DLL"));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Complete));
        });
    }

    [Test]
    public void HostLinkTraversalDeduplicatesCyclesAndOverlappingPhysicalRoots()
    {
        var nested = Path.Combine(_folder, "nested");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(nested, "one.dll"), "safe test");
        var linked = Path.Combine(nested, "back");
        try
        {
            Directory.CreateSymbolicLink(linked, _folder);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Assert.Ignore($"Host lacks symbolic-link creation permission: {exception.GetType().Name}");
        }

        var inventory = Service(new FakeInventoryInspector()).Scan([Root(_folder), Root(nested)],
            new InventoryOptions(Recursive: true, FollowLinks: true));
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Skipped.Any(item => item.Reason == InventorySkipReason.DuplicateDirectory), Is.True);
        });
    }

    [Test]
    public void HostLinkedSelectedRootTraversesWithoutFollowingDiscoveredLinks()
    {
        var target = Path.Combine(_folder, "target");
        var link = Path.Combine(_folder, "alias");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "one.dll"), "safe test");
        try
        {
            Directory.CreateSymbolicLink(link, target);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Assert.Ignore($"Host lacks symbolic-link creation permission: {exception.GetType().Name}");
        }

        var inventory = Service(new FakeInventoryInspector()).Scan([Root(link)]);
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Candidates[0].Input.FullPath, Does.StartWith(link));
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Complete));
        });
    }

    [Test]
    public void HostUnfollowedDiscoveredLinkProducesIncompleteCoverage()
    {
        var target = Path.Combine(_folder, "target");
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "one.dll"), "safe test");
        try
        {
            Directory.CreateSymbolicLink(Path.Combine(_folder, "alias"), target);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Assert.Ignore($"Host lacks symbolic-link creation permission: {exception.GetType().Name}");
        }

        var inventory = Service(new FakeInventoryInspector()).Scan([Root(_folder)], new InventoryOptions(Recursive: true));
        Assert.Multiple(() =>
        {
            Assert.That(inventory.Candidates, Has.Count.EqualTo(1));
            Assert.That(inventory.Skipped.Any(item => item.Reason == InventorySkipReason.LinkedDirectory), Is.True);
            Assert.That(inventory.Completion.State, Is.EqualTo(InventoryCompletionState.Incomplete));
        });
    }

    private static InventoryService Service(FakeInventoryInspector inspector) =>
        new(new HostInventoryFileSystem(), inspector, TimeProvider.System, new FixedInventoryCpuClock());

    private static SelectedInput Root(string path) => new(path, path, InputKind.Directory);
}
