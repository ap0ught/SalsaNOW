namespace SuperSalsaNOW.Cli.Commands;

using Microsoft.Extensions.Logging;
using Spectre.Console;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;
using SuperSalsaNOW.Cli.Configuration;

public class InstallModCommand
{
    private readonly IManifestLoader _manifestLoader;
    private readonly IModInstaller _modInstaller;
    private readonly ManifestSettings _manifestSettings;
    private readonly PathSettings _paths;
    private readonly ILogger<InstallModCommand> _logger;

    public InstallModCommand(
        IManifestLoader manifestLoader,
        IModInstaller modInstaller,
        ManifestSettings manifestSettings,
        PathSettings paths,
        ILogger<InstallModCommand> logger)
    {
        _manifestLoader = manifestLoader;
        _modInstaller = modInstaller;
        _manifestSettings = manifestSettings;
        _paths = paths;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        AnsiConsole.MarkupLine("[bold cyan]Install Mods[/]");
        AnsiConsole.WriteLine();

        await AnsiConsole.Status()
            .StartAsync("Loading mod manifest...", async ctx =>
            {
                try
                {
                    var manifest = await _manifestLoader.LoadManifestAsync(_manifestSettings.BaseUrl);

                    if (manifest.Mods.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[yellow]No mods found in manifest[/]");
                        return;
                    }

                    ctx.Status("Selecting mod...");

                    var selectedMod = AnsiConsole.Prompt(
                        new SelectionPrompt<ModDefinition>()
                            .Title("Select mod to install:")
                            .AddChoices(manifest.Mods)
                            .UseConverter(mod => $"{mod.Name} - {mod.Description}")
                    );

                    ctx.Status($"Installing {selectedMod.Name}...");

                    var options = new InstallOptions(
                        TargetDirectory: _paths.FullModsPath,
                        OverwriteExisting: true,
                        CreateShortcut: false
                    );

                    var progress = new Progress<double>(pct =>
                    {
                        ctx.Status($"Installing {selectedMod.Name}... {pct:F1}%");
                    });

                    var result = await _modInstaller.InstallAsync(selectedMod, options, progress);

                    if (result.Success)
                    {
                        AnsiConsole.MarkupLine($"[green]✓[/] {selectedMod.Name} installed to: {result.InstalledPath}");
                        _logger.LogInformation("Mod installed: {ModName} at {Path}", selectedMod.Name, result.InstalledPath);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]✗[/] Installation failed");
                        foreach (var error in result.Errors)
                        {
                            AnsiConsole.MarkupLine($"  [red]- {error}[/]");
                        }
                        _logger.LogError("Mod installation failed: {Errors}", string.Join(", ", result.Errors));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load or install mod");
                    AnsiConsole.MarkupLine($"[red]✗[/] Error: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}
