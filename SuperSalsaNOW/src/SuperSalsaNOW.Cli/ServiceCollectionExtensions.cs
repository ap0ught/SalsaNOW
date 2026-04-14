namespace SuperSalsaNOW.Cli;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;
using SuperSalsaNOW.Core.Services;
using SuperSalsaNOW.Windows.Services;
using SuperSalsaNOW.Cli.Configuration;
using SuperSalsaNOW.Cli.Commands;
using SuperSalsaNOW.Cli.Menu;
using SuperSalsaNOW.Cli.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSuperSalsaNOWServices(this IServiceCollection services, AppSettings settings)
    {
        // Configuration
        services.AddSingleton(settings);
        services.AddSingleton(settings.Manifest);
        services.AddSingleton(settings.Nexus);
        services.AddSingleton(settings.Paths);

        services.AddSingleton<IGameSession, GameSession>();

        // HTTP Client for manifest loading
        services.AddHttpClient<IManifestLoader, GitHubManifestLoader>();

        // Nexus client
        services.AddSingleton<INexusClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FluentNexusClient>>();
            var config = new NexusConfig(
                ApiKey: string.IsNullOrWhiteSpace(settings.Nexus.ApiKey)
                    ? settings.Nexus.DefaultApiKey
                    : settings.Nexus.ApiKey
            );
            return new FluentNexusClient(config, logger);
        });

        // Mod installer
        services.AddSingleton<IModInstaller, EldenRingReforgedInstaller>();

        // Windows services
        services.AddSingleton<IShortcutService, WindowsShortcutService>();
        services.AddSingleton<DepotDownloaderService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DepotDownloaderService>>();
            var paths = sp.GetRequiredService<PathSettings>();
            return new DepotDownloaderService(paths.FullToolsPath, logger);
        });

        // Commands
        services.AddTransient<ConfigureNexusCommand>();
        services.AddTransient<InstallGameCommand>();
        services.AddTransient<VerifyGameCommand>();
        services.AddTransient<InstallModCommand>();
        services.AddTransient<CreateShortcutCommand>();

        // Menu
        services.AddSingleton<MainMenu>();

        return services;
    }
}
