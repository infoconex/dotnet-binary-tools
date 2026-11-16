using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.ResultsReporting;

/// <summary>Coordinates one faithful inventory presentation and safe output placement.</summary>
public sealed class ReportService(IInventoryReportRenderer renderer, IReportFileStore fileStore,
    TimeProvider clock, Func<string> uniqueId) : IReportUseCase
{
    private readonly IInventoryReportRenderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    private readonly IReportFileStore _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
    private readonly TimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly Func<string> _uniqueId = uniqueId ?? throw new ArgumentNullException(nameof(uniqueId));

    /// <summary>Writes the report and returns the actual file path, or null for stdout; failures propagate.</summary>
    public string? Write(InventoryReport inventory, string command, ReportOptions options,
        TextWriter output, TextWriter diagnostics)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (command is not ("inspect" or "scan")) throw new ArgumentException("Unsupported report command.", nameof(command));
        options.Validate();

        var generatedUtc = _clock.GetUtcNow();
        string? actualPath = null;
        if (options.OutputPath is null)
        {
            _renderer.Render(inventory, command, generatedUtc, options, output);
            output.Flush();
        }
        else
        {
            var (path, writer) = CreateExclusive(options.OutputPath, generatedUtc);
            actualPath = path;
            using (writer)
            {
                _renderer.Render(inventory, command, generatedUtc, options, writer);
                writer.Flush();
            }
        }

        foreach (var reason in inventory.Completion.Reasons)
        {
            diagnostics.WriteLine($"Inventory coverage warning: {reason}.");
        }

        foreach (var candidate in inventory.Candidates)
        {
            if (candidate.Lifecycle?.DatasetWarning is not null)
            {
                diagnostics.WriteLine("Lifecycle reference warning: offline cache was bypassed.");
            }

            foreach (var finding in candidate.Lifecycle?.Findings ?? [])
            {
                if (finding.IsStale) diagnostics.WriteLine("Lifecycle warning: local reference data is stale.");
            }

            foreach (var entry in candidate.Configuration?.Diagnostics ?? [])
            {
                diagnostics.WriteLine($"Configuration diagnostic: {SafeCode(entry.Code)}.");
            }
        }

        return actualPath;
    }

    private (string Path, TextWriter Writer) CreateExclusive(string requestedPath, DateTimeOffset utc)
    {
        try
        {
            return (requestedPath, _fileStore.CreateNew(requestedPath));
        }
        catch (IOException) when (_fileStore.Exists(requestedPath))
        {
            // Only a confirmed collision permits a different name; unrelated write errors propagate.
        }

        var folder = Path.GetDirectoryName(requestedPath);
        var stem = Path.GetFileNameWithoutExtension(requestedPath);
        var extension = Path.GetExtension(requestedPath);
        var timestamp = utc.UtcDateTime.ToString("yyyyMMdd'T'HHmmssfff'Z'", System.Globalization.CultureInfo.InvariantCulture);
        for (var attempt = 0; attempt < 16; attempt++)
        {
            var suffix = _uniqueId();
            if (suffix.Length is < 8 or > 64 || !suffix.All(character =>
                character is >= 'a' and <= 'z' or >= '0' and <= '9'))
            {
                throw new InvalidOperationException("Invalid report filename identifier.");
            }

            var name = $"{stem}_{timestamp}_{suffix}{extension}";
            var candidate = string.IsNullOrEmpty(folder) ? name : Path.Combine(folder, name);
            try
            {
                return (candidate, _fileStore.CreateNew(candidate));
            }
            catch (IOException) when (_fileStore.Exists(candidate))
            {
                // Another writer won this exact name. Retry with a fresh unique suffix.
            }
        }

        throw new IOException("Unable to allocate a unique report filename after repeated collisions.");
    }

    private static string SafeCode(string code) => string.Concat(code.Take(64).Select(character =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-' ? character : '?'));
}
