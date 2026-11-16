using System.Security.Cryptography;
using Infoconex.DotNetBinary.Application.AssemblyInspection;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.AssemblyInspection;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class AssemblyInspectionTests
{
    private string _directory = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "assembly-inspection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    [Property("Scenario", "SC-ASM-001")]
    public void ReadsManagedDllMetadataWithoutExecutingIt()
    {
        var result = Inspect(Write("valid.dll", AssemblyFixtureFactory.Managed()));

        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(AssemblyInspectionOutcome.ManagedAssembly));
            Assert.That(result.Metadata!.Identity!.Name, Is.EqualTo("Synthetic.Sample"));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-002")]
    public void IdentifiesRenamedManagedContentIndependentOfFileExtension()
    {
        var result = Inspect(Write("unknown.weird", AssemblyFixtureFactory.Managed()));
        Assert.That(result.Outcome, Is.EqualTo(AssemblyInspectionOutcome.ManagedAssembly));
    }

    [Test]
    [Property("Scenario", "SC-ASM-003")]
    public void ClassifiesNativePeWithoutInventingManagedTarget()
    {
        var result = Inspect(Write("native.exe", AssemblyFixtureFactory.NativePe()));
        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(AssemblyInspectionOutcome.NativeBinary));
            Assert.That(result.Metadata, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-004")]
    public void DistinguishesTruncatedContentFromReadFailure()
    {
        var truncated = Inspect(Write("truncated.dll", [0x4d, 0x5a, 0x00]));
        var unreadable = Inspect(Path.Combine(_directory, "missing.dll"));

        Assert.Multiple(() =>
        {
            Assert.That(truncated.Outcome, Is.EqualTo(AssemblyInspectionOutcome.CorruptOrUnrecognized));
            Assert.That(unreadable.Outcome, Is.EqualTo(AssemblyInspectionOutcome.Unreadable));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-005")]
    public void RetainsExactDeclaredFrameworkAndAttributeProvenance()
    {
        var result = Inspect(Write("framework.dll", AssemblyFixtureFactory.Managed(".NETFramework,Version=v4.8")));
        Assert.Multiple(() =>
        {
            Assert.That(result.Metadata!.DeclaredTarget!.Moniker, Is.EqualTo(".NETFramework,Version=v4.8"));
            Assert.That(result.Metadata.DeclaredTarget.EvidenceSource, Is.EqualTo("System.Runtime.Versioning.TargetFrameworkAttribute"));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-006")]
    public void IdentifiesDeclaredFrameworkFamiliesAndVersions()
    {
        var samples = new[]
        {
            (".NETFramework,Version=v4.8", TargetFrameworkFamily.NetFramework, new Version(4, 8)),
            (".NETCoreApp,Version=v3.1", TargetFrameworkFamily.NetCore, new Version(3, 1)),
            (".NETCoreApp,Version=v11.0", TargetFrameworkFamily.ModernDotNet, new Version(11, 0)),
            (".NETStandard,Version=v2.0", TargetFrameworkFamily.NetStandard, new Version(2, 0)),
        };

        foreach (var (moniker, family, version) in samples)
        {
            var result = Inspect(Write(Guid.NewGuid().ToString("N"), AssemblyFixtureFactory.Managed(moniker)));
            Assert.Multiple(() =>
            {
                Assert.That(result.Metadata!.DeclaredTarget!.Family, Is.EqualTo(family), moniker);
                Assert.That(result.Metadata.DeclaredTarget.Version, Is.EqualTo(version), moniker);
            });
        }
    }

    [Test]
    [Property("Scenario", "SC-ASM-007")]
    public void UnknownFutureMonikerRemainsRawAndUnrecognized()
    {
        var result = Inspect(Write("future.dll", AssemblyFixtureFactory.Managed(".NETCoreApp,Version=v99.0")));
        Assert.Multiple(() =>
        {
            Assert.That(result.Metadata!.DeclaredTarget!.Moniker, Is.EqualTo(".NETCoreApp,Version=v99.0"));
            Assert.That(result.Metadata.DeclaredTarget.Family, Is.EqualTo(TargetFrameworkFamily.Unrecognized));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-008")]
    public void MissingTargetRemainsUnknownDespiteClrVersionAndReferences()
    {
        var result = Inspect(Write("net11.dll", AssemblyFixtureFactory.Managed(target: null)));
        Assert.Multiple(() =>
        {
            Assert.That(result.Metadata!.DeclaredTarget, Is.Null);
            Assert.That(result.Metadata.ClrMetadataVersion, Is.Not.Empty);
            Assert.That(result.Metadata.References, Is.Not.Empty);
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-009")]
    public void PreservesFullAssemblyIdentityIncludingToken()
    {
        var result = Inspect(Write("identity.dll", AssemblyFixtureFactory.Managed(publicKey: true)));
        var identity = result.Metadata!.Identity!;
        Assert.Multiple(() =>
        {
            Assert.That(identity.Name, Is.EqualTo("Synthetic.Sample"));
            Assert.That(identity.Version, Is.EqualTo(new Version(3, 2, 1, 0)));
            Assert.That(identity.Culture, Is.EqualTo("en-US"));
            Assert.That(identity.PublicKeyToken, Has.Length.EqualTo(16));
            Assert.That(identity.FullName, Does.Contain("Version=3.2.1.0"));
            Assert.That(identity.FullName, Does.Contain("PublicKeyToken="));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-010")]
    public void KeepsAssemblyFileAndInformationalVersionsDistinct()
    {
        var result = Inspect(Write("optional.dll", AssemblyFixtureFactory.Managed(optionalAttributes: true)));
        var metadata = result.Metadata!;
        Assert.Multiple(() =>
        {
            Assert.That(metadata.Identity!.Version.ToString(), Is.EqualTo("3.2.1.0"));
            Assert.That(metadata.FileVersion, Is.EqualTo("7.8.9.10"));
            Assert.That(metadata.InformationalVersion, Is.EqualTo("release-4+sha"));
            Assert.That(metadata.Company, Is.EqualTo("Example Inc."));
            Assert.That(metadata.Product, Is.EqualTo("Example Product"));
            Assert.That(metadata.Description, Is.EqualTo("Example description"));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-011")]
    public void MissingOptionalAttributesStayAbsent()
    {
        var result = Inspect(Write("minimal.dll", AssemblyFixtureFactory.Managed()));
        Assert.Multiple(() =>
        {
            Assert.That(result.Metadata!.FileVersion, Is.Null);
            Assert.That(result.Metadata.InformationalVersion, Is.Null);
            Assert.That(result.Metadata.Company, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-012")]
    public void ReportsMachineAndClrFlagsAsMetadataFacts()
    {
        var result = Inspect(Write("amd64.dll", AssemblyFixtureFactory.Managed(
            machine: System.Reflection.PortableExecutable.Machine.Amd64,
            flags: System.Reflection.PortableExecutable.CorFlags.ILOnly |
                   System.Reflection.PortableExecutable.CorFlags.StrongNameSigned)));
        Assert.Multiple(() =>
        {
            Assert.That(result.Metadata!.PeMachine, Is.EqualTo("Amd64"));
            Assert.That(result.Metadata.CorFlags, Does.Contain("ILOnly"));
            Assert.That(result.Metadata.CorFlags, Does.Contain("StrongNameSigned"));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-013")]
    public void ReportsDeclaredEntryPointAndDirectReferencesOnly()
    {
        var result = Inspect(Write("entry.exe", AssemblyFixtureFactory.Managed(entryPoint: true)));
        Assert.Multiple(() =>
        {
            Assert.That(result.Metadata!.HasEntryPoint, Is.True);
            Assert.That(result.Metadata.References.Select(reference => reference.Name),
                Does.Contain("Synthetic.Dependency"));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-014")]
    public void DoesNotAssociateAdjacentNativeHostWithManagedAssembly()
    {
        var native = Inspect(Write("host.exe", AssemblyFixtureFactory.NativePe()));
        var managed = Inspect(Write("host.dll", AssemblyFixtureFactory.Managed()));
        Assert.Multiple(() =>
        {
            Assert.That(native.Outcome, Is.EqualTo(AssemblyInspectionOutcome.NativeBinary));
            Assert.That(managed.Outcome, Is.EqualTo(AssemblyInspectionOutcome.ManagedAssembly));
            Assert.That(native.Metadata, Is.Null);
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-015")]
    public void ReportsLengthAndUtcLastModifiedTimestamp()
    {
        var bytes = AssemblyFixtureFactory.Managed();
        var path = Write("facts.dll", bytes);
        var expected = File.GetLastWriteTimeUtc(path);
        var result = Inspect(path);
        Assert.Multiple(() =>
        {
            Assert.That(result.FileFacts!.Length, Is.EqualTo(bytes.Length));
            Assert.That(result.FileFacts.LastModifiedUtc.UtcDateTime, Is.EqualTo(expected));
        });
    }

    [Test]
    [Property("Scenario", "SC-ASM-016")]
    public void CalculatesSha256OnlyOnExplicitRequest()
    {
        var bytes = AssemblyFixtureFactory.Managed();
        var path = Write("hash.dll", bytes);
        Assert.That(Inspect(path).Sha256, Is.Null);

        var result = Inspect(path, hash: true);
        Assert.That(result.Sha256, Is.EqualTo(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()));
    }

    [Test]
    [Property("Scenario", "SC-ASM-017")]
    public void MalformedOptionalAttributeLeavesIndependentMetadataAvailable()
    {
        var result = Inspect(Write("partial.dll", AssemblyFixtureFactory.Managed(
            optionalAttributes: true, malformedOptionalAttribute: true)));
        Assert.Multiple(() =>
        {
            Assert.That(result.Outcome, Is.EqualTo(AssemblyInspectionOutcome.ManagedAssembly));
            Assert.That(result.Metadata!.Identity!.Name, Is.EqualTo("Synthetic.Sample"));
            Assert.That(result.Metadata.FileVersion, Is.EqualTo("7.8.9.10"));
            Assert.That(result.Diagnostics, Has.Some.Property("Field").EqualTo("Company"));
        });
    }

    [Test]
    public void BoundsOversizedCandidatesBeforeReadingContents()
    {
        var path = Write("too-large.dll", AssemblyFixtureFactory.Managed());
        var input = new SelectedInput(path, path, InputKind.File);
        var service = new AssemblyInspectionService(new PortableExecutableAssemblyReader());

        var result = service.Inspect(input, new AssemblyInspectionOptions(MaxFileBytes: 50));

        Assert.That(result.Outcome, Is.EqualTo(AssemblyInspectionOutcome.ResourceLimited));
    }

    private AssemblyInspectionResult Inspect(string path, bool hash = false)
    {
        var input = new SelectedInput(path, path, InputKind.File);
        return new AssemblyInspectionService(new PortableExecutableAssemblyReader())
            .Inspect(input, new AssemblyInspectionOptions(ComputeSha256: hash));
    }

    private string Write(string name, byte[] content)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllBytes(path, content);
        return path;
    }
}
