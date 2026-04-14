namespace SuperSalsaNOW.Cli.Commands;

using Microsoft.Extensions.Logging;
using Spectre.Console;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Windows.Services;
using SuperSalsaNOW.Cli.Configuration;

public class CreateShortcutCommand
{
    private readonly IShortcutService _shortcutService;
    private readonly PathSettings _paths;
    private readonly IGameSession _gameSession;
    private readonly ILogger<CreateShortcutCommand> _logger;

    public CreateShortcutCommand(
        IShortcutService shortcutService,
        PathSettings paths,
        IGameSession gameSession,
        ILogger<CreateShortcutCommand> logger)
    {
        _shortcutService = shortcutService;
        _paths = paths;
        _gameSession = gameSession;
        _logger = logger;
    }

    public void Execute()
    {
        var profile = _gameSession.CurrentProfile
            ?? throw new InvalidOperationException("No active game profile. Select a game from the menu first.");

        if (string.IsNullOrWhiteSpace(profile.PrimaryModInstallSubfolder)
            || profile.DesktopShortcutLauncherFileNames is null
            || profile.DesktopShortcutLauncherFileNames.Count == 0
            || string.IsNullOrWhiteSpace(profile.DefaultDesktopShortcutName))
        {
            AnsiConsole.MarkupLine("[yellow]This game profile does not define a desktop shortcut flow yet.[/]");
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        AnsiConsole.MarkupLine("[bold cyan]Create Desktop Shortcut[/]");
        AnsiConsole.WriteLine();

        var modRoot = Path.Combine(_paths.FullModsPath, profile.PrimaryModInstallSubfolder);
        var launcherCandidates = profile.DesktopShortcutLauncherFileNames
            .Select(name => Path.Combine(modRoot, name))
            .ToList();

        var launcher = launcherCandidates.FirstOrDefault(File.Exists);

        if (launcher == null)
        {
            AnsiConsole.MarkupLine("[red]✗[/] Launcher not found");
            AnsiConsole.MarkupLine($"Searched in: {modRoot}");
            _logger.LogWarning("Launcher not found under {Path}", modRoot);
        }
        else
        {
            try
            {
                var desktopPath = WindowsShortcutService.GetDesktopPath();
                var shortcutPath = Path.Combine(desktopPath, profile.DefaultDesktopShortcutName);

                _shortcutService.CreateShortcut(
                    targetPath: launcher,
                    shortcutPath: shortcutPath,
                    workingDirectory: Path.GetDirectoryName(launcher)
                );

                AnsiConsole.MarkupLine($"[green]✓[/] Shortcut created: {shortcutPath}");
                _logger.LogInformation("Shortcut created at {Path}", shortcutPath);
            }
            catch (PlatformNotSupportedException)
            {
                AnsiConsole.MarkupLine("[red]✗[/] Shortcut creation requires Windows");
                _logger.LogWarning("Attempted to create shortcut on non-Windows platform");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗[/] Failed to create shortcut: {ex.Message}");
                _logger.LogError(ex, "Failed to create shortcut");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}
