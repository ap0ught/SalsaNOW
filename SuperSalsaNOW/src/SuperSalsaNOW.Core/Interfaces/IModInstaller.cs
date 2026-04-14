namespace SuperSalsaNOW.Core.Interfaces;

using SuperSalsaNOW.Core.Models;

/// <summary>
/// Installs and manages mods
/// </summary>
public interface IModInstaller
{
    /// <summary>
    /// Install a mod from manifest definition
    /// </summary>
    /// <param name="mod">Mod definition from manifest</param>
    /// <param name="options">Installation options (directory, overwrite, shortcut)</param>
    /// <param name="progress">Progress reporter (0-100 percentage)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Installation result with success status and any errors/warnings</returns>
    Task<InstallResult> InstallAsync(ModDefinition mod, InstallOptions options, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify mod installation is complete and valid
    /// </summary>
    /// <param name="mod">Mod definition to verify</param>
    /// <param name="installPath">Path where mod is installed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if installation is valid, false otherwise</returns>
    Task<bool> VerifyInstallationAsync(ModDefinition mod, string installPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get installation directory for a mod
    /// </summary>
    /// <param name="mod">Mod definition</param>
    /// <param name="options">Installation options</param>
    /// <returns>Full path to mod installation directory</returns>
    string GetInstallDirectory(ModDefinition mod, InstallOptions options);
}
