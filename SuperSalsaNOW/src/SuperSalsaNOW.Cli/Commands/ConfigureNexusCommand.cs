namespace SuperSalsaNOW.Cli.Commands;

using Microsoft.Extensions.Logging;
using Spectre.Console;
using SuperSalsaNOW.Cli.Configuration;

public class ConfigureNexusCommand
{
    private readonly NexusSettings _nexusSettings;
    private readonly ILogger<ConfigureNexusCommand> _logger;

    public ConfigureNexusCommand(NexusSettings nexusSettings, ILogger<ConfigureNexusCommand> logger)
    {
        _nexusSettings = nexusSettings;
        _logger = logger;
    }

    public void Execute()
    {
        AnsiConsole.MarkupLine("[bold cyan]Configure Nexus Mods API Key[/]");
        AnsiConsole.WriteLine();

        var currentKey = string.IsNullOrWhiteSpace(_nexusSettings.ApiKey)
            ? "[dim]Not configured[/]"
            : MaskApiKey(_nexusSettings.ApiKey);

        AnsiConsole.MarkupLine($"Current API Key: {currentKey}");
        AnsiConsole.WriteLine();

        var prompt = new TextPrompt<string>("[yellow]Enter Nexus API Key (or press Enter to use default):[/]")
            .AllowEmpty()
            .DefaultValue(_nexusSettings.DefaultApiKey);

        var apiKey = AnsiConsole.Prompt(prompt);

        _nexusSettings.ApiKey = apiKey;

        AnsiConsole.MarkupLine("[green]✓[/] Nexus API Key configured");
        _logger.LogInformation("Nexus API key configured");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private static string MaskApiKey(string key)
    {
        if (key.Length <= 8) return "****";
        return $"{key.Substring(0, 4)}...{key.Substring(key.Length - 4)}";
    }
}
