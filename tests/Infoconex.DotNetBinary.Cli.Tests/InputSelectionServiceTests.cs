using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class InputSelectionServiceTests
{
    [Test]
    [Property("Scenario", "SC-INP-001")]
    public void AcceptsUnusualFileExtensionWithoutClassifyingItsContents()
    {
        var probe = new StubProbe(_ => InputPathProbeResult.Found("/selected/Sample.bin", InputKind.File));
        var service = new InputSelectionService(probe);

        var result = service.Select(InputSelectionMode.Inspect, ["Sample.bin"]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Inputs, Has.Count.EqualTo(1));
            Assert.That(result.Inputs[0].RequestedPath, Is.EqualTo("Sample.bin"));
            Assert.That(result.Inputs[0].FullPath, Is.EqualTo("/selected/Sample.bin"));
            Assert.That(result.Inputs[0].Kind, Is.EqualTo(InputKind.File));
            Assert.That(probe.Requests, Is.EqualTo(new[] { "Sample.bin" }));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-002")]
    public void RejectsMultipleInspectInputsBeforeProbing()
    {
        var probe = new StubProbe(_ => throw new AssertionException("Should not access the filesystem"));
        var result = new InputSelectionService(probe).Select(InputSelectionMode.Inspect, ["a.dll", "b.dll"]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Inputs, Is.Empty);
            Assert.That(result.Failures[0].Reason, Is.EqualTo(InputSelectionFailureReason.InvalidCount));
            Assert.That(result.Failures[0].Message, Does.Contain("exactly one"));
            Assert.That(probe.Requests, Is.Empty);
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-003")]
    public void ForwardsTwoDirectoryRootsWithoutEnumerating()
    {
        var probe = new StubProbe(path => InputPathProbeResult.Found($"/root/{path}", InputKind.Directory));
        var result = new InputSelectionService(probe).Select(InputSelectionMode.Scan, ["a", "b"]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Inputs.Select(input => input.FullPath), Is.EqualTo(new[] { "/root/a", "/root/b" }));
            Assert.That(probe.Requests, Is.EqualTo(new[] { "a", "b" }));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-006")]
    public void AFailedRootInvalidatesTheEntireSelectionEvenIfOtherRootsAreValid()
    {
        var probe = new StubProbe(path => path == "missing"
            ? InputPathProbeResult.Failed(InputSelectionFailureReason.Missing)
            : InputPathProbeResult.Found($"/root/{path}", InputKind.Directory));
        var result = new InputSelectionService(probe).Select(InputSelectionMode.Scan, ["first", "missing", "last"]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Inputs, Is.Empty);
            Assert.That(result.Failures[0].RequestedPath, Is.EqualTo("missing"));
            Assert.That(probe.Requests, Is.EqualTo(new[] { "first", "missing", "last" }));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-011")]
    public void RejectsWrongInputKindForEitherOperation()
    {
        var probe = new StubProbe(path => InputPathProbeResult.Found(path, path == "file" ? InputKind.File : InputKind.Directory));
        var service = new InputSelectionService(probe);

        var inspect = service.Select(InputSelectionMode.Inspect, ["directory"]);
        var scan = service.Select(InputSelectionMode.Scan, ["file"]);

        Assert.Multiple(() =>
        {
            Assert.That(inspect.IsSuccess, Is.False);
            Assert.That(inspect.Failures[0].Reason, Is.EqualTo(InputSelectionFailureReason.WrongKind));
            Assert.That(scan.IsSuccess, Is.False);
            Assert.That(scan.Failures[0].Reason, Is.EqualTo(InputSelectionFailureReason.WrongKind));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-012")]
    public void SelectionCallsOnlyPathProbeAndExposesValidatedInputs()
    {
        // The sole filesystem port has no method to inspect contents or enumerate directories.
        var probe = new StubProbe(path => InputPathProbeResult.Found(path, path == "file" ? InputKind.File : InputKind.Directory));
        var service = new InputSelectionService(probe);

        var inspect = service.Select(InputSelectionMode.Inspect, ["file"]);
        var scan = service.Select(InputSelectionMode.Scan, ["directory"]);

        Assert.Multiple(() =>
        {
            Assert.That(inspect.Inputs, Has.Count.EqualTo(1));
            Assert.That(scan.Inputs, Has.Count.EqualTo(1));
            Assert.That(probe.Requests, Is.EqualTo(new[] { "file", "directory" }));
        });
    }

    [Test]
    public void RejectsNoScanRootsBeforeProbing()
    {
        var probe = new StubProbe(_ => throw new AssertionException("Should not probe"));
        var result = new InputSelectionService(probe).Select(InputSelectionMode.Scan, []);
        Assert.That(result.Failures[0].Reason, Is.EqualTo(InputSelectionFailureReason.InvalidCount));
    }

    [Test]
    public void ConvertsInaccessibleProbeIntoActionableFailureWithoutPartialOutput()
    {
        var probe = new StubProbe(_ => InputPathProbeResult.Failed(InputSelectionFailureReason.Inaccessible));
        var result = new InputSelectionService(probe).Select(InputSelectionMode.Inspect, ["private.dat"]);
        Assert.Multiple(() =>
        {
            Assert.That(result.Inputs, Is.Empty);
            Assert.That(result.Failures[0].RequestedPath, Is.EqualTo("private.dat"));
            Assert.That(result.Failures[0].Message, Does.Contain("access"));
        });
    }

    private sealed class StubProbe(Func<string, InputPathProbeResult> probe) : IInputPathProbe
    {
        public List<string> Requests { get; } = [];

        public InputPathProbeResult Probe(string path)
        {
            Requests.Add(path);
            return probe(path);
        }
    }
}
