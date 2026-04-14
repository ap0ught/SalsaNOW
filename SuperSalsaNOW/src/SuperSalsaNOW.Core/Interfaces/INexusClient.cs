namespace SuperSalsaNOW.Core.Interfaces;

using SuperSalsaNOW.Core.Models;

/// <summary>
/// Client for interacting with Nexus Mods API
/// </summary>
public interface INexusClient
{
    /// <summary>
    /// Get list of available files for a mod
    /// </summary>
    /// <param name="gameDomain">Nexus game domain (e.g., "eldenring")</param>
    /// <param name="modId">Nexus mod ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available mod files</returns>
    Task<List<ModFile>> GetModFilesAsync(string gameDomain, int modId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get download links for a specific file
    /// </summary>
    /// <param name="gameDomain">Nexus game domain</param>
    /// <param name="modId">Nexus mod ID</param>
    /// <param name="fileId">Specific file ID to download</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of CDN download links (typically expire in 1 hour)</returns>
    Task<List<DownloadLink>> GetDownloadLinksAsync(string gameDomain, int modId, int fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Select best file based on pattern (e.g., "main", "latest")
    /// </summary>
    /// <param name="files">Available mod files</param>
    /// <param name="pattern">Selection pattern ("main" for main file, "latest" for newest)</param>
    /// <returns>Selected file or null if no match found</returns>
    ModFile? SelectFile(List<ModFile> files, string pattern);
}
