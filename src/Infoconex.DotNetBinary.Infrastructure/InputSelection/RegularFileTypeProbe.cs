using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Infoconex.DotNetBinary.Infrastructure.InputSelection;

/// <summary>
/// Distinguishes ordinary files from pipes, sockets and other special nodes
/// without reading a byte of the selected candidate.
/// </summary>
internal static partial class RegularFileTypeProbe
{
    // Linux O_PATH obtains an inode-only descriptor; no candidate device is opened
    // for input. macOS has no O_PATH; O_NONBLOCK avoids waiting for FIFO writers.
    private const int LinuxOpenPath = 0x200000;
    private const int MacOpenNonBlocking = 0x0004;

    [LibraryImport("libc", EntryPoint = "open", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int OpenUnixPath(string path, int flags);

    /// <summary>Checks the OS file-handle kind without parsing or reading file contents.</summary>
    public static bool IsRegularFile(string path)
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var flags = OperatingSystem.IsLinux() ? LinuxOpenPath : MacOpenNonBlocking;
            var descriptor = OpenUnixPath(path, flags);
            if (descriptor < 0)
            {
                var error = Marshal.GetLastPInvokeError();
                throw error switch
                {
                    2 => new FileNotFoundException("The selected input no longer exists."),
                    13 => new UnauthorizedAccessException("The selected input cannot be accessed."),
                    _ => new IOException($"Could not obtain input file type (OS error {error})."),
                };
            }

            using var handle = new SafeFileHandle((nint)descriptor, ownsHandle: true);
            return handle.Type == FileHandleType.RegularFile;
        }

        // Windows gets the same file-type classification through its native handle.
        using var windowsHandle = File.OpenHandle(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete, FileOptions.Asynchronous);
        return windowsHandle.Type == FileHandleType.RegularFile;
    }
}
