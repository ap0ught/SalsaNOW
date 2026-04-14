using SuperSalsaNOW.Core.Models;

namespace SuperSalsaNOW.Core.GameProfiles;

public static class KnownGameProfiles
{
    public static IReadOnlyList<GameProfile> All { get; } =
    [
        new GameProfile
        {
            Id = GameProfileIds.EldenRing,
            DisplayName = "Elden Ring",
            SteamAppId = 1245620,
            GameFolderRelativeToInstallRoot = @"Games\ELDENRING",
            GameExecutableRelativePath = @"Game\eldenring.exe",
            PrimaryModInstallSubfolder = "elden-ring-reforged",
            DesktopShortcutLauncherFileNames =
            [
                "Launch ELDEN RING Reforged.bat",
                "!! Launch ELDEN RING Reforged.BAT",
                "launch.bat"
            ],
            DefaultDesktopShortcutName = "Elden Ring Reforged.lnk"
        }
    ];

    public static GameProfile? GetById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;
        return All.FirstOrDefault(p => p.Id.Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
