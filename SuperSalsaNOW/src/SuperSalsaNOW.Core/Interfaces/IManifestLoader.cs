namespace SuperSalsaNOW.Core.Interfaces;

using SuperSalsaNOW.Core.Models;

/// <summary>
/// Loads manifest configurations from remote sources (e.g., GitHub raw URLs)
/// </summary>
public interface IManifestLoader
{
    /// <summary>
    /// Load complete manifest from base URL
    /// </summary>
    /// <param name="baseUrl">Base URL of manifest repository (e.g., GitHub raw content URL)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete manifest with directory, mods, and tools configuration</returns>
    Task<ManifestRoot> LoadManifestAsync(string baseUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load specific manifest file (directory.json, mods.json, tools.json)
    /// </summary>
    /// <typeparam name="T">Expected type of manifest content</typeparam>
    /// <param name="baseUrl">Base URL of manifest repository</param>
    /// <param name="fileName">Name of manifest file to load</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized manifest content</returns>
    Task<T> LoadManifestFileAsync<T>(string baseUrl, string fileName, CancellationToken cancellationToken = default);
}
