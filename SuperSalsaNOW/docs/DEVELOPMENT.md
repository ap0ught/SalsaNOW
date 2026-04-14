# SuperSalsaNOW - Development Guide

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Getting Started](#getting-started)
3. [Development Workflow](#development-workflow)
4. [Building and Testing](#building-and-testing)
5. [Adding New Mods](#adding-new-mods)
6. [Extending the System](#extending-the-system)
7. [Debugging](#debugging)
8. [Code Conventions](#code-conventions)
9. [Common Tasks](#common-tasks)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

1. **.NET 8.0 SDK** (version 8.0.300 or later)
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation:
     ```bash
     dotnet --version
     # Should output: 8.0.300 or higher
     ```

2. **Git** (for version control)
   - Download: https://git-scm.com/downloads
   - Verify:
     ```bash
     git --version
     ```

3. **Code Editor** (choose one):
   - **Visual Studio 2022** (Windows/Mac) - Recommended for full IDE experience
   - **Visual Studio Code** with C# extension - Lightweight, cross-platform
   - **JetBrains Rider** - Powerful cross-platform .NET IDE

### Optional Tools

- **Windows 10/11** - Required for testing Windows-specific features
- **Nexus Mods Account** - For testing Nexus API integration (free)
- **Steam Account** - For testing DepotDownloader (game installation)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/psingley/SuperSalsaNOW.git
cd SuperSalsaNOW
```

### 2. Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages defined in `.csproj` files.

### 3. Build the Solution

```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Run the CLI Application

```bash
cd src/SuperSalsaNOW.Cli
dotnet run
```

You should see the interactive menu.

---

## Development Workflow

### macOS Development Setup

Since Windows-specific features (shortcuts, DepotDownloader) won't work on macOS, development focuses on:

1. **Core Logic** - Business logic, models, services (platform-agnostic)
2. **Unit Tests** - Test all core functionality
3. **API Integration** - Nexus API, GitHub manifest loading
4. **CLI Interface** - Menu system, commands (Spectre.Console works on macOS)

**What Works on macOS:**
- ✅ Building all projects
- ✅ Running unit tests
- ✅ Testing Nexus API integration
- ✅ Testing manifest loading
- ✅ CLI menu navigation
- ✅ File download and extraction logic

**What Requires Windows:**
- ❌ Desktop shortcut creation
- ❌ DepotDownloader execution
- ❌ Full end-to-end testing

**Recommended Workflow:**
1. Develop core features on macOS
2. Test platform-agnostic code on macOS
3. Publish Windows build from macOS
4. Transfer executable to Windows VM/machine for testing
5. Validate Windows-specific features
6. Report results back to macOS for fixes

### Project Organization

```
SuperSalsaNOW/
├── src/                      # Source code
│   ├── SuperSalsaNOW.Core/   # Work here for cross-platform features
│   ├── SuperSalsaNOW.Windows/# Work here for Windows-specific features
│   └── SuperSalsaNOW.Cli/    # Work here for UI and commands
├── tests/                    # Unit tests (work here frequently)
├── docs/                     # Documentation (update as you build)
└── scripts/                  # Build and utility scripts
```

---

## Building and Testing

### Build Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/SuperSalsaNOW.Core

# Build in Release mode
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

### Test Commands

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~GitHubManifestLoaderTests"

# Run tests matching pattern
dotnet test --filter "Name~Nexus"
```

### Run Commands

```bash
# Run CLI application
cd src/SuperSalsaNOW.Cli
dotnet run

# Run with arguments (future)
dotnet run -- --config custom-config.json

# Run in Release mode
dotnet run -c Release
```

### Publish Commands

**Framework-Dependent (Requires .NET 8 runtime on target):**

```bash
# Windows x64 (smallest size)
dotnet publish src/SuperSalsaNOW.Cli -c Release -r win-x64 --self-contained false

# macOS x64 (Intel Macs)
dotnet publish src/SuperSalsaNOW.Cli -c Release -r osx-x64 --self-contained false

# macOS ARM64 (Apple Silicon)
dotnet publish src/SuperSalsaNOW.Cli -c Release -r osx-arm64 --self-contained false
```

**Self-Contained (Bundles .NET runtime, ~60MB larger):**

```bash
# Windows x64
dotnet publish src/SuperSalsaNOW.Cli -c Release -r win-x64 --self-contained true

# Single-file executable (Windows)
dotnet publish src/SuperSalsaNOW.Cli -c Release -r win-x64 \
  --self-contained false \
  /p:PublishSingleFile=true
```

**Output Location:**
```
src/SuperSalsaNOW.Cli/bin/Release/net8.0/win-x64/publish/
└── SuperSalsaNOW.exe
```

---

## Adding New Mods

### 1. Update Manifest

Edit the manifest JSON file (hosted on GitHub):

```json
{
  "mods": [
    {
      "id": "seamless-coop",
      "name": "Seamless Co-op",
      "description": "Multiplayer mod for Elden Ring",
      "nexus": {
        "gameDomain": "eldenring",
        "modId": 510,
        "filePattern": "main"
      },
      "strategy": "ModEngine2"
    }
  ]
}
```

**Fields:**
- `id`: Unique identifier (kebab-case)
- `name`: Display name
- `description`: User-facing description
- `nexus.gameDomain`: Nexus game slug (always "eldenring" for now)
- `nexus.modId`: Numeric mod ID from Nexus URL
- `nexus.filePattern`: Which file to download ("main", "latest", "optional")
- `strategy`: Installation strategy enum value

### 2. Create Installer (if needed)

If the mod requires custom installation logic, create a new installer:

**File:** `src/SuperSalsaNOW.Core/Services/SeamlessCoopInstaller.cs`

```csharp
namespace SuperSalsaNOW.Core.Services;

public class SeamlessCoopInstaller : IModInstaller
{
    private readonly INexusClient _nexusClient;
    private readonly ILogger<SeamlessCoopInstaller> _logger;

    public SeamlessCoopInstaller(INexusClient nexusClient, ILogger<SeamlessCoopInstaller> logger)
    {
        _nexusClient = nexusClient;
        _logger = logger;
    }

    public async Task<InstallResult> InstallAsync(
        ModDefinition mod,
        InstallOptions options,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Custom installation logic here
        // 1. Download from Nexus
        // 2. Extract files
        // 3. Copy to specific directories
        // 4. Modify config files (if needed)
        // 5. Return result

        return new InstallResult(true, installPath, errors, warnings);
    }

    public Task<bool> VerifyInstallationAsync(ModDefinition mod, string installPath, CancellationToken cancellationToken)
    {
        // Check for required files
        var requiredFile = Path.Combine(installPath, "seamlesscoopsettings.ini");
        return Task.FromResult(File.Exists(requiredFile));
    }

    public string GetInstallDirectory(ModDefinition mod, InstallOptions options)
    {
        return Path.Combine(options.TargetDirectory, mod.Id);
    }
}
```

### 3. Register Installer

**File:** `src/SuperSalsaNOW.Cli/ServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddSuperSalsaNOWServices(
    this IServiceCollection services,
    AppSettings settings)
{
    // Existing registrations...

    // Add new installer
    services.AddTransient<SeamlessCoopInstaller>();

    return services;
}
```

### 4. Update Command (if needed)

If the mod requires a dedicated menu option, add to `MainMenu.cs`:

```csharp
var choice = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[cyan]Main Menu[/]")
        .AddChoices(
            "1. Install Elden Ring",
            "2. Verify Vanilla Installation",
            "3. Configure Nexus API Key",
            "4. Install Elden Ring Reforged (ERR)",
            "5. Install Seamless Co-op",  // New option
            "6. Create Desktop Shortcut",
            "Q. Quit"
        )
);
```

### 5. Test

```bash
dotnet test
dotnet run
```

---

## Extending the System

### Adding a New Install Strategy

**Scenario:** You want to add support for "Reloaded II" mods.

1. **Define Enum Value:**

   **File:** `src/SuperSalsaNOW.Core/Models/ManifestModels.cs`

   ```csharp
   public enum InstallStrategy
   {
       ErrLauncher,
       ModEngine2,
       ReloadedII  // New strategy
   }
   ```

2. **Create Installer:**

   **File:** `src/SuperSalsaNOW.Core/Services/ReloadedIIInstaller.cs`

   ```csharp
   public class ReloadedIIInstaller : IModInstaller
   {
       // Implement IModInstaller interface
   }
   ```

3. **Register Service:**

   ```csharp
   services.AddTransient<ReloadedIIInstaller>();
   ```

4. **Update Factory Pattern (if needed):**

   Create an installer factory:

   ```csharp
   public interface IModInstallerFactory
   {
       IModInstaller GetInstaller(InstallStrategy strategy);
   }

   public class ModInstallerFactory : IModInstallerFactory
   {
       private readonly IServiceProvider _serviceProvider;

       public IModInstaller GetInstaller(InstallStrategy strategy)
       {
           return strategy switch
           {
               InstallStrategy.ErrLauncher => _serviceProvider.GetRequiredService<EldenRingReforgedInstaller>(),
               InstallStrategy.ModEngine2 => _serviceProvider.GetRequiredService<ModEngine2Installer>(),
               InstallStrategy.ReloadedII => _serviceProvider.GetRequiredService<ReloadedIIInstaller>(),
               _ => throw new NotSupportedException($"Strategy {strategy} not supported")
           };
       }
   }
   ```

### Adding a New Command

**Scenario:** Add "Uninstall Mod" command.

1. **Create Command Class:**

   **File:** `src/SuperSalsaNOW.Cli/Commands/UninstallModCommand.cs`

   ```csharp
   namespace SuperSalsaNOW.Cli.Commands;

   public class UninstallModCommand
   {
       private readonly IModInstaller _installer;
       private readonly ILogger<UninstallModCommand> _logger;

       public UninstallModCommand(IModInstaller installer, ILogger<UninstallModCommand> logger)
       {
           _installer = installer;
           _logger = logger;
       }

       public async Task ExecuteAsync()
       {
           // Prompt user for mod to uninstall
           // Delete mod files
           // Update state
       }
   }
   ```

2. **Register Command:**

   **File:** `src/SuperSalsaNOW.Cli/ServiceCollectionExtensions.cs`

   ```csharp
   services.AddTransient<UninstallModCommand>();
   ```

3. **Add to Menu:**

   **File:** `src/SuperSalsaNOW.Cli/Menu/MainMenu.cs`

   ```csharp
   private readonly UninstallModCommand _uninstallMod;

   public MainMenu(..., UninstallModCommand uninstallMod)
   {
       // ...
       _uninstallMod = uninstallMod;
   }

   // Add menu option and case handler
   ```

### Adding Platform-Specific Code

**Scenario:** Add Linux shortcut support.

1. **Create Linux Library:**

   ```bash
   dotnet new classlib -n SuperSalsaNOW.Linux -f net8.0
   ```

2. **Implement Interface:**

   **File:** `src/SuperSalsaNOW.Linux/Services/LinuxShortcutService.cs`

   ```csharp
   public class LinuxShortcutService : IShortcutService
   {
       public void CreateDesktopShortcut(string name, string targetPath, string? iconPath, string? workingDirectory)
       {
           // Create .desktop file
           var desktopFile = $"[Desktop Entry]\nName={name}\nExec={targetPath}\n...";
           var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{name}.desktop");
           File.WriteAllText(desktopPath, desktopFile);
       }
   }
   ```

3. **Conditional Registration:**

   ```csharp
   if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
   {
       services.AddSingleton<IShortcutService, LinuxShortcutService>();
   }
   ```

---

## Debugging

### Visual Studio Code

1. **Install C# Extension** (Microsoft)

2. **Create Launch Configuration:**

   **File:** `.vscode/launch.json`

   ```json
   {
       "version": "0.2.0",
       "configurations": [
           {
               "name": ".NET Core Launch (console)",
               "type": "coreclr",
               "request": "launch",
               "preLaunchTask": "build",
               "program": "${workspaceFolder}/src/SuperSalsaNOW.Cli/bin/Debug/net8.0/SuperSalsaNOW.Cli.dll",
               "args": [],
               "cwd": "${workspaceFolder}/src/SuperSalsaNOW.Cli",
               "console": "integratedTerminal",
               "stopAtEntry": false
           }
       ]
   }
   ```

3. **Set Breakpoints** - Click in gutter next to line numbers

4. **Press F5** to start debugging

### Visual Studio 2022

1. **Open Solution** - `SuperSalsaNOW.sln`
2. **Set Startup Project** - Right-click `SuperSalsaNOW.Cli` → Set as Startup Project
3. **Set Breakpoints** - Click in gutter or press F9
4. **Press F5** to start debugging

### Common Debugging Tasks

**Inspect Nexus API Response:**
```csharp
var files = await _nexusClient.GetModFilesAsync("eldenring", 541, cancellationToken);
_logger.LogInformation("Retrieved {Count} files", files.Count);  // Set breakpoint here
```

**Test Manifest Parsing:**
```csharp
var manifest = await _manifestLoader.LoadAsync(cancellationToken);
_logger.LogInformation("Loaded {Count} mods", manifest.Mods.Count);  // Set breakpoint here
```

**Debug Download Progress:**
```csharp
var progress = new Progress<double>(p =>
{
    Console.WriteLine($"Progress: {p:P}");  // Set breakpoint here
});
```

---

## Code Conventions

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Class | PascalCase | `EldenRingReforgedInstaller` |
| Interface | I + PascalCase | `IModInstaller` |
| Method | PascalCase | `InstallAsync` |
| Property | PascalCase | `ModId` |
| Field (private) | _camelCase | `_nexusClient` |
| Parameter | camelCase | `modDefinition` |
| Local variable | camelCase | `installPath` |
| Constant | PascalCase | `DefaultTimeout` |

### File Organization

```csharp
// 1. Using statements (grouped and sorted)
using System;
using System.Threading.Tasks;
using SuperSalsaNOW.Core.Interfaces;

// 2. Namespace
namespace SuperSalsaNOW.Core.Services;

// 3. XML doc comment
/// <summary>
/// Installs mods using Nexus Mods API
/// </summary>
public class NexusModInstaller : IModInstaller
{
    // 4. Fields (private, readonly preferred)
    private readonly INexusClient _nexusClient;
    private readonly ILogger<NexusModInstaller> _logger;

    // 5. Constructor
    public NexusModInstaller(INexusClient nexusClient, ILogger<NexusModInstaller> logger)
    {
        _nexusClient = nexusClient;
        _logger = logger;
    }

    // 6. Public methods
    public async Task<InstallResult> InstallAsync(...)
    {
        // Implementation
    }

    // 7. Private methods
    private string GetInstallPath(...)
    {
        // Implementation
    }
}
```

### Logging

**Use Structured Logging:**

```csharp
// Good
_logger.LogInformation("Installing mod: {ModName} (ID: {ModId})", mod.Name, mod.Id);

// Bad
_logger.LogInformation($"Installing mod: {mod.Name} (ID: {mod.Id})");
```

**Log Levels:**
- `LogTrace`: Very detailed, usually disabled in production
- `LogDebug`: Debugging information
- `LogInformation`: General flow (mod started, completed)
- `LogWarning`: Unexpected but recoverable (missing optional file)
- `LogError`: Error that prevents operation (download failed)
- `LogCritical`: Fatal error requiring shutdown

### Error Handling

```csharp
public async Task<InstallResult> InstallAsync(...)
{
    var errors = new List<string>();
    var warnings = new List<string>();

    try
    {
        // Happy path code
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to download mod from Nexus");
        errors.Add($"Network error: {ex.Message}");
        return new InstallResult(false, null, errors, warnings);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during installation");
        errors.Add($"Installation failed: {ex.Message}");
        return new InstallResult(false, null, errors, warnings);
    }
}
```

**Never Swallow Exceptions:**
```csharp
// Bad
try { ... } catch { }

// Good
try { ... }
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;  // Or handle gracefully
}
```

### Async/Await

```csharp
// Good: Async all the way
public async Task<InstallResult> InstallAsync(...)
{
    var files = await _nexusClient.GetModFilesAsync(...);
    await DownloadHelper.DownloadFileAsync(...);
    return result;
}

// Bad: Blocking on async code
public InstallResult Install(...)
{
    var files = _nexusClient.GetModFilesAsync(...).Result;  // ❌ Deadlock risk
}
```

**Cancellation Token Pattern:**
```csharp
public async Task<T> OperationAsync(CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();

    // Pass token to downstream calls
    var result = await SomeOperationAsync(cancellationToken);

    return result;
}
```

---

## Common Tasks

### Update Manifest URL

**File:** `src/SuperSalsaNOW.Cli/appsettings.json`

```json
{
  "ManifestBaseUrl": "https://raw.githubusercontent.com/YOUR-USERNAME/SuperSalsaNOWThings/main"
}
```

### Add New NuGet Package

```bash
# Add to specific project
dotnet add src/SuperSalsaNOW.Core package Newtonsoft.Json

# Add specific version
dotnet add src/SuperSalsaNOW.Core package Newtonsoft.Json --version 13.0.3
```

### Update NuGet Packages

```bash
# List outdated packages
dotnet list package --outdated

# Update all packages (use cautiously)
dotnet add package <PackageName>
```

### Generate Code Coverage Report

1. **Install ReportGenerator:**
   ```bash
   dotnet tool install -g dotnet-reportgenerator-globaltool
   ```

2. **Run Tests with Coverage:**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Generate HTML Report:**
   ```bash
   reportgenerator \
     -reports:tests/**/coverage.cobertura.xml \
     -targetdir:coverage-report \
     -reporttypes:Html
   ```

4. **Open Report:**
   ```bash
   open coverage-report/index.html
   ```

### Profile Performance

```csharp
using System.Diagnostics;

var sw = Stopwatch.StartNew();
await SomeOperationAsync();
sw.Stop();
_logger.LogInformation("Operation took {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

---

## Troubleshooting

### Build Errors

**Error:** `The type or namespace name 'X' could not be found`

**Solution:**
1. Ensure all projects are restored: `dotnet restore`
2. Check project references in `.csproj` files
3. Rebuild solution: `dotnet clean && dotnet build`

**Error:** `NU1202: Package X is not compatible with net8.0`

**Solution:**
1. Check package supports .NET 8.0
2. Update package to newer version
3. Check `<TargetFramework>` in `.csproj`

### Runtime Errors

**Error:** `System.IO.FileNotFoundException: Could not load file or assembly`

**Solution:**
1. Clean and rebuild: `dotnet clean && dotnet build`
2. Delete `bin` and `obj` folders manually
3. Restore packages: `dotnet restore`

**Error:** `NullReferenceException` in dependency injection

**Solution:**
1. Check service is registered in `ServiceCollectionExtensions.cs`
2. Verify constructor parameter types match registrations
3. Check service lifetime (Singleton vs Transient vs Scoped)

### Test Failures

**Error:** Tests fail with `HttpRequestException`

**Solution:**
1. Check network connectivity
2. Verify Nexus API key is configured (for integration tests)
3. Mock HTTP calls for unit tests

**Error:** Tests pass locally but fail in CI

**Solution:**
1. Check environment-specific configuration (file paths, API keys)
2. Use `[Trait("Category", "Integration")]` to skip integration tests in CI
3. Ensure CI has necessary secrets configured

### Windows-Specific Issues

**Error:** `PlatformNotSupportedException` when calling Windows services on macOS

**Solution:**
This is expected. Windows-specific code should be conditionally executed:

```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    _shortcutService.CreateDesktopShortcut(...);
}
else
{
    _logger.LogWarning("Shortcut creation not supported on this platform");
}
```

### Configuration Issues

**Error:** `NullReferenceException` when accessing `AppSettings`

**Solution:**
1. Ensure `appsettings.json` exists in CLI project
2. Verify `appsettings.json` is copied to output directory (check `.csproj`):
   ```xml
   <ItemGroup>
     <None Update="appsettings.json">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </None>
   </ItemGroup>
   ```

---

## Resources

### Official Documentation
- **.NET 8 Docs**: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8
- **C# Language Reference**: https://learn.microsoft.com/en-us/dotnet/csharp/
- **xUnit Testing**: https://xunit.net/

### Libraries Used
- **Spectre.Console**: https://spectreconsole.net/
- **FluentNexus**: https://github.com/Pathoschild/FluentNexus
- **Nexus Mods API**: https://app.swaggerhub.com/apis-docs/NexusMods/nexus-mods_public_api_params_in_form_data/1.0

### Community
- **Project Repository**: https://github.com/psingley/SuperSalsaNOW
- **Issues**: https://github.com/psingley/SuperSalsaNOW/issues
- **Discussions**: https://github.com/psingley/SuperSalsaNOW/discussions

---

*Last Updated: 2025-11-09*
