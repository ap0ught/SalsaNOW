namespace SuperSalsaNOW.Core.Services;

using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Loads manifest files from GitHub raw content URLs
/// </summary>
public class GitHubManifestLoader : IManifestLoader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubManifestLoader> _logger;

    public GitHubManifestLoader(HttpClient httpClient, ILogger<GitHubManifestLoader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ManifestRoot> LoadManifestAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading manifests from {BaseUrl}", baseUrl);

        var directory = await LoadManifestFileAsync<DirectoryConfig>(baseUrl, "directory.json", cancellationToken);
        var mods = await LoadManifestFileAsync<List<ModDefinition>>(baseUrl, "mods.json", cancellationToken);
        var tools = await LoadManifestFileAsync<List<ToolDefinition>>(baseUrl, "tools.json", cancellationToken);

        return new ManifestRoot(directory, mods, tools);
    }

    /// <inheritdoc/>
    public async Task<T> LoadManifestFileAsync<T>(string baseUrl, string fileName, CancellationToken cancellationToken = default)
    {
        var url = $"{baseUrl.TrimEnd('/')}/{fileName}";
        _logger.LogDebug("Fetching manifest file: {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonConvert.DeserializeObject<T>(json);

        if (result == null)
            throw new InvalidOperationException($"Failed to deserialize manifest file: {fileName}");

        return result;
    }
}
