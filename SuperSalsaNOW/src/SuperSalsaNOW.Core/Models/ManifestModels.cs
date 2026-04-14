namespace SuperSalsaNOW.Core.Models;

/// <summary>
/// Root manifest containing all configuration sections
/// </summary>
public record ManifestRoot(
    DirectoryConfig Directory,
    List<ModDefinition> Mods,
    List<ToolDefinition> Tools
);

/// <summary>
/// Directory structure configuration for game and mod installations
/// </summary>
public record DirectoryConfig(
    string InstallRoot,
    string GameDirectory,
    string ModsDirectory
);

/// <summary>
/// Complete definition of a mod including source and installation strategy
/// </summary>
public record ModDefinition(
    string Id,
    string Name,
    string Description,
    NexusInfo Nexus,
    InstallStrategy Strategy
);

/// <summary>
/// Nexus Mods API information for retrieving mod files
/// </summary>
public record NexusInfo(
    string GameDomain,      // e.g., "eldenring"
    int ModId,              // e.g., 541 for ERR
    string FilePattern      // e.g., "main", "latest"
);

/// <summary>
/// External tool definition (e.g., Mod Engine 2)
/// </summary>
public record ToolDefinition(
    string Id,
    string Name,
    string Url,
    string Version
);

/// <summary>
/// Strategy for installing and launching a mod
/// </summary>
public enum InstallStrategy
{
    /// <summary>
    /// Use mod's built-in launcher (e.g., ERR launcher)
    /// </summary>
    ErrLauncher,

    /// <summary>
    /// Use Mod Engine 2 framework for loading
    /// </summary>
    ModEngine2
}
