namespace SuperSalsaNOW.Cli.Menu;

using Spectre.Console;
using SuperSalsaNOW.Cli.Commands;
using SuperSalsaNOW.Core.GameProfiles;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;

public class MainMenu
{
    private readonly ConfigureNexusCommand _configureNexus;
    private readonly InstallGameCommand _installGame;
    private readonly VerifyGameCommand _verifyGame;
    private readonly InstallModCommand _installMod;
    private readonly CreateShortcutCommand _createShortcut;
    private readonly IGameSession _gameSession;

    public MainMenu(
        ConfigureNexusCommand configureNexus,
        InstallGameCommand installGame,
        VerifyGameCommand verifyGame,
        InstallModCommand installMod,
        CreateShortcutCommand createShortcut,
        IGameSession gameSession)
    {
        _configureNexus = configureNexus;
        _installGame = installGame;
        _verifyGame = verifyGame;
        _installMod = installMod;
        _createShortcut = createShortcut;
        _gameSession = gameSession;
    }

    public async Task RunAsync()
    {
        await EnsureGameProfileAsync();

        while (true)
        {
            Console.Clear();
            ShowHeader();

            var profile = _gameSession.CurrentProfile!;
            var choices = new List<string>
            {
                "1. Change active game",
                $"2. Install {profile.DisplayName}",
                "3. Verify vanilla installation",
                "4. Configure Nexus API key",
                "5. Install mod from manifest",
                "6. Create desktop shortcut (if configured for this game)",
                "Q. Quit"
            };

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Main Menu[/]")
                    .AddChoices(choices)
            );

            try
            {
                Console.Clear();

                switch (choice[0])
                {
                    case '1':
                        await PromptSelectGameAsync();
                        break;
                    case '2':
                        await _installGame.ExecuteAsync();
                        break;
                    case '3':
                        _verifyGame.Execute();
                        break;
                    case '4':
                        _configureNexus.Execute();
                        break;
                    case '5':
                        await _installMod.ExecuteAsync();
                        break;
                    case '6':
                        _createShortcut.Execute();
                        break;
                    case 'Q':
                    case 'q':
                        return;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
        }
    }

    private async Task EnsureGameProfileAsync()
    {
        if (_gameSession.CurrentProfile != null)
            return;

        Console.Clear();
        ShowHeader();
        AnsiConsole.MarkupLine("[yellow]Choose the game you are setting up.[/]");
        AnsiConsole.WriteLine();
        await PromptSelectGameAsync();
    }

    private Task PromptSelectGameAsync()
    {
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<GameProfile>()
                .Title("[cyan]Select active game[/]")
                .AddChoices(KnownGameProfiles.All)
                .UseConverter(p => p.DisplayName)
        );

        _gameSession.SelectProfile(selected);
        AnsiConsole.MarkupLine($"[green]✓[/] Active game: [bold]{selected.DisplayName}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
        return Task.CompletedTask;
    }

    private static void ShowHeader()
    {
        var rule = new Rule("[bold cyan]SuperSalsaNOW — mod manager[/]");
        rule.Justification = Justify.Center;
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }
}
