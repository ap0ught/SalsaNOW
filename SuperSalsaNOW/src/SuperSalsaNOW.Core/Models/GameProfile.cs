namespace SuperSalsaNOW.Core.Models;

/// <summary>
/// Describes a supported Steam / Nexus modding target. Elden Ring is one catalog entry;
/// add more profiles here as implementations land.
/// </summary>
public sealed class GameProfile
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required int SteamAppId { get; init; }
    /// <summary>Segment under <see cref="Cli.Configuration.PathSettings.InstallRoot"/> (e.g. Games\ELDENRING).</summary>
    public required string GameFolderRelativeToInstallRoot { get; init; }
    /// <summary>Path under the resolved game root to the main executable (e.g. Game\eldenring.exe).</summary>
    public required string GameExecutableRelativePath { get; init; }
    /// <summary>Mods path segment for this title's primary shortcut flow (e.g. elden-ring-reforged).</summary>
    public string? PrimaryModInstallSubfolder { get; init; }
    public IReadOnlyList<string>? DesktopShortcutLauncherFileNames { get; init; }
    public string? DefaultDesktopShortcutName { get; init; }
}

public static class GameProfileIds
{
    public const string EldenRing = "elden-ring";
}
