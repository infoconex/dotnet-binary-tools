using System.Xml;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;

/// <summary>Safely parses only documented Framework startup runtime declarations.</summary>
internal static class FrameworkConfigurationParser
{
    private const int MaxDepth = 32;
    private const int MaxRequests = 128;

    internal static DeploymentConfigurationResult Parse(
        byte[] content, string path, DeploymentConfigurationAssociation association, long maxFileBytes)
    {
        var requests = new List<DeploymentRuntimeRequest>();
        var values = new List<DeploymentConfigurationValue>();
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = Math.Min(maxFileBytes * 2, 8 * 1024 * 1024),
            MaxCharactersFromEntities = 1024,
            ConformanceLevel = ConformanceLevel.Document,
        };

        try
        {
            using var stream = new MemoryStream(content, writable: false);
            using var reader = XmlReader.Create(stream, settings);
            var rootObserved = false;
            var startupObserved = false;
            var runtimeIndex = 0;

            while (reader.Read())
            {
                if (reader.Depth > MaxDepth)
                {
                    return Failure(DeploymentConfigurationOutcome.ResourceLimited, path, association, "XmlNestingLimit");
                }

                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == 1 && reader.Name == "startup")
                {
                    startupObserved = false;
                }

                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (reader.Depth == 0)
                {
                    if (reader.Name != "configuration")
                    {
                        return Failure(DeploymentConfigurationOutcome.Unsupported, path, association, "UnsupportedXmlRoot");
                    }

                    rootObserved = true;
                }
                else if (reader.Depth == 1 && reader.Name == "startup")
                {
                    startupObserved = !reader.IsEmptyElement;
                    var activation = reader.GetAttribute("useLegacyV2RuntimeActivationPolicy");
                    if (activation is not null)
                    {
                        values.Add(new DeploymentConfigurationValue(
                            "configuration.startup.@useLegacyV2RuntimeActivationPolicy", activation));
                    }
                }
                else if (startupObserved && reader.Depth == 2 && reader.Name == "supportedRuntime")
                {
                    if (runtimeIndex == MaxRequests)
                    {
                        return Failure(DeploymentConfigurationOutcome.ResourceLimited, path, association, "RuntimeRequestLimit");
                    }

                    var source = $"configuration.startup.supportedRuntime[{runtimeIndex++}]";
                    var fields = new List<DeploymentConfigurationValue>();
                    AddAttribute(reader, source, "version", fields);
                    AddAttribute(reader, source, "sku", fields);
                    if (fields.Count != 0)
                    {
                        requests.Add(new DeploymentRuntimeRequest(source, fields.AsReadOnly()));
                    }
                }
            }

            return rootObserved && (requests.Count != 0 || values.Count != 0)
                ? new DeploymentConfigurationResult(DeploymentConfigurationOutcome.Parsed, path, association,
                    requests.AsReadOnly(), values.AsReadOnly(), [])
                : Failure(DeploymentConfigurationOutcome.Unsupported, path, association, "NoSupportedRuntimeDeclarations");
        }
        catch (XmlException)
        {
            return Failure(DeploymentConfigurationOutcome.Malformed, path, association, "InvalidXmlConfiguration");
        }
    }

    private static void AddAttribute(
        XmlReader reader, string source, string name, List<DeploymentConfigurationValue> fields)
    {
        var value = reader.GetAttribute(name);
        if (value is not null)
        {
            fields.Add(new DeploymentConfigurationValue($"{source}.@{name}", value));
        }
    }

    private static DeploymentConfigurationResult Failure(
        DeploymentConfigurationOutcome outcome, string path, DeploymentConfigurationAssociation association, string diagnostic) =>
        new(outcome, path, association, [], [], [new InspectionDiagnostic(diagnostic, "File")]);
}
