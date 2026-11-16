using System.Globalization;
using System.Text.Json;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.LifecycleAssessment;

/// <summary>Reads and writes the fixed, validated, Native AOT-safe lifecycle reference format.</summary>
public static class LifecycleDatasetCodec
{
    /// <summary>Maximum accepted dataset size, before JSON parsing.</summary>
    public const int MaxDatasetBytes = 128 * 1024;

    /// <summary>Reads one trusted dataset snapshot, rejecting malformed or ambiguous entries.</summary>
    public static LifecycleReferenceDataset Parse(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length == 0 || bytes.Length > MaxDatasetBytes)
        {
            throw new InvalidDataException("Lifecycle reference data exceeds supported size or is empty.");
        }

        try
        {
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;
            RequireObjectWithUniqueProperties(root);
            if (root.GetProperty("schemaVersion").GetInt32() != 1)
            {
                throw new InvalidDataException("Unknown lifecycle dataset schema version.");
            }

            var version = RequiredString(root, "version");
            var versionDate = ReadDate(version);
            var retrieved = ReadDate(RequiredString(root, "retrievedOn"));
            if (versionDate > retrieved)
            {
                throw new InvalidDataException("Lifecycle snapshot cannot predate its declared publication.");
            }

            var array = root.GetProperty("entries");
            if (array.ValueKind != JsonValueKind.Array || array.GetArrayLength() is < 1 or > 128)
            {
                throw new InvalidDataException("Invalid lifecycle entry count.");
            }

            var entries = new List<LifecycleReferenceEntry>(array.GetArrayLength());
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var element in array.EnumerateArray())
            {
                RequireObjectWithUniqueProperties(element);
                var family = RequiredString(element, "family") switch
                {
                    "NetFramework" => TargetFrameworkFamily.NetFramework,
                    "NetCore" => TargetFrameworkFamily.NetCore,
                    "ModernDotNet" => TargetFrameworkFamily.ModernDotNet,
                    _ => throw new InvalidDataException("Unrecognized lifecycle reference family."),
                };
                var release = RequiredString(element, "version");
                if (!Version.TryParse(release, out var parsed) || parsed.Minor < 0 || parsed.Revision >= 0 ||
                    (parsed.Build >= 0 && !(family == TargetFrameworkFamily.NetFramework && parsed.Major == 4 &&
                        ((parsed.Minor is >= 5 and <= 7 && parsed.Build is 1 or 2) ||
                            (parsed.Minor == 8 && parsed.Build == 1)))))
                {
                    throw new InvalidDataException("Invalid lifecycle reference version.");
                }

                if (!identities.Add($"{family}:{release}"))
                {
                    throw new InvalidDataException("Duplicate lifecycle reference entry.");
                }

                var hostDependent = element.GetProperty("hostDependent");
                if (hostDependent.ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
                    (hostDependent.GetBoolean() && family != TargetFrameworkFamily.NetFramework))
                {
                    throw new InvalidDataException("Invalid lifecycle applicability qualification.");
                }

                var source = RequiredString(element, "source");
                if (source is not ("https://dotnet.microsoft.com/en-us/platform/support/policy" or
                    "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework" or
                    "https://learn.microsoft.com/en-us/lifecycle/faq/dotnet-framework"))
                {
                    throw new InvalidDataException("Unapproved lifecycle reference source.");
                }

                var end = OptionalDate(element, "endOfSupport");
                var published = OptionalDate(element, "publishedOn");
                if (published is not null && published > retrieved)
                {
                    throw new InvalidDataException("Lifecycle source publication cannot follow retrieval.");
                }

                var limitation = RequiredString(element, "limitation");
                if (limitation.Length > 512)
                {
                    throw new InvalidDataException("Lifecycle limitation is too long.");
                }

                entries.Add(new LifecycleReferenceEntry(family, release, end, hostDependent.GetBoolean(),
                    source, published, limitation));
            }

            return new LifecycleReferenceDataset(version, retrieved, entries.AsReadOnly());
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or
            FormatException or InvalidOperationException or ArgumentOutOfRangeException)
        {
            throw new InvalidDataException("Invalid lifecycle dataset format.", exception);
        }
    }

    /// <summary>Writes and validates a dataset without serializer reflection or ambiguous JSON fields.</summary>
    public static byte[] Serialize(LifecycleReferenceDataset dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", 1);
            writer.WriteString("version", dataset.Version);
            writer.WriteString("retrievedOn", Date(dataset.RetrievedOn));
            writer.WriteStartArray("entries");
            foreach (var entry in dataset.Entries)
            {
                writer.WriteStartObject();
                writer.WriteString("family", entry.Family.ToString());
                writer.WriteString("version", entry.Version);
                if (entry.EndOfSupport is { } end)
                {
                    writer.WriteString("endOfSupport", Date(end));
                }

                writer.WriteBoolean("hostDependent", entry.HostDependent);
                writer.WriteString("source", entry.Source);
                if (entry.PublishedOn is { } published)
                {
                    writer.WriteString("publishedOn", Date(published));
                }

                writer.WriteString("limitation", entry.Limitation);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        var bytes = stream.ToArray();
        _ = Parse(bytes);
        return bytes;
    }

    private static void RequireObjectWithUniqueProperties(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("Expected a reference JSON object.");
        }

        var fields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!fields.Add(property.Name))
            {
                throw new InvalidDataException("Repeated JSON property in lifecycle reference data.");
            }
        }
    }

    private static string RequiredString(JsonElement element, string field)
    {
        var value = element.GetProperty(field);
        if (value.ValueKind != JsonValueKind.String || value.GetString() is not { Length: > 0 } text ||
            text.Length > 1024)
        {
            throw new InvalidDataException("Missing or invalid lifecycle reference field.");
        }

        return text;
    }

    private static DateOnly? OptionalDate(JsonElement element, string field) =>
        element.TryGetProperty(field, out var value) ? ReadDate(value.GetString() ?? "") : null;

    private static DateOnly ReadDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date : throw new InvalidDataException("Invalid lifecycle reference date.");

    private static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
