using System.Security;
using Infoconex.DotNetBinary.Application.DeploymentConfiguration;
using Infoconex.DotNetBinary.Domain.Models;
using Infoconex.DotNetBinary.Infrastructure.InputSelection;

namespace Infoconex.DotNetBinary.Infrastructure.DeploymentConfiguration;

/// <summary>Reads a bounded, regular deployment configuration file as untrusted data.</summary>
public sealed class FileDeploymentConfigurationReader : IDeploymentConfigurationReader
{
    private const int BufferSize = 16 * 1024;

    /// <inheritdoc />
    public DeploymentConfigurationResult Read(string path, DeploymentConfigurationAssociation association, long maxFileBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (maxFileBytes <= 0 || maxFileBytes > 4 * 1024 * 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFileBytes));
        }

        try
        {
            if (!RegularFileTypeProbe.IsRegularFile(path))
            {
                return Failure(DeploymentConfigurationOutcome.Unreadable, path, association, "NotRegularFile");
            }

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete, BufferSize, FileOptions.SequentialScan);
            if (stream.Length > maxFileBytes)
            {
                return Failure(DeploymentConfigurationOutcome.ResourceLimited, path, association, "ConfigurationSizeLimit");
            }

            using var bytes = new MemoryStream();
            var buffer = new byte[BufferSize];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                if (bytes.Length + count > maxFileBytes)
                {
                    return Failure(DeploymentConfigurationOutcome.ResourceLimited, path, association, "ConfigurationSizeLimit");
                }

                bytes.Write(buffer, 0, count);
            }

            if (stream.Length != bytes.Length)
            {
                return Failure(DeploymentConfigurationOutcome.Unreadable, path, association, "ConfigurationChanged");
            }

            var content = bytes.ToArray();
            if (path.EndsWith(".exe.config", StringComparison.OrdinalIgnoreCase))
            {
                return FrameworkConfigurationParser.Parse(content, path, association, maxFileBytes);
            }

            if (path.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase))
            {
                return ModernRuntimeConfigurationParser.Parse(content, path, association);
            }

            return Failure(DeploymentConfigurationOutcome.Unsupported, path, association, "UnsupportedConfigurationFilename");
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Failure(DeploymentConfigurationOutcome.NotFound, path, association, "ConfigurationNotFound");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException or
            NotSupportedException or ArgumentException)
        {
            return Failure(DeploymentConfigurationOutcome.Unreadable, path, association, "ConfigurationReadFailure");
        }
    }

    private static DeploymentConfigurationResult Failure(
        DeploymentConfigurationOutcome outcome, string path, DeploymentConfigurationAssociation association, string diagnostic) =>
        new(outcome, path, association, [], [], [new InspectionDiagnostic(diagnostic, "File")]);
}
