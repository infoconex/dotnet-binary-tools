using Infoconex.DotNetBinary.Application.InputSelection;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.InputSelection;

/// <summary>
/// Applies the host .NET filesystem path and existence contracts to literal selected paths.
/// It does not read candidate contents or enumerate any directories.
/// </summary>
public sealed class HostFileSystemInputPathProbe : IInputPathProbe
{
    /// <inheritdoc />
    public InputPathProbeResult Probe(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return InputPathProbeResult.Failed(InputSelectionFailureReason.InvalidPath);
        }

        try
        {
            var fullPath = Path.GetFullPath(path);

            // Exists can report a dangling symbolic link as present on some hosts.
            // Resolve only directly selected links to verify their final targets.
            // Preserve the literal qualified selection path for downstream consumers.
            if (File.Exists(fullPath))
            {
                var target = File.ResolveLinkTarget(fullPath, returnFinalTarget: true);
                return target is null || File.Exists(target.FullName)
                    ? InputPathProbeResult.Found(fullPath, InputKind.File)
                    : InputPathProbeResult.Failed(InputSelectionFailureReason.BrokenLink);
            }

            if (Directory.Exists(fullPath))
            {
                var target = Directory.ResolveLinkTarget(fullPath, returnFinalTarget: true);
                return target is null || Directory.Exists(target.FullName)
                    ? InputPathProbeResult.Found(fullPath, InputKind.Directory)
                    : InputPathProbeResult.Failed(InputSelectionFailureReason.BrokenLink);
            }

            // Path.Exists also recognizes special nodes and broken final-component links.
            // Neither operation opens a file or traverses a selected directory.
            if (Path.Exists(fullPath))
            {
                var attributes = File.GetAttributes(fullPath);
                return (attributes & FileAttributes.ReparsePoint) != 0
                    ? InputPathProbeResult.Failed(InputSelectionFailureReason.BrokenLink)
                    : InputPathProbeResult.Failed(InputSelectionFailureReason.WrongKind);
            }

            // Exists deliberately suppresses some errors. GetAttributes can expose access
            // failures, or an explicitly selected link whose destination disappeared.
            var unresolvedAttributes = File.GetAttributes(fullPath);
            return (unresolvedAttributes & FileAttributes.ReparsePoint) != 0
                ? InputPathProbeResult.Failed(InputSelectionFailureReason.BrokenLink)
                : InputPathProbeResult.Failed(InputSelectionFailureReason.Inaccessible);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return InputPathProbeResult.Failed(InputSelectionFailureReason.InvalidPath);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return InputPathProbeResult.Failed(InputSelectionFailureReason.Missing);
        }
        catch (UnauthorizedAccessException)
        {
            return InputPathProbeResult.Failed(InputSelectionFailureReason.Inaccessible);
        }
        catch (IOException)
        {
            return InputPathProbeResult.Failed(InputSelectionFailureReason.Unavailable);
        }
        catch (System.Security.SecurityException)
        {
            return InputPathProbeResult.Failed(InputSelectionFailureReason.Inaccessible);
        }
    }
}
