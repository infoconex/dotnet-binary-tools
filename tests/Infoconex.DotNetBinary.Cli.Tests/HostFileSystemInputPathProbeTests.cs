using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.InputSelection;
using NUnit.Framework;

namespace Infoconex.DotNetBinary.Cli.Tests;

[TestFixture]
[Category("Integration")]
public sealed class HostFileSystemInputPathProbeTests
{
    private string _folder = null!;
    private HostFileSystemInputPathProbe _probe = null!;

    [SetUp]
    public void SetUp()
    {
        _folder = Path.Combine(Path.GetTempPath(), "dotnet-binary-cap001-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        _probe = new HostFileSystemInputPathProbe();
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_folder, recursive: true);
    }

    [Test]
    [Property("Scenario", "SC-INP-004")]
    public void ResolvesRelativePathsAgainstTheHostWorkingDirectory()
    {
        // Keep the fixture below the current directory to exercise a relative path
        // even on Windows hosts where the temp directory may be on another drive.
        var folder = Path.Combine(Environment.CurrentDirectory, "cap001-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var file = Path.Combine(folder, "資料 with spaces.bin");
            File.WriteAllText(file, "synthetic fixture");
            var relative = Path.GetRelativePath(Environment.CurrentDirectory, file);

            var result = _probe.Probe(relative);

            Assert.Multiple(() =>
            {
                Assert.That(Path.IsPathFullyQualified(relative), Is.False);
                Assert.That(result.FullPath, Is.EqualTo(Path.GetFullPath(file)));
                Assert.That(result.Kind, Is.EqualTo(InputKind.File));
            });
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    [Property("Scenario", "SC-INP-005")]
    public void ExplainsMissingExplicitPathWithoutLosingItsIdentity()
    {
        var path = Path.Combine(_folder, "missing.dll");
        var result = new InputSelectionService(_probe).Select(InputSelectionMode.Inspect, [path]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Failures[0].RequestedPath, Is.EqualTo(path));
            Assert.That(result.Failures[0].Reason, Is.EqualTo(InputSelectionFailureReason.Missing));
            Assert.That(result.Failures[0].Message, Does.Contain("exist"));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-007")]
    public void AcceptsFileSymbolicLinkToAnExistingRegularFile()
    {
        var target = Path.Combine(_folder, "payload.dat");
        var link = Path.Combine(_folder, "file-link");
        File.WriteAllText(target, "safe fixture");
        CreateFileSymlinkOrIgnore(link, target);

        var result = _probe.Probe(link);
        Assert.Multiple(() =>
        {
            Assert.That(result.Kind, Is.EqualTo(InputKind.File));
            Assert.That(result.FullPath, Is.EqualTo(link));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-008")]
    public void AcceptsAnExplicitDirectorySymbolicLinkWithoutTraversal()
    {
        var target = Path.Combine(_folder, "target");
        var link = Path.Combine(_folder, "directory-link");
        Directory.CreateDirectory(target);
        Directory.CreateDirectory(Path.Combine(target, "unvisited-child"));
        CreateDirectorySymlinkOrIgnore(link, target);

        var result = _probe.Probe(link);
        Assert.Multiple(() =>
        {
            Assert.That(result.Kind, Is.EqualTo(InputKind.Directory));
            Assert.That(result.FullPath, Is.EqualTo(link));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-009")]
    public void RejectsBrokenFileAndDirectoryLinks()
    {
        var fileLink = Path.Combine(_folder, "broken-file-link");
        var directoryLink = Path.Combine(_folder, "broken-directory-link");
        CreateFileSymlinkOrIgnore(fileLink, Path.Combine(_folder, "not-a-file"));
        CreateDirectorySymlinkOrIgnore(directoryLink, Path.Combine(_folder, "not-a-directory"));

        var file = _probe.Probe(fileLink);
        var directory = _probe.Probe(directoryLink);

        Assert.Multiple(() =>
        {
            Assert.That(file.Failure, Is.Not.EqualTo(InputSelectionFailureReason.None));
            Assert.That(directory.Failure, Is.Not.EqualTo(InputSelectionFailureReason.None));
        });
    }

    [Test]
    [Property("Scenario", "SC-INP-010")]
    public void LeavesWildcardArgumentLiteralRatherThanExpandingIt()
    {
        File.WriteAllText(Path.Combine(_folder, "match.dat"), "safe fixture");
        var wildcard = Path.Combine(_folder, "*.dat");

        var result = _probe.Probe(wildcard);

        Assert.That(result.Failure, Is.Not.EqualTo(InputSelectionFailureReason.None));
    }

    [Test]
    public void DirectoryIsNotMistakenForARegularFile()
    {
        var result = _probe.Probe(_folder);
        Assert.That(result.Kind, Is.EqualTo(InputKind.Directory));
    }

    [Test]
    public void InvalidPathDoesNotLeakRawExceptionDetails()
    {
        var result = _probe.Probe("invalid\0path");
        Assert.That(result.Failure, Is.EqualTo(InputSelectionFailureReason.InvalidPath));
    }

    [Test]
    [Platform("Linux,MacOsX")]
    public void RejectsNonRegularNamedPipeWithoutOpeningIt()
    {
        var pipe = Path.Combine(_folder, "special-node");
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("mkfifo")
        {
            ArgumentList = { pipe },
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
        }) ?? throw new InvalidOperationException("Could not start mkfifo.");
        process.WaitForExit();
        Assert.That(process.ExitCode, Is.Zero, process.StandardError.ReadToEnd());

        var result = _probe.Probe(pipe);
        Assert.That(result.Failure, Is.EqualTo(InputSelectionFailureReason.WrongKind));
    }

    [Test]
    [Platform("Win")]
    public void AcceptsWindowsJunctionToExistingDirectory()
    {
        var target = Path.Combine(_folder, "junction-target");
        var junction = Path.Combine(_folder, "junction");
        Directory.CreateDirectory(target);
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd.exe")
        {
            ArgumentList = { "/c", "mklink", "/J", junction, target },
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        }) ?? throw new InvalidOperationException("Could not start mklink.");
        process.WaitForExit();
        Assert.That(process.ExitCode, Is.Zero, process.StandardError.ReadToEnd());

        var result = _probe.Probe(junction);
        Assert.That(result.Kind, Is.EqualTo(InputKind.Directory));
    }

    private static void CreateFileSymlinkOrIgnore(string link, string target)
    {
        try
        {
            File.CreateSymbolicLink(link, target);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            Assert.Ignore($"Host lacks symlink creation permission: {exception.GetType().Name}");
        }
    }

    private static void CreateDirectorySymlinkOrIgnore(string link, string target)
    {
        try
        {
            Directory.CreateSymbolicLink(link, target);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            Assert.Ignore($"Host lacks symlink creation permission: {exception.GetType().Name}");
        }
    }
}
