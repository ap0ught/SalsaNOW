using SuperSalsaNOW.Core.Models;

namespace SuperSalsaNOW.Core.Interfaces;

/// <summary>
/// Tracks which <see cref="GameProfile"/> the user is working with for this session.
/// </summary>
public interface IGameSession
{
    GameProfile? CurrentProfile { get; }

    /// <summary>
    /// Applies the profile and syncs install paths (game folder relative to install root).
    /// </summary>
    void SelectProfile(GameProfile profile);
}
