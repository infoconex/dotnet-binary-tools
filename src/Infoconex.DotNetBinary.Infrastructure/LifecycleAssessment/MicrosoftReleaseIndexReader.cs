using System.Globalization;
using System.Text.Json;
using Infoconex.DotNetBinary.Application.LifecycleAssessment;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

/// <summary>Strictly validates official Microsoft release-index JSON and preserves separate Framework records.</summary>
public sealed class MicrosoftReleaseIndexReader : ILifecycleReleaseIndexReader
{
    private const string SourceUrl = "https://dotnet.microsoft.com/en-us/platform/support/policy";
    private const int MaxSourceBytes = 256 * 1024;
    private const string Limitation = "Microsoft .NET major/minor support only; patch currency, operating system and execution environment are not assessed.";

    /// <inheritdoc />
    public LifecycleReferenceDataset Read(byte[] contents, LifecycleReferenceDataset current, DateOnly retrievedOn)
    {
        ArgumentNullException.ThrowIfNull(contents);
        ArgumentNullException.ThrowIfNull(current);
        if (contents.Length is 0 or > MaxSourceBytes)
        {
            throw new InvalidDataException("Invalid release index byte count.");
        }

        try
        {
            using var document = JsonDocument.Parse(contents, new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;
            RequireUniqueObject(root);
            var array = root.GetProperty("releases-index");
            if (array.ValueKind != JsonValueKind.Array || array.GetArrayLength() is < 3 or > 128)
            {
                throw new InvalidDataException("Invalid Microsoft release-index channel count.");
            }

            var entries = new List<LifecycleReferenceEntry>();
            var channels = new HashSet<string>(StringComparer.Ordinal);
            var publication = DateOnly.MinValue;
            foreach (var element in array.EnumerateArray())
            {
                RequireUniqueObject(element);
                var channel = RequiredString(element, "channel-version");
                var product = RequiredString(element, "product");
                var phase = RequiredString(element, "support-phase");
                var latestDate = ReadDate(RequiredString(element, "latest-release-date"));
                var end = ReadDate(RequiredString(element, "eol-date"));
                if (!channels.Add(channel) || !Version.TryParse(channel, out var version) ||
                    version.Minor < 0 || version.Build >= 0 || version.Revision >= 0 ||
                    version.Major < 1 || version.Major > 100 ||
                    phase is not ("active" or "maintenance" or "eol") ||
                    latestDate > retrievedOn || end.Year < 2016 ||
                    (phase == "eol" && end > retrievedOn) ||
                    (phase is "active" or "maintenance" && end <= retrievedOn))
                {
                    throw new InvalidDataException("Invalid Microsoft release-index lifecycle entry.");
                }

                var family = version.Major switch
                {
                    <= 3 when product == ".NET Core" => TargetFrameworkFamily.NetCore,
                    >= 5 when product == ".NET" && version.Minor == 0 => TargetFrameworkFamily.ModernDotNet,
                    _ => throw new InvalidDataException("Unexpected Microsoft release-index product or channel."),
                };
                if (latestDate > publication)
                {
                    publication = latestDate;
                }

                entries.Add(new LifecycleReferenceEntry(family, channel, end, false, SourceUrl, latestDate, Limitation));
            }

            if (!channels.Contains("11.0") || !channels.Contains("10.0") ||
                !channels.Contains("3.1"))
            {
                throw new InvalidDataException("Microsoft release-index coverage is incomplete.");
            }

            // The index has no Windows component-lifecycle information. Preserve the separately
            // verified Framework entries; never replace them with guesses from .NET release data.
            entries.AddRange(current.Entries.Where(entry => entry.Family == TargetFrameworkFamily.NetFramework));
            if (!entries.Any(entry => entry.Family == TargetFrameworkFamily.NetFramework))
            {
                throw new InvalidDataException("Missing independently sourced Framework reference coverage.");
            }

            var replacement = new LifecycleReferenceDataset(publication.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                retrievedOn, entries.AsReadOnly());
            _ = LifecycleDatasetCodec.Serialize(replacement);
            return replacement;
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or
            FormatException or InvalidOperationException or ArgumentOutOfRangeException)
        {
            throw new InvalidDataException("Invalid Microsoft release-index format.", exception);
        }
    }

    private static void RequireUniqueObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Expected an official release-index object.");
        }

        var fields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!fields.Add(property.Name))
            {
                throw new InvalidDataException("Duplicate JSON property in release index.");
            }
        }
    }

    private static string RequiredString(JsonElement element, string name)
    {
        var property = element.GetProperty(name);
        if (property.ValueKind != JsonValueKind.String || property.GetString() is not { Length: > 0 } value ||
            value.Length > 128)
        {
            throw new InvalidDataException("Missing or invalid Microsoft release-index field.");
        }

        return value;
    }

    private static DateOnly ReadDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date : throw new InvalidDataException("Invalid Microsoft lifecycle date.");
}
