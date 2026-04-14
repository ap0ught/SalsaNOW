namespace SuperSalsaNOW.Cli.Commands;

using Microsoft.Extensions.Logging;
using Spectre.Console;
using SuperSalsaNOW.Windows.Services;
using SuperSalsaNOW.Cli.Configuration;
using SuperSalsaNOW.Core.Interfaces;

public class VerifyGameCommand
{
    private readonly DepotDownloaderService _depotDownloader;
    private readonly PathSettings _paths;
    private readonly IGameSession _gameSession;
    private readonly ILogger<VerifyGameCommand> _logger;

    public VerifyGameCommand(
        DepotDownloaderService depotDownloader,
        PathSettings paths,
        IGameSession gameSession,
        ILogger<VerifyGameCommand> logger)
    {
        _depotDownloader = depotDownloader;
        _paths = paths;
        _gameSession = gameSession;
        _logger = logger;
    }

    public void Execute()
    {
        var profile = _gameSession.CurrentProfile
            ?? throw new InvalidOperationException("No active game profile. Select a game from the menu first.");

        AnsiConsole.MarkupLine("[bold cyan]Verify Vanilla Installation[/]");
        AnsiConsole.WriteLine();

        var installed = _depotDownloader.VerifyGameInstallation(_paths.FullGamePath, profile.GameExecutableRelativePath);

        if (!installed)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] {profile.DisplayName} not found at: {_paths.FullGamePath}");
            AnsiConsole.MarkupLine("[yellow]Please install the game first[/]");
            _logger.LogWarning("{Game} not found at {Path}", profile.DisplayName, _paths.FullGamePath);
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {profile.DisplayName} found at: {_paths.FullGamePath}");
            _logger.LogInformation("{Game} verified at {Path}", profile.DisplayName, _paths.FullGamePath);

            var launch = AnsiConsole.Confirm("Launch game to verify?", defaultValue: false);

            if (launch)
            {
                AnsiConsole.MarkupLine($"[yellow]Launching {profile.DisplayName}...[/]");
                var process = _depotDownloader.LaunchGame(_paths.FullGamePath, profile.GameExecutableRelativePath);

                if (process != null)
                {
                    AnsiConsole.MarkupLine("[green]✓[/] Game launched");
                    AnsiConsole.MarkupLine("[dim]Please verify the game reaches main menu, then exit[/]");
                    _logger.LogInformation("Game launched for verification");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]✗[/] Failed to launch game");
                }
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}
