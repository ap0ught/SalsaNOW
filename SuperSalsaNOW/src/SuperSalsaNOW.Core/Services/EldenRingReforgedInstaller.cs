namespace SuperSalsaNOW.Core.Services;

using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;
using SuperSalsaNOW.Core.Utilities;
using Microsoft.Extensions.Logging;

/// <summary>
/// Installs Elden Ring Reforged mod
/// Strategy: Download from Nexus, extract, prepare for launch
/// </summary>
public class EldenRingReforgedInstaller : IModInstaller
{
    private readonly INexusClient _nexusClient;
    private readonly ILogger<EldenRingReforgedInstaller> _logger;

    public EldenRingReforgedInstaller(INexusClient nexusClient, ILogger<EldenRingReforgedInstaller> logger)
    {
        _nexusClient = nexusClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<InstallResult> InstallAsync(ModDefinition mod, InstallOptions options, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            _logger.LogInformation("Installing mod: {ModName}", mod.Name);

            // Get mod files from Nexus
            var files = await _nexusClient.GetModFilesAsync(mod.Nexus.GameDomain, mod.Nexus.ModId, cancellationToken);
            var selectedFile = _nexusClient.SelectFile(files, mod.Nexus.FilePattern);

            if (selectedFile == null)
            {
                errors.Add($"No file found matching pattern: {mod.Nexus.FilePattern}");
                return new InstallResult(false, null, errors, warnings);
            }

            _logger.LogInformation("Selected file: {FileName} ({Size} bytes)", selectedFile.FileName, selectedFile.SizeBytes);

            // Get download links
            var links = await _nexusClient.GetDownloadLinksAsync(mod.Nexus.GameDomain, mod.Nexus.ModId, selectedFile.FileId, cancellationToken);
            if (links.Count == 0)
            {
                errors.Add("No download links available");
                return new InstallResult(false, null, errors, warnings);
            }

            var downloadUrl = links.First().Url;

            // Determine install directory
            var installDir = GetInstallDirectory(mod, options);
            Directory.CreateDirectory(installDir);

            // Download file
            var downloadPath = Path.Combine(installDir, selectedFile.FileName);
            _logger.LogInformation("Downloading to: {Path}", downloadPath);

            await DownloadHelper.DownloadFileAsync(downloadUrl, downloadPath, progress, cancellationToken);

            // Extract if ZIP
            if (selectedFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                selectedFile.FileName.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Extracting archive...");
                await ZipHelper.ExtractAsync(downloadPath, installDir, cancellationToken);
            }

            _logger.LogInformation("Installation complete: {Path}", installDir);

            return new InstallResult(true, installDir, errors, warnings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Installation failed for mod: {ModName}", mod.Name);
            errors.Add($"Installation failed: {ex.Message}");
            return new InstallResult(false, null, errors, warnings);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyInstallationAsync(ModDefinition mod, string installPath, CancellationToken cancellationToken = default)
    {
        // TODO: Implement verification logic
        // For ERR, check for launcher executable or required files
        return await Task.FromResult(Directory.Exists(installPath));
    }

    /// <inheritdoc/>
    public string GetInstallDirectory(ModDefinition mod, InstallOptions options)
    {
        return Path.Combine(options.TargetDirectory, mod.Id);
    }
}
