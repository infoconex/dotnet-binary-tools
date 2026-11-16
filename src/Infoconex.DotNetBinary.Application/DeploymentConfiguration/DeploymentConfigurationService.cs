using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.DeploymentConfiguration;

/// <summary>Coordinates constrained deployment configuration association and interpretation.</summary>
public sealed class DeploymentConfigurationService(IDeploymentConfigurationReader reader)
{
    private const long DefaultMaxFileBytes = 1024 * 1024;
    private const long AbsoluteMaxFileBytes = 4 * 1024 * 1024;
    private readonly IDeploymentConfigurationReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    /// <summary>Finds only exact associated configurations, or reads one validated explicit file.</summary>
    /// <param name="input">An individual CAP-001 validated selected file.</param>
    /// <param name="explicitConfiguration">A separately CAP-001 validated selected config file, if supplied.</param>
    /// <param name="maxFileBytes">Bounded resource budget in bytes, defaulting to one MiB.</param>
    /// <returns>A classified, provenance-preserving result independent of assembly targets.</returns>
    public DeploymentConfigurationResult Inspect(
        SelectedInput input, SelectedInput? explicitConfiguration = null, long maxFileBytes = DefaultMaxFileBytes)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Kind != InputKind.File)
        {
            throw new ArgumentException("Deployment configuration inspection requires a selected file.", nameof(input));
        }

        if (maxFileBytes <= 0 || maxFileBytes > AbsoluteMaxFileBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFileBytes));
        }

        if (explicitConfiguration is not null)
        {
            if (explicitConfiguration.Kind != InputKind.File)
            {
                throw new ArgumentException("Explicit configuration must be a validated file.", nameof(explicitConfiguration));
            }

            if (!IsSupportedConfigurationName(explicitConfiguration.FullPath))
            {
                return new DeploymentConfigurationResult(DeploymentConfigurationOutcome.Unsupported,
                    explicitConfiguration.FullPath, DeploymentConfigurationAssociation.Explicit, [], [],
                    [new InspectionDiagnostic("UnsupportedConfigurationFilename", "File")]);
            }

            return _reader.Read(explicitConfiguration.FullPath, DeploymentConfigurationAssociation.Explicit, maxFileBytes);
        }

        var path = input.FullPath;
        if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var framework = _reader.Read(path + ".config", DeploymentConfigurationAssociation.Automatic, maxFileBytes);
            if (framework.Outcome != DeploymentConfigurationOutcome.NotFound)
            {
                return framework;
            }
        }

        if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            var modernPath = path[..^4] + ".runtimeconfig.json";
            var modern = _reader.Read(modernPath, DeploymentConfigurationAssociation.Automatic, maxFileBytes);
            if (modern.Outcome != DeploymentConfigurationOutcome.NotFound)
            {
                return modern;
            }
        }

        return new DeploymentConfigurationResult(DeploymentConfigurationOutcome.NotFound, null, null, [], [], []);
    }

    private static bool IsSupportedConfigurationName(string path) =>
        path.EndsWith(".exe.config", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase);
}
