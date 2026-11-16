namespace Infoconex.DotNetBinary.Application.AssemblyInspection;

/// <summary>Explicit inspection policy, including opt-in hashing and a bounded file size.</summary>
/// <param name="ComputeSha256">Hash the inspected bytes only when requested.</param>
/// <param name="MaxFileBytes">Maximum file bytes permitted for reading or hashing.</param>
public sealed record AssemblyInspectionOptions(bool ComputeSha256 = false, long MaxFileBytes = 512L * 1024 * 1024)
{
    /// <summary>Ensures a policy never permits an unbounded or negative input.</summary>
    public void Validate()
    {
        if (MaxFileBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxFileBytes));
        }
    }
}
