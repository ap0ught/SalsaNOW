namespace SuperSalsaNOW.Core.Interfaces;

/// <summary>
/// Creates desktop shortcuts (.lnk files on Windows)
/// Platform-specific implementation required
/// </summary>
public interface IShortcutService
{
    /// <summary>
    /// Create a desktop shortcut
    /// </summary>
    /// <param name="targetPath">Path to executable or file to launch</param>
    /// <param name="shortcutPath">Full path where shortcut should be created</param>
    /// <param name="workingDirectory">Working directory for shortcut (optional)</param>
    /// <param name="arguments">Command-line arguments (optional)</param>
    void CreateShortcut(string targetPath, string shortcutPath, string? workingDirectory = null, string? arguments = null);

    /// <summary>
    /// Check if shortcut already exists
    /// </summary>
    /// <param name="shortcutPath">Full path to shortcut file</param>
    /// <returns>True if shortcut exists, false otherwise</returns>
    bool ShortcutExists(string shortcutPath);
}
