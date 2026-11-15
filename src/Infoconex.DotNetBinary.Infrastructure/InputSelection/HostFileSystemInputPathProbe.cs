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

            // Exists follows valid file/directory symlinks and junctions while never enumerating.
            // Their literal qualified paths remain intact for later consumption.
            if (File.Exists(fullPath))
            {
                var attributes = File.GetAttributes(fullPath);
                return (attributes & (FileAttributes.Directory | FileAttributes.Device)) == 0
                    ? InputPathProbeResult.Found(fullPath, InputKind.File)
                    : InputPathProbeResult.Failed(InputSelectionFailureReason.Unavailable);
            }

            if (Directory.Exists(fullPath))
            {
                return InputPathProbeResult.Found(fullPath, InputKind.Directory);
            }

            // Exists intentionally suppresses errors. GetAttributes allows us to distinguish
            // missing paths, broken final-component links and reported access failures.
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
