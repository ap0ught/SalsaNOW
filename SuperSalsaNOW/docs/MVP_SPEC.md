# SuperSalsaNOW - MVP Specification

## Project Overview

**SuperSalsaNOW** is a cross-platform mod manager for Elden Ring, inspired by the original SalsaNOW project by dpadGuy. This tool automates the installation and management of Elden Ring mods, with intelligent dependency resolution and conflict detection.

### Goals

1. **Simplify Mod Installation**: One-click installation of complex mod setups
2. **Dependency Management**: Automatically resolve and install mod dependencies
3. **Conflict Detection**: Warn users about file conflicts between mods
4. **Cross-Platform**: Build on macOS, run on Windows (primary target)
5. **Extensible**: Easy to add new mods and installation strategies

### Non-Goals (Phase 1 MVP)

- Full conflict resolution UI
- Mod load order management
- Save game compatibility checking
- Multiple game support (Elden Ring only for MVP)
- Mod updates/version management

## MVP User Stories

### Story 1: Fresh Installation
**As a** user with no Elden Ring installation
**I want to** install both the game and Elden Ring Reforged mod with one tool
**So that** I can start playing the modded game without manual steps

**Acceptance Criteria:**
- [ ] Tool prompts for Steam credentials
- [ ] Tool downloads Elden Ring via DepotDownloader
- [ ] Tool downloads ERR from Nexus Mods
- [ ] Tool creates desktop shortcut to launch modded game
- [ ] User can click shortcut and play immediately

### Story 2: Existing Game Installation
**As a** user with Elden Ring already installed
**I want to** install mods on top of my existing game
**So that** I don't need to re-download the entire game

**Acceptance Criteria:**
- [ ] Tool detects existing Elden Ring installation
- [ ] Tool validates game files are correct
- [ ] Tool installs mods to correct directories
- [ ] Original game files remain untouched

### Story 3: Nexus Mods Integration
**As a** modder
**I want to** download mods automatically from Nexus
**So that** I don't need to manually download and extract files

**Acceptance Criteria:**
- [ ] Tool accepts Nexus API key
- [ ] Tool retrieves mod metadata from Nexus API
- [ ] Tool downloads latest/specified mod version
- [ ] Tool extracts archives automatically

### Story 4: Desktop Shortcut
**As a** user
**I want to** launch the modded game from my desktop
**So that** I can easily start playing without navigating folders

**Acceptance Criteria:**
- [ ] Shortcut created on Windows desktop
- [ ] Shortcut has custom icon (if available)
- [ ] Shortcut launches mod launcher (ERR launcher)
- [ ] Game starts with mods enabled

## Architecture Overview

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    SuperSalsaNOW.Cli                        │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              MainMenu (Spectre.Console)               │  │
│  └──────────────────────┬────────────────────────────────┘  │
│                         │                                    │
│  ┌──────────────────────┴────────────────────────────────┐  │
│  │                   Commands Layer                      │  │
│  │  • InstallEldenRingCommand                           │  │
│  │  • InstallModCommand                                 │  │
│  │  • VerifyGameCommand                                 │  │
│  │  • CreateShortcutCommand                             │  │
│  │  • ConfigureNexusCommand                             │  │
│  └──────────────────────┬────────────────────────────────┘  │
└─────────────────────────┼────────────────────────────────────┘
                          │
         ┌────────────────┴─────────────────┐
         │                                   │
┌────────┴──────────┐            ┌───────────┴──────────┐
│ SuperSalsaNOW.Core│            │ SuperSalsaNOW.Windows│
│                   │            │                      │
│ • IModInstaller   │            │ • Shortcut Service   │
│ • INexusClient    │            │ • DepotDownloader    │
│ • IManifestLoader │            │ • Windows P/Invoke   │
│ • DependencyResolver│          │                      │
│ • ConflictDetector│            │                      │
└───────────────────┘            └──────────────────────┘
```

### Technology Stack

- **.NET 8.0**: Cross-platform framework
- **C#**: Primary language
- **Spectre.Console**: Rich terminal UI
- **Pathoschild.FluentNexus**: Nexus Mods API client
- **DepotDownloader**: Steam content downloader (Windows-only)
- **xUnit**: Unit testing framework

### Platform Support

| Platform | Build | Run | Notes |
|----------|-------|-----|-------|
| macOS    | ✅    | ⚠️  | Development platform; limited runtime features |
| Windows  | ✅    | ✅  | Primary target; full functionality |
| Linux    | ✅    | ⚠️  | Theoretical support; untested |

**Windows-Specific Features:**
- Desktop shortcut creation (P/Invoke to Shell32)
- DepotDownloader execution
- Registry access (future: game detection)

## Data Model

### Manifest Structure

The manifest is a JSON file hosted on GitHub that defines all mods, their sources, and installation strategies.

```json
{
  "directory": {
    "installRoot": "I:\\",
    "gameDirectory": "Games\\ELDENRING",
    "modsDirectory": "Mods"
  },
  "mods": [
    {
      "id": "elden-ring-reforged",
      "name": "Elden Ring Reforged",
      "description": "Complete overhaul mod",
      "nexus": {
        "gameDomain": "eldenring",
        "modId": 541,
        "filePattern": "main"
      },
      "strategy": "ErrLauncher"
    }
  ],
  "tools": [
    {
      "id": "mod-engine-2",
      "name": "Mod Engine 2",
      "url": "https://github.com/soulsmods/ModEngine2/releases/latest",
      "version": "2.1.0"
    }
  ]
}
```

### Domain Models

**ModDefinition:**
- Id: Unique identifier
- Name: Display name
- Description: User-facing description
- Nexus: Source information (game domain, mod ID, file pattern)
- Strategy: Installation approach (ErrLauncher, ModEngine2)

**NexusInfo:**
- GameDomain: Nexus game identifier (e.g., "eldenring")
- ModId: Numeric mod identifier
- FilePattern: Which file to download ("main", "latest", "optional")

**InstallStrategy:**
- ErrLauncher: Mod has built-in launcher
- ModEngine2: Use Mod Engine 2 framework

## Core Interfaces

### IModInstaller
```csharp
public interface IModInstaller
{
    Task<InstallResult> InstallAsync(
        ModDefinition mod,
        InstallOptions options,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyInstallationAsync(
        ModDefinition mod,
        string installPath,
        CancellationToken cancellationToken = default);

    string GetInstallDirectory(
        ModDefinition mod,
        InstallOptions options);
}
```

**Implementations:**
- `EldenRingReforgedInstaller`: Handles ERR-specific installation
- `ModEngine2Installer`: (Future) Handles ME2-based mods

### INexusClient
```csharp
public interface INexusClient
{
    Task<List<NexusModFile>> GetModFilesAsync(
        string gameDomain,
        int modId,
        CancellationToken cancellationToken = default);

    Task<List<NexusDownloadLink>> GetDownloadLinksAsync(
        string gameDomain,
        int modId,
        int fileId,
        CancellationToken cancellationToken = default);

    NexusModFile? SelectFile(
        List<NexusModFile> files,
        string pattern);
}
```

**Implementation:**
- `FluentNexusClient`: Uses Pathoschild.FluentNexus library

### IManifestLoader
```csharp
public interface IManifestLoader
{
    Task<ManifestRoot> LoadAsync(
        CancellationToken cancellationToken = default);
}
```

**Implementation:**
- `GitHubManifestLoader`: Fetches manifest from GitHub repository

### IShortcutService
```csharp
public interface IShortcutService
{
    void CreateDesktopShortcut(
        string name,
        string targetPath,
        string? iconPath = null,
        string? workingDirectory = null);
}
```

**Implementation:**
- `WindowsShortcutService`: Uses Windows Shell32 COM interface

## MVP Workflow

### Phase 1: Initial Setup
1. User launches `SuperSalsaNOW.exe`
2. Tool displays main menu
3. User selects "Configure Nexus API Key"
4. Tool prompts for API key and saves to `appsettings.json`

### Phase 2: Game Installation (Optional)
1. User selects "Install Elden Ring"
2. Tool prompts for Steam credentials
3. Tool downloads DepotDownloader if not present
4. Tool executes DepotDownloader to install game
5. Tool verifies installation (checks for `eldenring.exe`)

### Phase 3: Mod Installation
1. User selects "Install Elden Ring Reforged"
2. Tool loads manifest from GitHub
3. Tool queries Nexus API for latest ERR files
4. Tool downloads mod archive
5. Tool extracts to configured mods directory
6. Tool verifies installation

### Phase 4: Shortcut Creation
1. User selects "Create Desktop Shortcut"
2. Tool locates ERR launcher executable
3. Tool creates Windows shortcut on desktop
4. User clicks shortcut to launch modded game

## Configuration

### appsettings.json
```json
{
  "ManifestBaseUrl": "https://raw.githubusercontent.com/psingley/SuperSalsaNOWThings/main",
  "NexusApiKey": "",
  "Paths": {
    "InstallRoot": "I:\\",
    "GameDirectory": "Games\\ELDENRING",
    "ModsDirectory": "Mods"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Configuration Sources:**
1. `appsettings.json` (defaults)
2. Environment variables (overrides)
3. Command-line arguments (highest priority)

## Testing Strategy

### Unit Tests (Core Library)
- Manifest parsing and validation
- Dependency resolution logic
- Conflict detection algorithms
- Nexus client file selection

### Integration Tests
- GitHub manifest loading
- Nexus API interactions (with test API key)
- File download and extraction

### Manual Testing (Windows Required)
- DepotDownloader execution
- Shortcut creation
- Game launch
- End-to-end installation flow

**Testing Checklist:** See `docs/WINDOWS_TESTING.md`

## Extension Points (Phase 2+)

### Dependency Resolution
```csharp
public interface IDependencyResolver
{
    List<ModDefinition> ResolveDependencies(ModDefinition mod);
    InstallOrder DetermineInstallOrder(List<ModDefinition> mods);
}
```

**Use Cases:**
- Mod requires Mod Engine 2
- Mod requires specific game version
- Mod requires prerequisite mods

### Conflict Detection
```csharp
public interface IConflictDetector
{
    List<FileConflict> DetectConflicts(List<ModDefinition> mods);
    ConflictResolution SuggestResolution(FileConflict conflict);
}
```

**Use Cases:**
- Two mods modify same game file
- Incompatible mod versions
- Load order issues

### Profile Management
**Future Feature:**
- Save/load mod profiles
- Switch between mod configurations
- Export/import profiles for sharing

## Known Limitations (MVP)

1. **Windows-Only Runtime**: DepotDownloader and shortcuts require Windows
2. **Single Mod Focus**: MVP targets Elden Ring Reforged specifically
3. **No Conflict Resolution**: Detects conflicts but doesn't resolve them
4. **No Update Management**: Doesn't track or update mod versions
5. **No Uninstaller**: Cannot safely remove mods (Phase 2 feature)
6. **Manual Steam Credentials**: User must provide Steam login

## Success Metrics

### MVP is Complete When:
- [ ] User can install Elden Ring from scratch via CLI
- [ ] User can install ERR mod from Nexus
- [ ] Desktop shortcut launches modded game
- [ ] All tests pass on Windows 10/11
- [ ] Documentation is complete and accurate
- [ ] Code is published to GitHub with MIT license

### Quality Criteria:
- [ ] No hardcoded paths in code (use configuration)
- [ ] Proper error handling with user-friendly messages
- [ ] Progress reporting for long operations
- [ ] Logging for debugging
- [ ] Unit test coverage >70%
- [ ] Clean separation between Core and Windows libraries

## Future Roadmap (Post-MVP)

### Phase 2: Enhanced Features
- Dependency resolution
- Conflict detection
- Multiple mod support
- Mod profiles

### Phase 3: Advanced Features
- Automatic updates
- Mod uninstallation
- Save game management
- GUI interface (Avalonia?)

### Phase 4: Ecosystem
- Multiple game support
- Community manifest repository
- Mod compatibility database
- Integration with other mod managers

## References

- **Original SalsaNOW**: Inspiration for this project (by dpadGuy)
- **Nexus Mods API**: https://app.swaggerhub.com/apis-docs/NexusMods/nexus-mods_public_api_params_in_form_data/1.0
- **DepotDownloader**: https://github.com/SteamRE/DepotDownloader
- **Elden Ring Reforged**: https://www.nexusmods.com/eldenring/mods/541
- **FluentNexus**: https://github.com/Pathoschild/FluentNexus

## License

This project will be released under the MIT License, allowing free use, modification, and distribution.

## Acknowledgments

- **dpadGuy**: Creator of original SalsaNOW concept
- **Nexus Mods**: For providing the mod hosting platform and API
- **SteamRE**: For DepotDownloader tool
- **Elden Ring Reforged Team**: For creating an amazing mod

---

*Last Updated: 2025-11-09*
