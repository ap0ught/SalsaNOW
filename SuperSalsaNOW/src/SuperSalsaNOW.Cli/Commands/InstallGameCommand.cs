namespace SuperSalsaNOW.Cli.Commands;

using Microsoft.Extensions.Logging;
using Spectre.Console;
using SuperSalsaNOW.Windows.Services;
using SuperSalsaNOW.Cli.Configuration;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;

public class InstallGameCommand
{
    private readonly DepotDownloaderService _depotDownloader;
    private readonly PathSettings _paths;
    private readonly IGameSession _gameSession;
    private readonly ILogger<InstallGameCommand> _logger;

    public InstallGameCommand(
        DepotDownloaderService depotDownloader,
        PathSettings paths,
        IGameSession gameSession,
        ILogger<InstallGameCommand> logger)
    {
        _depotDownloader = depotDownloader;
        _paths = paths;
        _gameSession = gameSession;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var profile = _gameSession.CurrentProfile
            ?? throw new InvalidOperationException("No active game profile. Select a game from the menu first.");

        AnsiConsole.MarkupLine($"[bold cyan]Install {profile.DisplayName}[/]");
        AnsiConsole.WriteLine();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose installation method:")
                .AddChoices(
                    "Via Steam (recommended)",
                    "Via DepotDownloader (automated)",
                    "Skip (already installed)"
                )
        );

        if (choice.Contains("Skip"))
        {
            AnsiConsole.MarkupLine("[yellow]Skipping installation[/]");
            return;
        }

        if (choice.Contains("Steam"))
        {
            await InstallViaSteamAsync(profile);
        }
        else
        {
            await InstallViaDepotDownloaderAsync(profile);
        }
    }

    private Task InstallViaSteamAsync(GameProfile profile)
    {
        AnsiConsole.MarkupLine("[yellow]Manual Installation via Steam:[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("1. Open Steam client");
        AnsiConsole.MarkupLine("2. Go to Library");
        AnsiConsole.MarkupLine($"3. Find '{profile.DisplayName}'");
        AnsiConsole.MarkupLine($"4. Install to: [cyan]{_paths.FullGamePath}[/]");
        AnsiConsole.MarkupLine("5. Wait for installation to complete");
        AnsiConsole.MarkupLine("6. Exit Steam");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Press any key when installation is complete...[/]");
        Console.ReadKey(true);

        _logger.LogInformation("User installed {Game} via Steam", profile.DisplayName);
        return Task.CompletedTask;
    }

    private async Task InstallViaDepotDownloaderAsync(GameProfile profile)
    {
        AnsiConsole.MarkupLine("[yellow]Automated Installation via DepotDownloader[/]");
        AnsiConsole.WriteLine();

        var username = AnsiConsole.Ask<string>("Steam [cyan]username[/]:");
        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("Steam [cyan]password[/]:")
                .Secret()
        );

        var progress = new Progress<string>(msg => AnsiConsole.MarkupLine($"[dim]{msg}[/]"));

        await AnsiConsole.Status()
            .StartAsync($"Installing {profile.DisplayName}...", async ctx =>
            {
                try
                {
                    var success = await _depotDownloader.InstallSteamAppAsync(
                        profile.SteamAppId,
                        username,
                        password,
                        _paths.FullGamePath,
                        progress
                    );

                    if (success)
                    {
                        AnsiConsole.MarkupLine($"[green]✓[/] {profile.DisplayName} installed successfully");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]✗[/] Installation failed");
                    }
                }
                catch (NotImplementedException)
                {
                    AnsiConsole.MarkupLine("[red]✗[/] DepotDownloader auto-download not yet implemented");
                    AnsiConsole.MarkupLine("[yellow]Please download manually from:[/]");
                    AnsiConsole.MarkupLine("https://github.com/SteamRE/DepotDownloader/releases");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Installation failed");
                    AnsiConsole.MarkupLine($"[red]✗[/] {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}
