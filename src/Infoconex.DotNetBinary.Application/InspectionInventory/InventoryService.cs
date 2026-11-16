using System.Security;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.InspectionInventory;

/// <summary>Orchestrates bounded, explicit-scope inspection and independent candidate findings.</summary>
public sealed class InventoryService(IInventoryFileSystem fileSystem,
    IInventoryCandidateInspector inspector, TimeProvider clock, IInventoryCpuClock cpuClock) : IInventoryUseCase
{
    private readonly IInventoryFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly IInventoryCandidateInspector _inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
    private readonly TimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly IInventoryCpuClock _cpuClock = cpuClock ?? throw new ArgumentNullException(nameof(cpuClock));

    /// <summary>Builds one shared inventory for a validated explicit file of any extension.</summary>
    public InventoryReport Inspect(SelectedInput file, SelectedInput? explicitConfiguration = null,
        InventoryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Kind != InputKind.File) throw new ArgumentException("Inspect requires a selected file.", nameof(file));
        if (explicitConfiguration is not null && explicitConfiguration.Kind != InputKind.File)
        {
            throw new ArgumentException("Explicit configuration must be a selected file.", nameof(explicitConfiguration));
        }

        options ??= new InventoryOptions();
        options.Validate();
        var candidates = new List<InventoryCandidate>(1);
        var skipped = new List<InventorySkippedPath>();
        var reasons = new List<InventoryCompletionReason>();
        var started = _clock.GetUtcNow();
        var cpuStarted = _cpuClock.Used;
        AddCandidate(file, explicitConfiguration, options, file.FullPath, candidates, skipped, reasons);
        if (_clock.GetUtcNow() - started >= TimeSpan.FromSeconds(options.MaxElapsedSeconds))
        {
            reasons.Add(InventoryCompletionReason.ElapsedTimeLimit);
        }

        if (_cpuClock.Used - cpuStarted >= TimeSpan.FromSeconds(options.MaxCpuSeconds))
        {
            reasons.Add(InventoryCompletionReason.CpuTimeLimit);
        }

        return new InventoryReport([file], candidates, skipped, [], reasons);
    }

    /// <summary>Builds one inventory for all prevalidated directories with shared completion semantics.</summary>
    public InventoryReport Scan(IReadOnlyList<SelectedInput> roots, InventoryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(roots);
        if (roots.Count == 0 || roots.Any(root => root is null || root.Kind != InputKind.Directory))
        {
            throw new ArgumentException("Scan requires one or more validated directories.", nameof(roots));
        }

        options ??= new InventoryOptions();
        options.Validate();
        var candidates = new List<InventoryCandidate>();
        var skipped = new List<InventorySkippedPath>();
        var errors = new List<InventoryTraversalError>();
        var reasons = new List<InventoryCompletionReason>();
        var visitedDirectories = new HashSet<string>(_fileSystem.PathComparer);
        var visitedFiles = new HashSet<string>(_fileSystem.PathComparer);
        var pending = new Queue<DirectoryWork>();
        var maxDirectoryEvents = Math.Min(100_000, Math.Max(1024, options.MaxCandidates * 4));
        var started = _clock.GetUtcNow();
        var cpuStarted = _cpuClock.Used;
        foreach (var root in roots)
        {
            if (pending.Count + errors.Count + skipped.Count >= maxDirectoryEvents)
            {
                reasons.Add(InventoryCompletionReason.DirectoryLimit);
                break;
            }

            try
            {
                pending.Enqueue(new DirectoryWork(root.FullPath,
                    _fileSystem.GetPhysicalDirectoryPath(root.FullPath), root.FullPath, 0));
            }
            catch (Exception exception) when (IsRecoverableFileSystemFailure(exception))
            {
                errors.Add(new InventoryTraversalError(root.FullPath, root.FullPath, ErrorCode(exception)));
            }
        }

        var stopped = false;
        while (pending.Count != 0 && !stopped)
        {
            if (_clock.GetUtcNow() - started >= TimeSpan.FromSeconds(options.MaxElapsedSeconds))
            {
                reasons.Add(InventoryCompletionReason.ElapsedTimeLimit);
                break;
            }

            if (_cpuClock.Used - cpuStarted >= TimeSpan.FromSeconds(options.MaxCpuSeconds))
            {
                reasons.Add(InventoryCompletionReason.CpuTimeLimit);
                break;
            }

            var directory = pending.Dequeue();
            if (!visitedDirectories.Add(directory.PhysicalPath))
            {
                skipped.Add(new InventorySkippedPath(directory.Root, directory.LogicalPath,
                    InventorySkipReason.DuplicateDirectory));
                continue;
            }

            try
            {
                foreach (var entry in _fileSystem.EnumerateDirectory(directory.LogicalPath, directory.PhysicalPath))
                {
                    if (_clock.GetUtcNow() - started >= TimeSpan.FromSeconds(options.MaxElapsedSeconds))
                    {
                        reasons.Add(InventoryCompletionReason.ElapsedTimeLimit);
                        stopped = true;
                        break;
                    }

                    if (_cpuClock.Used - cpuStarted >= TimeSpan.FromSeconds(options.MaxCpuSeconds))
                    {
                        reasons.Add(InventoryCompletionReason.CpuTimeLimit);
                        stopped = true;
                        break;
                    }

                    if (skipped.Count + errors.Count + visitedDirectories.Count + pending.Count >= maxDirectoryEvents)
                    {
                        reasons.Add(InventoryCompletionReason.DirectoryLimit);
                        stopped = true;
                        break;
                    }

                    if (entry.IsDirectory)
                    {
                        if (!options.Recursive) continue;
                        if (entry.IsLink && !options.FollowLinks)
                        {
                            skipped.Add(new InventorySkippedPath(directory.Root, entry.Path, InventorySkipReason.LinkedDirectory));
                        }
                        else if (directory.Depth >= options.MaxDepth)
                        {
                            skipped.Add(new InventorySkippedPath(directory.Root, entry.Path, InventorySkipReason.DepthLimit));
                            reasons.Add(InventoryCompletionReason.DepthLimit);
                        }
                        else
                        {
                            pending.Enqueue(new DirectoryWork(entry.Path, entry.PhysicalPath, directory.Root, directory.Depth + 1));
                        }

                        continue;
                    }

                    if (!IsCandidateExtension(entry.Path)) continue;
                    if (!visitedFiles.Add(entry.PhysicalPath))
                    {
                        skipped.Add(new InventorySkippedPath(directory.Root, entry.Path, InventorySkipReason.DuplicateCandidate));
                        continue;
                    }

                    if (candidates.Count >= options.MaxCandidates)
                    {
                        skipped.Add(new InventorySkippedPath(directory.Root, entry.Path, InventorySkipReason.CandidateLimit));
                        reasons.Add(InventoryCompletionReason.CandidateLimit);
                        stopped = true;
                        break;
                    }

                    var selected = new SelectedInput(entry.Path, entry.Path, InputKind.File);
                    AddCandidate(selected, null, options, directory.Root, candidates, skipped, reasons, entry.FileLength);
                }
            }
            catch (Exception exception) when (IsRecoverableFileSystemFailure(exception))
            {
                errors.Add(new InventoryTraversalError(directory.Root, directory.LogicalPath, ErrorCode(exception)));
            }
        }

        return new InventoryReport(roots, candidates, skipped, errors, reasons);
    }

    private void AddCandidate(SelectedInput selected, SelectedInput? configuration,
        InventoryOptions options, string root, List<InventoryCandidate> candidates,
        List<InventorySkippedPath> skipped, List<InventoryCompletionReason> reasons,
        long? knownFileLength = null)
    {
        try
        {
            var length = knownFileLength ?? _fileSystem.GetFileLength(selected.FullPath);
            if (length > options.MaxFileBytes)
            {
                var limited = new AssemblyInspectionResult(AssemblyInspectionOutcome.ResourceLimited, null, null, null,
                    [new InspectionDiagnostic("FileSizeLimit", "File")]);
                candidates.Add(new InventoryCandidate(selected, new InventoryEvidence(limited, null, null)));
                skipped.Add(new InventorySkippedPath(root, selected.FullPath, InventorySkipReason.FileSizeLimit));
                reasons.Add(InventoryCompletionReason.FileSizeLimit);
                return;
            }

            candidates.Add(new InventoryCandidate(selected,
                _inspector.Inspect(selected, configuration, options.MaxFileBytes, options.ComputeSha256)));
        }
        catch (Exception exception) when (IsRecoverableFileSystemFailure(exception) ||
            exception is BadImageFormatException or InvalidDataException)
        {
            var unreadable = new AssemblyInspectionResult(AssemblyInspectionOutcome.Unreadable, null, null, null,
                [new InspectionDiagnostic("CandidateReadFailure", "File")]);
            candidates.Add(new InventoryCandidate(selected, new InventoryEvidence(unreadable, null, null)));
        }
    }

    private static bool IsCandidateExtension(string path)
    {
        if (path.Length < 4 || path[^4] != '.') return false;
        return ExtensionEquals(path.AsSpan(path.Length - 3), "dll") ||
            ExtensionEquals(path.AsSpan(path.Length - 3), "exe");

        static bool ExtensionEquals(ReadOnlySpan<char> actual, ReadOnlySpan<char> expected)
        {
            for (var index = 0; index < 3; index++)
            {
                var character = actual[index];
                if (character is >= 'A' and <= 'Z') character = (char)(character + ('a' - 'A'));
                if (character != expected[index]) return false;
            }

            return true;
        }
    }

    private static bool IsRecoverableFileSystemFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or SecurityException or
            ArgumentException or NotSupportedException;

    private static string ErrorCode(Exception exception) => exception switch
    {
        UnauthorizedAccessException or SecurityException => "AccessDenied",
        DirectoryNotFoundException or FileNotFoundException => "PathDisappeared",
        _ => "TraversalFailure",
    };

    private sealed record DirectoryWork(string LogicalPath, string PhysicalPath, string Root, int Depth);
}
