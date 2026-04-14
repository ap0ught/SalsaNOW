namespace SuperSalsaNOW.Core.Services;

using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;
using Microsoft.Extensions.Logging;
using Pathoschild.FluentNexus;
using NexusModFile = Pathoschild.FluentNexus.Models.ModFile;

/// <summary>
/// Nexus Mods API client using FluentNexus library
/// </summary>
public class FluentNexusClient : INexusClient
{
    private readonly NexusClient _client;
    private readonly ILogger<FluentNexusClient> _logger;

    public FluentNexusClient(NexusConfig config, ILogger<FluentNexusClient> logger)
    {
        _client = new NexusClient(config.ApiKey, "SuperSalsaNOW", "1.0.0", config.UserAgent);
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<Models.ModFile>> GetModFilesAsync(string gameDomain, int modId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching files for mod {ModId} in game {GameDomain}", modId, gameDomain);

        var filesList = await _client.ModFiles.GetModFiles(gameDomain, modId);
        var files = filesList.Files;

        return files.Select(f => new Models.ModFile(
            FileId: f.FileID,
            FileName: f.FileName,
            Version: f.FileVersion ?? "unknown",
            SizeBytes: f.SizeInBytes ?? 0,
            UploadedDate: f.UploadedTimestamp.DateTime
        )).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DownloadLink>> GetDownloadLinksAsync(string gameDomain, int modId, int fileId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating download links for file {FileId}", fileId);

        var links = await _client.ModFiles.GetDownloadLinks(gameDomain, modId, fileId);

        return links.Select(l => new DownloadLink(
            Url: l.Uri.ToString(),
            ExpiresAt: DateTime.UtcNow.AddHours(1) // Nexus links typically expire in 1 hour
        )).ToList();
    }

    /// <inheritdoc/>
    public Models.ModFile? SelectFile(List<Models.ModFile> files, string pattern)
    {
        _logger.LogDebug("Selecting file with pattern: {Pattern}", pattern);

        return pattern.ToLowerInvariant() switch
        {
            "main" => files.FirstOrDefault(f => f.FileName.Contains("MAIN", StringComparison.OrdinalIgnoreCase)),
            "latest" => files.OrderByDescending(f => f.UploadedDate).FirstOrDefault(),
            _ => files.FirstOrDefault()
        };
    }
}
