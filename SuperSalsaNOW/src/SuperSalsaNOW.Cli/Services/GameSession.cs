using SuperSalsaNOW.Cli.Configuration;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;

namespace SuperSalsaNOW.Cli.Services;

public sealed class GameSession : IGameSession
{
    private readonly PathSettings _paths;

    public GameSession(PathSettings paths) => _paths = paths;

    public GameProfile? CurrentProfile { get; private set; }

    public void SelectProfile(GameProfile profile)
    {
        CurrentProfile = profile;
        _paths.GameDirectory = profile.GameFolderRelativeToInstallRoot;
    }
}
