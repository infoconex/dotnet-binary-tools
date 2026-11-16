using Infoconex.DotNetBinary.Domain.Models;

namespace Infoconex.DotNetBinary.Application.DeploymentConfiguration;

/// <summary>Outbound port for bounded filesystem and deployment configuration parsing.</summary>
public interface IDeploymentConfigurationReader
{
    /// <summary>Reads a single directly named supported file without executing it.</summary>
    /// <param name="path">Host-qualified path, never a search pattern.</param>
    /// <param name="association">How the application selected this exact file.</param>
    /// <param name="maxFileBytes">The maximum permitted source bytes.</param>
    /// <returns>Independent declared values and a classified outcome.</returns>
    DeploymentConfigurationResult Read(string path, DeploymentConfigurationAssociation association, long maxFileBytes);
}
