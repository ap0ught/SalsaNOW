namespace SuperSalsaNOW.Core.Models;

/// <summary>
/// Configuration for Nexus Mods API client
/// </summary>
public record NexusConfig(
    string ApiKey,
    string UserAgent = "SuperSalsaNOW/1.0"
);
