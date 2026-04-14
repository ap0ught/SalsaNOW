namespace SuperSalsaNOW.Windows.Services;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Manages DepotDownloader for installing Steam games without Steam client
/// ⚠️ REQUIRES WINDOWS: DepotDownloader is Windows-only tool
/// Reference: https://github.com/SteamRE/DepotDownloader
/// </summary>
public class DepotDownloaderService
{
    private readonly ILogger<DepotDownloaderService> _logger;
    private readonly string _depotDownloaderPath;

    public DepotDownloaderService(string toolsDirectory, ILogger<DepotDownloaderService> logger)
    {
        _depotDownloaderPath = Path.Combine(toolsDirectory, "DepotDownloader", "DepotDownloader.exe");
        _logger = logger;
    }

    /// <summary>
    /// Download DepotDownloader tool if not already present
    /// </summary>
    public Task EnsureDepotDownloaderAsync(CancellationToken cancellationToken = default)
    {
        if (System.IO.File.Exists(_depotDownloaderPath))
        {
            _logger.LogInformation("DepotDownloader already exists at: {Path}", _depotDownloaderPath);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Downloading DepotDownloader...");

        // TODO: Download from GitHub releases
        // URL: https://github.com/SteamRE/DepotDownloader/releases/latest
        // Should download ZIP and extract to tools directory

        throw new NotImplementedException("DepotDownloader download not yet implemented. Download manually from: https://github.com/SteamRE/DepotDownloader/releases");
    }

    /// <summary>
    /// Install a Steam app using DepotDownloader (game-specific app id comes from the active profile).
    /// </summary>
    public async Task<bool> InstallSteamAppAsync(
        int steamAppId,
        string username,
        string password,
        string installDirectory,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureDepotDownloaderAsync(cancellationToken);

        _logger.LogInformation("Installing Steam app {AppId} to: {Directory}", steamAppId, installDirectory);
        progress?.Report($"Starting Steam app {steamAppId} installation via DepotDownloader...");

        var arguments = $"-app {steamAppId} " +
                       $"-username \"{username}\" " +
                       $"-password \"{password}\" " +
                       $"-os windows " +
                       $"-no-mobile " +
                       $"-dir \"{installDirectory}\"";

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _depotDownloaderPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false // Show window for Steam Guard codes if needed
            };

            using var process = new Process { StartInfo = processInfo };

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    _logger.LogInformation("DepotDownloader: {Output}", args.Data);
                    progress?.Report(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    _logger.LogWarning("DepotDownloader Error: {Error}", args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            var success = process.ExitCode == 0;

            if (success)
            {
                var steamAppIdPath = Path.Combine(installDirectory, "steam_appid.txt");
                await System.IO.File.WriteAllTextAsync(steamAppIdPath, steamAppId.ToString(), cancellationToken);

                _logger.LogInformation("Steam app {AppId} installation complete", steamAppId);
                progress?.Report("Installation complete!");
            }
            else
            {
                _logger.LogError("DepotDownloader exited with code: {ExitCode}", process.ExitCode);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run DepotDownloader");
            progress?.Report($"Installation failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifies that the game executable exists under the install root.
    /// </summary>
    public bool VerifyGameInstallation(string gameInstallRoot, string executableRelativeToGameRoot)
    {
        var gamePath = Path.Combine(gameInstallRoot, executableRelativeToGameRoot);
        var exists = System.IO.File.Exists(gamePath);

        _logger.LogInformation("Game verification: {Path} exists = {Exists}", gamePath, exists);

        return exists;
    }

    /// <summary>
    /// Launch the game from the resolved executable path.
    /// </summary>
    public Process? LaunchGame(string gameInstallRoot, string executableRelativeToGameRoot)
    {
        var gamePath = Path.Combine(gameInstallRoot, executableRelativeToGameRoot);

        if (!System.IO.File.Exists(gamePath))
        {
            _logger.LogError("Cannot launch - game not found at: {Path}", gamePath);
            return null;
        }

        _logger.LogInformation("Launching game: {Path}", gamePath);

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = gamePath,
            WorkingDirectory = Path.GetDirectoryName(gamePath),
            UseShellExecute = true
        });

        return process;
    }
}
