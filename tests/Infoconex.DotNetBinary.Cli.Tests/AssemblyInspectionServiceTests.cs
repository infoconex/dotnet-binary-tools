using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Domain.Models;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Unit")]
public sealed class AssemblyInspectionServiceTests
{
    [Test]
    public void DelegatesSelectedFileAndExplicitOptionsToOutboundPort()
    {
        var file = new SelectedInput("requested.dll", "/canonical/path.dll", InputKind.File);
        var options = new AssemblyInspectionOptions(ComputeSha256: true, MaxFileBytes: 1024);
        var expected = new AssemblyInspectionResult(AssemblyInspectionOutcome.NativeBinary, null, null, null, []);
        var port = new CapturingMetadataReader(expected);

        var actual = new AssemblyInspectionService(port).Inspect(file, options);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.SameAs(expected));
            Assert.That(port.Input, Is.SameAs(file));
            Assert.That(port.Options, Is.SameAs(options));
        });
    }

    [Test]
    public void RejectsDirectoryWithoutInvokingMetadataPort()
    {
        var input = new SelectedInput("source", "/source", InputKind.Directory);
        var port = new CapturingMetadataReader(null!);

        Assert.Throws<ArgumentException>(() => new AssemblyInspectionService(port).Inspect(input));
        Assert.That(port.Input, Is.Null);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void RejectsInvalidSizePoliciesBeforeOpeningFile(long limit)
    {
        var input = new SelectedInput("x", "/x", InputKind.File);
        var port = new CapturingMetadataReader(null!);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AssemblyInspectionService(port).Inspect(input, new AssemblyInspectionOptions(MaxFileBytes: limit)));
        Assert.That(port.Input, Is.Null);
    }

    private sealed class CapturingMetadataReader(AssemblyInspectionResult result) : IAssemblyMetadataReader
    {
        public SelectedInput? Input { get; private set; }

        public AssemblyInspectionOptions? Options { get; private set; }

        public AssemblyInspectionResult Read(SelectedInput input, AssemblyInspectionOptions options)
        {
            Input = input;
            Options = options;
            return result;
        }
    }
}
