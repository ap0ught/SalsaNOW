namespace SuperSalsaNOW.Windows.Services;

using SuperSalsaNOW.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

/// <summary>
/// Windows-specific implementation of shortcut creation using Windows Script Host
/// ⚠️ REQUIRES WINDOWS: Uses COM interop (IWshRuntimeLibrary)
///
/// NOTE: This uses dynamic COM invocation to avoid compile-time dependencies.
/// On Windows build agents, enable COMReference in .csproj for better type safety.
/// </summary>
public class WindowsShortcutService : IShortcutService
{
    private readonly ILogger<WindowsShortcutService> _logger;

    public WindowsShortcutService(ILogger<WindowsShortcutService> logger)
    {
        _logger = logger;
    }

    public void CreateShortcut(string targetPath, string shortcutPath, string? workingDirectory = null, string? arguments = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Shortcut creation is only supported on Windows");
        }

        try
        {
            _logger.LogInformation("Creating Windows shortcut: {ShortcutPath} -> {TargetPath}", shortcutPath, targetPath);

            // ⚠️ WINDOWS-ONLY: COM object creation using late binding
            // This avoids compile-time COM reference requirements
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
            {
                throw new PlatformNotSupportedException("WScript.Shell COM object not available");
            }

            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);

            shortcut.TargetPath = targetPath;

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                shortcut.WorkingDirectory = workingDirectory;
            }
            else
            {
                // Default to target's directory
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath) ?? "";
            }

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                shortcut.Arguments = arguments;
            }

            shortcut.Save();

            _logger.LogInformation("Shortcut created successfully");

            // Release COM objects
            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);
        }
        catch (COMException ex)
        {
            _logger.LogError(ex, "COM error creating shortcut (requires Windows)");
            throw new PlatformNotSupportedException("Shortcut creation requires Windows", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create shortcut");
            throw;
        }
    }

    public bool ShortcutExists(string shortcutPath)
    {
        var exists = System.IO.File.Exists(shortcutPath);
        _logger.LogDebug("Shortcut exists check: {Path} = {Exists}", shortcutPath, exists);
        return exists;
    }

    /// <summary>
    /// Get the standard Windows desktop path for current user
    /// </summary>
    public static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    /// <summary>
    /// Get the public desktop path (all users)
    /// </summary>
    public static string GetPublicDesktopPath()
    {
        // C:\Users\Public\Desktop
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "..", "Desktop"
        );
    }
}
