using System.Globalization;
using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.LifecycleAssessment;

/// <summary>Produces offline advisories separately for CAP-002 targets and CAP-003 requests.</summary>
public sealed class LifecycleAssessmentService(ILifecycleDatasetStore store, TimeProvider clock)
{
    private const int StaleAfterDays = 90;
    private const string UnknownLimitation = "No applicable published runtime-support lifecycle entry; actual compatibility or runtime presence cannot be inferred.";
    private readonly ILifecycleDatasetStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly TimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>Assesses only explicitly evidenced declarations using locally available references.</summary>
    /// <param name="declaredTarget">Optional CAP-002 declared target.</param>
    /// <param name="configuration">Optional CAP-003 configuration evidence.</param>
    /// <returns>Independent advisory findings; no command exit or compliance decision.</returns>
    public LifecycleAssessmentResult Assess(TargetFrameworkIdentity? declaredTarget,
        DeploymentConfigurationResult? configuration)
    {
        var loaded = _store.Load();
        var dataset = loaded.Dataset;
        var assessmentDate = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
        var age = assessmentDate.DayNumber - dataset.RetrievedOn.DayNumber;
        var stale = age > StaleAfterDays || age < 0;
        var findings = new List<LifecycleFinding>();

        if (declaredTarget is not null)
        {
            findings.Add(Evaluate(LifecycleEvidenceKind.AssemblyTarget, declaredTarget.Moniker,
                declaredTarget.EvidenceSource, declaredTarget.Family, declaredTarget.Version,
                dataset, assessmentDate, stale));
        }

        if (configuration is not null)
        {
            foreach (var request in configuration.RuntimeRequests)
            {
                var (family, version, identity) = ClassifyRequest(request);
                findings.Add(Evaluate(LifecycleEvidenceKind.ConfiguredRuntime, identity,
                    $"{configuration.Path ?? "configuration"}:{request.Origin}", family, version,
                    dataset, assessmentDate, stale));
            }
        }

        return new LifecycleAssessmentResult(findings.AsReadOnly(), loaded.Warning);
    }

    private static LifecycleFinding Evaluate(LifecycleEvidenceKind evidenceKind, string identity,
        string evidenceSource, TargetFrameworkFamily family, Version? version,
        LifecycleReferenceDataset dataset, DateOnly date, bool stale)
    {
        if (family == TargetFrameworkFamily.NetStandard)
        {
            return Finding(LifecycleStatus.NotApplicable, null, null,
                " .NET Standard is an API specification, not a runtime with its own support lifecycle.");
        }

        var entry = dataset.Entries.FirstOrDefault(reference => reference.Family == family &&
            version is not null && reference.Version == VersionKey(family, version));
        if (entry is null)
        {
            return Finding(LifecycleStatus.Unknown, null, null, UnknownLimitation);
        }

        var status = entry.EndOfSupport is { } end && date >= end
            ? LifecycleStatus.OutOfSupport
            : entry.HostDependent || entry.EndOfSupport is null ? LifecycleStatus.Unknown : LifecycleStatus.Supported;
        return Finding(status, entry.EndOfSupport, entry, entry.Limitation);

        LifecycleFinding Finding(LifecycleStatus status, DateOnly? endOfSupport,
            LifecycleReferenceEntry? entry, string limitation) => new(evidenceKind, identity,
            evidenceSource, family, status, endOfSupport, date, dataset.Version, entry?.Source,
            entry?.PublishedOn, dataset.RetrievedOn, stale, limitation.Trim());
    }

    private static string VersionKey(TargetFrameworkFamily family, Version version)
    {
        if (family == TargetFrameworkFamily.NetFramework && version.Build >= 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{version.Major}.{version.Minor}.{version.Build}");
        }

        return string.Create(CultureInfo.InvariantCulture, $"{version.Major}.{version.Minor}");
    }

    private static (TargetFrameworkFamily Family, Version? Version, string Identity) ClassifyRequest(
        DeploymentRuntimeRequest request)
    {
        var name = request.Values.FirstOrDefault(value => value.Field.EndsWith(".name", StringComparison.Ordinal))?.RawValue;
        var version = request.Values.FirstOrDefault(value => value.Field.EndsWith(".version", StringComparison.Ordinal) ||
            value.Field.EndsWith(".@version", StringComparison.Ordinal))?.RawValue;
        var sku = request.Values.FirstOrDefault(value => value.Field.EndsWith(".@sku", StringComparison.Ordinal))?.RawValue;
        if (sku is not null)
        {
            var framework = new TargetFrameworkIdentity(sku);
            return (framework.Family, framework.Version, sku);
        }

        var identity = name is not null ? $"{name} {version ?? "(version not declared)"}" :
            version ?? "(runtime version not declared)";
        if (name is not ("Microsoft.NETCore.App" or "Microsoft.AspNetCore.App" or "Microsoft.WindowsDesktop.App") ||
            version is null || !Version.TryParse(version, out var parsed) || parsed.Minor < 0 ||
            parsed.Revision >= 0 || parsed.Major < 1)
        {
            return (TargetFrameworkFamily.Unrecognized, null, identity);
        }

        var family = parsed.Major switch
        {
            <= 3 => TargetFrameworkFamily.NetCore,
            >= 5 and <= 11 when parsed.Minor == 0 => TargetFrameworkFamily.ModernDotNet,
            _ => TargetFrameworkFamily.Unrecognized,
        };
        return (family, family == TargetFrameworkFamily.Unrecognized ? null : parsed, identity);
    }
}
