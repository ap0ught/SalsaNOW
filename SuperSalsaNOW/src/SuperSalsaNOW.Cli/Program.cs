using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSalsaNOW.Cli;
using SuperSalsaNOW.Cli.Configuration;
using SuperSalsaNOW.Cli.Menu;
using SuperSalsaNOW.Core.GameProfiles;
using SuperSalsaNOW.Core.Interfaces;

// Build configuration - appsettings.json is optional, defaults hardcoded
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();

// Apply hardcoded defaults if not in config (like SalsaNOW does)
if (string.IsNullOrWhiteSpace(appSettings.Manifest.BaseUrl))
{
    appSettings.Manifest.BaseUrl = "https://raw.githubusercontent.com/psingley/SuperSalsaNOWThings/main";
}

if (string.IsNullOrWhiteSpace(appSettings.Nexus.ApiKey))
{
    appSettings.Nexus.ApiKey = appSettings.Nexus.DefaultApiKey;
}

if (string.IsNullOrWhiteSpace(appSettings.Nexus.DefaultApiKey))
{
    appSettings.Nexus.DefaultApiKey = "UkWU+JE6Lo2eIEKB5x7lm8iIYI2OXVitLAtLkhMZyck=--LXLBhGm9lfmeox4A--OrhzYNCXFaissr30jOdm6w==";
}

if (string.IsNullOrWhiteSpace(appSettings.Paths.InstallRoot))
{
    appSettings.Paths.InstallRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "SuperSalsaNOW");
}

if (string.IsNullOrWhiteSpace(appSettings.Paths.ModsDirectory))
{
    appSettings.Paths.ModsDirectory = "Mods";
}

if (string.IsNullOrWhiteSpace(appSettings.Paths.ToolsDirectory))
{
    appSettings.Paths.ToolsDirectory = "Tools";
}

// Do not default GameDirectory: it is set when the user picks a game profile (or via ActiveGameProfileId).

// Build host with DI - simple configuration without appsettings.json
var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        // Simple console logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddConsole();
        });

        services.AddSuperSalsaNOWServices(appSettings);
    })
    .Build();

// Optional: restore last game from config (no implicit Elden default unless id is set)
if (!string.IsNullOrWhiteSpace(appSettings.ActiveGameProfileId))
{
    var fromConfig = KnownGameProfiles.GetById(appSettings.ActiveGameProfileId);
    if (fromConfig != null)
        host.Services.GetRequiredService<IGameSession>().SelectProfile(fromConfig);
}

// Run application
var mainMenu = host.Services.GetRequiredService<MainMenu>();
await mainMenu.RunAsync();
