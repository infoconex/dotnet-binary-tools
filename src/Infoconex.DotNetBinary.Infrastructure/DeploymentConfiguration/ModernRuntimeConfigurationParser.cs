using System.Text.Json;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;

/// <summary>Parses documented modern runtime request fields without choosing a runtime.</summary>
internal static class ModernRuntimeConfigurationParser
{
    private const int MaxDepth = 32;
    private const int MaxFrameworkRequests = 128;

    internal static DeploymentConfigurationResult Parse(
        byte[] content, string path, DeploymentConfigurationAssociation association)
    {
        try
        {
            using var document = JsonDocument.Parse(content, new JsonDocumentOptions
            {
                MaxDepth = MaxDepth,
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
            });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !HasUniqueProperties(root) ||
                !root.TryGetProperty("runtimeOptions", out var options) || options.ValueKind != JsonValueKind.Object ||
                !HasUniqueProperties(options))
            {
                return Failure(DeploymentConfigurationOutcome.Unsupported, path, association, "UnsupportedRuntimeOptions");
            }

            var values = new List<DeploymentConfigurationValue>();
            var requests = new List<DeploymentRuntimeRequest>();
            if (!ReadString(options, "runtimeOptions", "tfm", values) ||
                !ReadPolicy(options, "runtimeOptions", "rollForward", values) ||
                !ReadPolicy(options, "runtimeOptions", "applyPatches", values) ||
                !ReadPolicy(options, "runtimeOptions", "rollForwardOnNoCandidateFx", values))
            {
                return UnsupportedWithEvidence(path, association, "UnsupportedRuntimeField", requests, values);
            }

            if (options.TryGetProperty("framework", out var framework) &&
                !TryReadFramework(framework, "runtimeOptions.framework", requests))
            {
                return UnsupportedWithEvidence(path, association, "UnsupportedFrameworkStructure", requests, values);
            }

            if (options.TryGetProperty("frameworks", out var frameworks))
            {
                if (frameworks.ValueKind != JsonValueKind.Array)
                {
                    return UnsupportedWithEvidence(path, association, "UnsupportedFrameworksStructure", requests, values);
                }

                if (frameworks.GetArrayLength() > MaxFrameworkRequests)
                {
                    return Failure(DeploymentConfigurationOutcome.ResourceLimited, path, association, "RuntimeRequestLimit");
                }

                for (var index = 0; index < frameworks.GetArrayLength(); index++)
                {
                    if (!TryReadFramework(frameworks[index], $"runtimeOptions.frameworks[{index}]", requests))
                    {
                        return UnsupportedWithEvidence(path, association, "UnsupportedFrameworksStructure", requests, values);
                    }
                }
            }

            return values.Count != 0 || requests.Count != 0
                ? new DeploymentConfigurationResult(DeploymentConfigurationOutcome.Parsed, path, association,
                    requests.AsReadOnly(), values.AsReadOnly(), [])
                : Failure(DeploymentConfigurationOutcome.Unsupported, path, association, "NoSupportedRuntimeDeclarations");
        }
        catch (JsonException)
        {
            return Failure(DeploymentConfigurationOutcome.Malformed, path, association, "InvalidJsonOrDepthLimit");
        }
    }

    private static bool TryReadFramework(
        JsonElement element, string origin, List<DeploymentRuntimeRequest> requests)
    {
        if (element.ValueKind != JsonValueKind.Object || !HasUniqueProperties(element))
        {
            return false;
        }

        var fields = new List<DeploymentConfigurationValue>();
        if (!ReadString(element, origin, "name", fields) ||
            !ReadString(element, origin, "version", fields) ||
            !ReadPolicy(element, origin, "rollForward", fields) ||
            !ReadPolicy(element, origin, "applyPatches", fields) ||
            !ReadPolicy(element, origin, "rollForwardOnNoCandidateFx", fields))
        {
            return false;
        }

        if (fields.Count != 0)
        {
            requests.Add(new DeploymentRuntimeRequest(origin, fields.AsReadOnly()));
        }

        return true;
    }

    private static bool ReadString(
        JsonElement element, string origin, string name, List<DeploymentConfigurationValue> values)
    {
        if (!element.TryGetProperty(name, out var property))
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        values.Add(new DeploymentConfigurationValue($"{origin}.{name}", property.GetString()!));
        return true;
    }

    private static bool ReadPolicy(
        JsonElement element, string origin, string name, List<DeploymentConfigurationValue> values)
    {
        if (!element.TryGetProperty(name, out var property))
        {
            return true;
        }

        var supported = name switch
        {
            "applyPatches" => property.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "rollForwardOnNoCandidateFx" => property.ValueKind == JsonValueKind.Number,
            _ => property.ValueKind == JsonValueKind.String,
        };
        if (!supported)
        {
            return false;
        }

        values.Add(new DeploymentConfigurationValue($"{origin}.{name}",
            property.ValueKind == JsonValueKind.String ? property.GetString()! : property.GetRawText()));
        return true;
    }

    private static bool HasUniqueProperties(JsonElement element)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!seen.Add(property.Name))
            {
                return false;
            }
        }

        return true;
    }

    private static DeploymentConfigurationResult Failure(
        DeploymentConfigurationOutcome outcome, string path, DeploymentConfigurationAssociation association, string diagnostic) =>
        new(outcome, path, association, [], [], [new InspectionDiagnostic(diagnostic, "File")]);

    private static DeploymentConfigurationResult UnsupportedWithEvidence(
        string path, DeploymentConfigurationAssociation association, string diagnostic,
        List<DeploymentRuntimeRequest> requests, List<DeploymentConfigurationValue> values) =>
        new(DeploymentConfigurationOutcome.Unsupported, path, association,
            requests.AsReadOnly(), values.AsReadOnly(), [new InspectionDiagnostic(diagnostic, "runtimeOptions")]);
}
