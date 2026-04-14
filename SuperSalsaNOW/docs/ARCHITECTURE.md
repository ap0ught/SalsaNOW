# SuperSalsaNOW - Architecture Documentation

## Table of Contents

1. [System Overview](#system-overview)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Data Flow](#data-flow)
5. [Interface Contracts](#interface-contracts)
6. [Extension Points](#extension-points)
7. [Technology Stack](#technology-stack)
8. [Design Decisions](#design-decisions)

---

## System Overview

SuperSalsaNOW is a multi-project .NET solution designed for cross-platform development with Windows-specific runtime capabilities. The architecture follows clean architecture principles with clear separation between business logic, platform-specific code, and presentation layers.

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          User Interface Layer                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │             SuperSalsaNOW.Cli (Console Application)              │   │
│  │  • Spectre.Console for rich terminal UI                         │   │
│  │  • Command pattern for user actions                             │   │
│  │  • Menu-driven interface                                        │   │
│  │  • Configuration management (appsettings.json)                  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │                               │
┌───────────────────┴───────────────┐   ┌───────────┴─────────────────────┐
│  SuperSalsaNOW.Core (Library)     │   │  SuperSalsaNOW.Windows (Library) │
│                                   │   │                                  │
│  Platform-Agnostic Business Logic │   │  Windows-Specific Implementation │
│  • Domain models                  │   │  • Desktop shortcuts (Shell32)  │
│  • Service interfaces             │   │  • DepotDownloader wrapper      │
│  • Core algorithms                │   │  • Registry access (future)     │
│  • Utilities (HTTP, ZIP)          │   │  • Windows P/Invoke             │
│                                   │   │                                  │
│  Dependencies:                    │   │  Dependencies:                   │
│  • FluentNexus (Nexus API)       │   │  • Core library                  │
│  • Microsoft.Extensions.*         │   │  • .NET 8.0-windows TFM         │
│  • Newtonsoft.Json                │   │                                  │
└───────────────────────────────────┘   └──────────────────────────────────┘
                    │
                    │
┌───────────────────┴───────────────────────────────────────────────────┐
│                     External Dependencies                             │
│  • Nexus Mods API (REST)                                             │
│  • GitHub (Manifest hosting)                                         │
│  • DepotDownloader (Steam content)                                   │
└───────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Framework | Platform |
|-----------|---------------|-----------|----------|
| **SuperSalsaNOW.Cli** | User interaction, orchestration | .NET 8.0 | Cross-platform |
| **SuperSalsaNOW.Core** | Business logic, models, services | .NET 8.0 | Cross-platform |
| **SuperSalsaNOW.Windows** | Windows-specific features | .NET 8.0-windows | Windows only |
| **SuperSalsaNOW.Core.Tests** | Unit and integration tests | .NET 8.0 | Cross-platform |

---

## Project Structure

```
SuperSalsaNOW/
├── src/
│   ├── SuperSalsaNOW.Core/              # Cross-platform core library
│   │   ├── Interfaces/
│   │   │   ├── IManifestLoader.cs       # Manifest loading contract
│   │   │   ├── INexusClient.cs          # Nexus API abstraction
│   │   │   ├── IModInstaller.cs         # Mod installation contract
│   │   │   ├── IShortcutService.cs      # Shortcut creation abstraction
│   │   │   ├── IDependencyResolver.cs   # (Future) Dependency resolution
│   │   │   └── IConflictDetector.cs     # (Future) Conflict detection
│   │   │
│   │   ├── Models/
│   │   │   ├── ManifestModels.cs        # Manifest structure (records)
│   │   │   ├── InstallModels.cs         # Installation data models
│   │   │   └── NexusModels.cs           # Nexus API response models
│   │   │
│   │   ├── Services/
│   │   │   ├── GitHubManifestLoader.cs  # Loads manifest from GitHub
│   │   │   ├── FluentNexusClient.cs     # Nexus API client wrapper
│   │   │   └── EldenRingReforgedInstaller.cs  # ERR-specific installer
│   │   │
│   │   └── Utilities/
│   │       ├── DownloadHelper.cs        # HTTP download with progress
│   │       └── ZipHelper.cs             # Archive extraction
│   │
│   ├── SuperSalsaNOW.Windows/           # Windows-specific library
│   │   ├── Services/
│   │   │   ├── WindowsShortcutService.cs  # IShortcutService implementation
│   │   │   └── DepotDownloaderService.cs  # Steam game downloader
│   │   │
│   │   └── Interop/
│   │       └── WindowsAPI.cs            # P/Invoke declarations
│   │
│   └── SuperSalsaNOW.Cli/               # Console application
│       ├── Commands/                     # Command pattern handlers
│       │   ├── ConfigureNexusCommand.cs
│       │   ├── InstallEldenRingCommand.cs
│       │   ├── VerifyGameCommand.cs
│       │   ├── InstallModCommand.cs
│       │   └── CreateShortcutCommand.cs
│       │
│       ├── Menu/
│       │   └── MainMenu.cs              # Interactive menu system
│       │
│       ├── Configuration/
│       │   └── AppSettings.cs           # Configuration model
│       │
│       ├── ServiceCollectionExtensions.cs  # DI registration
│       ├── Program.cs                    # Entry point
│       └── appsettings.json             # Application configuration
│
├── tests/
│   └── SuperSalsaNOW.Core.Tests/        # Unit tests
│       ├── Services/                     # Service tests
│       └── Utilities/                    # Utility tests
│
├── docs/                                 # Documentation
│   ├── MVP_SPEC.md
│   ├── ARCHITECTURE.md (this file)
│   ├── DEVELOPMENT.md
│   └── WINDOWS_TESTING.md
│
├── scripts/                              # Build and utility scripts
│
├── SuperSalsaNOW.sln                    # Solution file
└── README.md                            # Project overview
```

---

## Core Components

### 1. Manifest System

The manifest is the heart of the system, defining all mods, tools, and installation strategies.

**Design Pattern:** Repository Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                    Manifest Flow                            │
└─────────────────────────────────────────────────────────────┘

   GitHub Repository                    Application
   ┌──────────────┐                    ┌──────────────┐
   │ manifest.json│──── HTTP GET ─────→│ IManifestLoader│
   └──────────────┘                    └──────┬─────────┘
                                              │
                                              ↓
                                       ┌──────────────┐
                                       │ ManifestRoot │
                                       │ (Deserialized)│
                                       └──────┬─────────┘
                                              │
                     ┌────────────────────────┼────────────────────────┐
                     ↓                        ↓                        ↓
              ┌──────────────┐        ┌──────────────┐        ┌──────────────┐
              │ DirectoryConfig│      │ ModDefinition[]│      │ToolDefinition[]│
              └──────────────┘        └──────────────┘        └──────────────┘
```

**Manifest Model Hierarchy:**

```csharp
ManifestRoot
├── DirectoryConfig
│   ├── InstallRoot: string
│   ├── GameDirectory: string
│   └── ModsDirectory: string
│
├── Mods: List<ModDefinition>
│   └── ModDefinition
│       ├── Id: string
│       ├── Name: string
│       ├── Description: string
│       ├── Nexus: NexusInfo
│       │   ├── GameDomain: string
│       │   ├── ModId: int
│       │   └── FilePattern: string
│       └── Strategy: InstallStrategy (enum)
│
└── Tools: List<ToolDefinition>
    └── ToolDefinition
        ├── Id: string
        ├── Name: string
        ├── Url: string
        └── Version: string
```

**Why This Design:**
- **JSON-based**: Easy to edit, version control, and host
- **GitHub-hosted**: Free CDN, version history, community contributions
- **Record types**: Immutable, concise, suitable for data transfer
- **Extensible**: Easy to add new properties without breaking existing code

### 2. Mod Installation Pipeline

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Mod Installation Flow                            │
└─────────────────────────────────────────────────────────────────────┘

User Request
     │
     ↓
┌──────────────────┐
│ InstallModCommand│  (CLI Layer)
└────────┬─────────┘
         │
         ↓
┌──────────────────┐
│  IModInstaller   │  (Core Interface)
└────────┬─────────┘
         │
         ↓
┌────────────────────────────┐
│EldenRingReforgedInstaller  │  (Core Implementation)
└────────┬───────────────────┘
         │
         ├─────→ INexusClient.GetModFilesAsync()
         │       └─→ Nexus API: GET /v1/games/{game}/mods/{id}/files
         │
         ├─────→ INexusClient.SelectFile(pattern)
         │       └─→ Filter files by pattern ("main", "latest", etc.)
         │
         ├─────→ INexusClient.GetDownloadLinksAsync()
         │       └─→ Nexus API: POST /v1/games/{game}/mods/{id}/files/{file_id}/download_link
         │
         ├─────→ DownloadHelper.DownloadFileAsync()
         │       └─→ HTTP download with progress reporting
         │
         ├─────→ ZipHelper.ExtractAsync()
         │       └─→ Extract archive to install directory
         │
         └─────→ Return InstallResult
                 └─→ {Success, InstallPath, Errors, Warnings}
```

**Key Abstractions:**

1. **IModInstaller**: Strategy pattern for different installation types
2. **INexusClient**: Adapter pattern for Nexus API
3. **Progress Reporting**: IProgress<double> for UI updates
4. **Cancellation**: CancellationToken for user cancellation

### 3. Dependency Injection Container

```
┌─────────────────────────────────────────────────────────────┐
│              Service Registration (Program.cs)               │
└─────────────────────────────────────────────────────────────┘

Host.CreateDefaultBuilder()
  │
  ├─→ Configuration
  │   ├─ appsettings.json
  │   ├─ Environment Variables
  │   └─ Command-line Args
  │
  ├─→ Logging
  │   └─ Console Logger (Serilog in future)
  │
  └─→ Services (via ServiceCollectionExtensions)
      │
      ├─→ Core Services
      │   ├─ IManifestLoader → GitHubManifestLoader
      │   ├─ INexusClient → FluentNexusClient
      │   └─ IModInstaller → EldenRingReforgedInstaller
      │
      ├─→ Windows Services (conditional)
      │   ├─ IShortcutService → WindowsShortcutService
      │   └─ DepotDownloaderService
      │
      └─→ CLI Components
          ├─ MainMenu
          └─ Commands (all command classes)
```

**Service Lifetimes:**

| Service | Lifetime | Reason |
|---------|----------|--------|
| Configuration | Singleton | Shared state, no mutation |
| IManifestLoader | Singleton | Stateless, caches manifest |
| INexusClient | Scoped | Per-operation, manages API rate limits |
| IModInstaller | Transient | Per-installation, tracks state |
| Commands | Transient | Per-invocation, short-lived |

---

## Data Flow

### End-to-End: Install Elden Ring Reforged

```
┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
│  User    │   │  CLI     │   │  Core    │   │  Nexus   │   │  File    │
│          │   │  Layer   │   │  Services│   │  API     │   │  System  │
└─────┬────┘   └────┬─────┘   └────┬─────┘   └────┬─────┘   └────┬─────┘
      │             │              │              │              │
      │ Select      │              │              │              │
      │ "Install ERR"│             │              │              │
      ├────────────→│              │              │              │
      │             │ LoadManifest()│             │              │
      │             ├─────────────→│              │              │
      │             │              │ HTTP GET     │              │
      │             │              ├─────────────→│              │
      │             │              │ manifest.json│              │
      │             │              │←─────────────┤              │
      │             │ ManifestRoot │              │              │
      │             │←─────────────┤              │              │
      │             │              │              │              │
      │             │ InstallAsync()│             │              │
      │             ├─────────────→│              │              │
      │             │              │ GetModFiles()│              │
      │             │              ├─────────────→│              │
      │             │              │ FileList[]   │              │
      │             │              │←─────────────┤              │
      │             │              │ SelectFile() │              │
      │             │              ├──────┐       │              │
      │             │              │      │       │              │
      │             │              │←─────┘       │              │
      │             │              │ GetDownloadLinks()          │
      │             │              ├─────────────→│              │
      │             │              │ DownloadURL[]│              │
      │             │              │←─────────────┤              │
      │             │              │              │ DownloadFile()│
      │             │              │              ├─────────────→│
      │             │              │              │ mod.zip      │
      │             │              │              │←─────────────┤
      │             │              │              │ ExtractZip() │
      │             │              │              ├─────────────→│
      │             │              │              │ Files[]      │
      │             │              │              │←─────────────┤
      │             │ InstallResult│              │              │
      │             │←─────────────┤              │              │
      │ Display     │              │              │              │
      │ "Success"   │              │              │              │
      │←────────────┤              │              │              │
      │             │              │              │              │
```

---

## Interface Contracts

### IManifestLoader

**Purpose:** Abstract manifest source (GitHub, local file, etc.)

```csharp
public interface IManifestLoader
{
    Task<ManifestRoot> LoadAsync(CancellationToken cancellationToken = default);
}
```

**Implementations:**
- `GitHubManifestLoader`: Fetches from raw.githubusercontent.com
- (Future) `LocalManifestLoader`: Loads from local JSON file
- (Future) `CachedManifestLoader`: Decorator with TTL caching

### INexusClient

**Purpose:** Abstract Nexus Mods API for testability and flexibility

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

**Why This Interface:**
- Testable: Mock for unit tests
- Flexible: Swap implementations (e.g., local cache)
- Rate-limited: Can implement retry/backoff logic

### IModInstaller

**Purpose:** Strategy pattern for different mod types

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
- `EldenRingReforgedInstaller`: MVP implementation for ERR
- (Future) `ModEngine2Installer`: For ME2-based mods
- (Future) `ManualInstaller`: For mods requiring manual steps

### IShortcutService

**Purpose:** Platform abstraction for desktop shortcuts

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

**Implementations:**
- `WindowsShortcutService`: Uses Shell32 COM (Windows only)
- (Future) `MacShortcutService`: Creates .app bundle (macOS)
- (Future) `LinuxShortcutService`: Creates .desktop file (Linux)

---

## Extension Points

### Phase 2: Dependency Resolution

**Interface:**
```csharp
public interface IDependencyResolver
{
    List<ModDefinition> ResolveDependencies(ModDefinition mod);
    InstallOrder DetermineInstallOrder(List<ModDefinition> mods);
}
```

**Use Case:**
1. User selects "Install Mod X"
2. Resolver determines Mod X requires Mod Engine 2 + Seamless Co-op
3. System prompts: "This will install 3 mods: Mod X, ME2, Seamless Co-op. Continue?"
4. Installs in correct order: ME2 → Seamless Co-op → Mod X

**Algorithm:**
- Topological sort for dependency graph
- Cycle detection for circular dependencies
- Conflict detection for incompatible mods

### Phase 2: Conflict Detection

**Interface:**
```csharp
public interface IConflictDetector
{
    List<FileConflict> DetectConflicts(List<ModDefinition> mods);
    ConflictResolution SuggestResolution(FileConflict conflict);
}
```

**Types of Conflicts:**
1. **File Conflicts**: Two mods modify same file
2. **Version Conflicts**: Mod requires different game version
3. **Incompatibility**: Mods explicitly incompatible (manifest-defined)

**Resolution Strategies:**
1. **Load Order**: Define which mod loads last (wins)
2. **Merge**: (Advanced) Merge compatible changes
3. **Warn**: Inform user, let them decide

### Phase 3: Profile System

**Concept:**
- User creates profiles: "Vanilla", "PvE Build", "Challenge Run"
- Each profile has enabled mods + configuration
- Switch profiles with one click

**Data Model:**
```csharp
public record ModProfile(
    string Name,
    string Description,
    List<string> EnabledModIds,
    Dictionary<string, string> ConfigOverrides
);
```

---

## Technology Stack

### Core Technologies

| Technology | Purpose | Version |
|------------|---------|---------|
| **.NET** | Runtime framework | 8.0 |
| **C#** | Programming language | 12.0 |
| **xUnit** | Unit testing | Latest |
| **Spectre.Console** | Rich terminal UI | Latest |
| **FluentNexus** | Nexus Mods API client | Latest |
| **Newtonsoft.Json** | JSON serialization | Latest |

### Windows-Specific

| Technology | Purpose |
|------------|---------|
| **DepotDownloader** | Steam content downloader |
| **Shell32 COM** | Desktop shortcut creation |
| **Registry API** | (Future) Game detection |

### External Services

| Service | Purpose | Authentication |
|---------|---------|----------------|
| **Nexus Mods API** | Mod metadata, downloads | API Key (user-provided) |
| **GitHub** | Manifest hosting | None (public repo) |
| **Steam** | Game download (via DepotDownloader) | Username/password |

---

## Design Decisions

### 1. Why .NET 8.0?

**Pros:**
- Cross-platform: Build on macOS, run on Windows
- Modern language features (records, pattern matching, nullable types)
- Excellent CLI tooling (dotnet CLI)
- Rich ecosystem (NuGet packages)
- Performance: Native AOT compilation possible

**Cons:**
- Requires .NET runtime (unless self-contained)
- Larger executable size than native code

**Decision:** Benefits outweigh costs for this use case

### 2. Why Separate Core and Windows Libraries?

**Rationale:**
- **Testability**: Core logic testable on any platform
- **Portability**: Core can run on macOS/Linux (limited features)
- **Maintenance**: Clear boundary between platform-agnostic and platform-specific
- **Future-proofing**: Easy to add Linux/macOS-specific implementations

**Example:**
```
SuperSalsaNOW.Core       → .NET 8.0 (cross-platform)
SuperSalsaNOW.Windows    → .NET 8.0-windows (Windows-only)
SuperSalsaNOW.Cli        → .NET 8.0 (cross-platform, conditionally uses Windows lib)
```

### 3. Why GitHub for Manifest Hosting?

**Alternatives Considered:**
1. **Embedded in app**: No updates without recompile
2. **Database/API**: Overkill, requires hosting
3. **GitHub**: Free, versioned, CDN, community-editable

**Decision:** GitHub provides best balance of simplicity and flexibility

### 4. Why Command Pattern for CLI?

**Pattern:**
```csharp
public interface ICommand
{
    void Execute();
    Task ExecuteAsync();
}
```

**Benefits:**
- Separation of concerns: Each command is isolated
- Testability: Mock dependencies easily
- Extensibility: Add new commands without modifying menu
- Undo/Redo: (Future) Track command history

### 5. Why Records for Models?

**Before (Classes):**
```csharp
public class ModDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    // Mutable, verbose, error-prone
}
```

**After (Records):**
```csharp
public record ModDefinition(string Id, string Name);
// Immutable, concise, value semantics
```

**Benefits:**
- Immutability: Models are data transfer objects, shouldn't change
- Conciseness: Less boilerplate
- Value equality: Compare by value, not reference

### 6. Why FluentNexus Instead of Raw HTTP?

**Alternatives:**
1. **HttpClient + manual JSON**: More control, more code
2. **FluentNexus**: Pre-built, tested, maintained

**Decision:** FluentNexus saves time and reduces errors

### 7. Why Spectre.Console for UI?

**Alternatives:**
1. **Console.WriteLine**: Basic, no progress bars or colors
2. **Terminal.Gui**: Full TUI framework (overkill)
3. **Spectre.Console**: Rich features, simple API

**Decision:** Spectre.Console provides excellent UX without complexity

---

## Deployment Architecture

### Single-File Executable

```bash
dotnet publish -c Release -r win-x64 --self-contained false \
  /p:PublishSingleFile=true
```

**Output:**
- `SuperSalsaNOW.exe` (single executable)
- Requires .NET 8.0 runtime installed on target machine

**Alternatives:**
- **Self-contained**: Bundle runtime (~60MB larger)
- **Native AOT**: Compile to native code (experimental, limited library support)

**Decision:** Framework-dependent for MVP (smaller size, simpler deployment)

### Configuration Storage

```
C:\Users\<User>\AppData\Local\SuperSalsaNOW\
├── appsettings.json         # User configuration
├── manifest-cache.json      # Cached manifest (future)
└── logs\                    # Application logs
```

**Rationale:**
- Standard location for user data
- Separate from executable (updates don't lose config)
- Easy to backup/share

---

## Security Considerations

### 1. API Key Storage

**Current:** Plain text in `appsettings.json`

**Risks:**
- Key visible to anyone with file access
- Committed to version control if not careful

**Future Improvements:**
- Windows Credential Manager (DPAPI)
- Prompt for key on first run, don't persist

### 2. Steam Credentials

**Current:** Passed as command-line arguments to DepotDownloader

**Risks:**
- Visible in process list
- Not persisted (user re-enters each time)

**Mitigation:**
- Warn user about Steam Guard requirement
- Never log credentials
- Consider encrypted storage (future)

### 3. Downloaded Files

**Current:** Trust Nexus Mods CDN

**Risks:**
- Malicious mods on Nexus
- Man-in-the-middle (if not HTTPS)

**Mitigation:**
- Verify HTTPS for all downloads
- (Future) Checksum validation
- (Future) Scan with Windows Defender API

---

## Performance Considerations

### 1. Manifest Caching

**Problem:** Fetching manifest on every operation is slow

**Solution (Phase 2):**
```csharp
public class CachedManifestLoader : IManifestLoader
{
    private readonly IManifestLoader _inner;
    private readonly TimeSpan _ttl;
    private ManifestRoot? _cached;
    private DateTime _lastFetch;

    public async Task<ManifestRoot> LoadAsync(CancellationToken ct)
    {
        if (_cached != null && DateTime.Now - _lastFetch < _ttl)
            return _cached;

        _cached = await _inner.LoadAsync(ct);
        _lastFetch = DateTime.Now;
        return _cached;
    }
}
```

### 2. Parallel Downloads

**Future Optimization:**
- Download multiple mods in parallel
- Use `Task.WhenAll` for concurrent operations
- Rate-limit to avoid Nexus API throttling

### 3. Progress Reporting

**Current:** IProgress<double> for percentage updates

**Best Practice:**
- Report every 1% change (avoid flooding UI)
- Use `BufferBlock` for backpressure

---

## Testing Strategy

### Unit Tests

**Targets:**
- Manifest parsing (deserialize JSON correctly)
- File selection logic (pattern matching)
- Dependency resolution algorithms
- Conflict detection logic

**Mocking:**
```csharp
var mockNexusClient = new Mock<INexusClient>();
mockNexusClient
    .Setup(x => x.GetModFilesAsync("eldenring", 541, It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<NexusModFile> { /* test data */ });
```

### Integration Tests

**Targets:**
- GitHub manifest loading (real HTTP call)
- Nexus API (with test API key, rate-limited)
- File download and extraction

**Strategy:**
- Use `[Trait("Category", "Integration")]` to separate
- Run in CI with environment variables for API keys

### Manual Tests

**Windows-Only Features:**
- DepotDownloader execution
- Shortcut creation
- Game launch

**Checklist:** See `WINDOWS_TESTING.md`

---

## Future Architecture Evolution

### Phase 2: Plugin System

**Concept:**
- Third-party developers create installer plugins
- Plugins implement `IModInstaller`
- Dynamically loaded via reflection

**Example:**
```csharp
public class CommunityModInstaller : IModInstaller
{
    // Custom installation logic
}
```

### Phase 3: GUI Frontend

**Options:**
1. **Avalonia**: Cross-platform XAML UI
2. **Blazor**: Web-based UI
3. **MAUI**: Native cross-platform

**Architecture:**
- Core and Windows libraries remain unchanged
- Add `SuperSalsaNOW.Gui` project
- Share same interfaces and services

### Phase 4: Cloud Sync

**Concept:**
- Sync mod profiles across devices
- Cloud save backups
- Community profile sharing

**Architecture:**
- Add `SuperSalsaNOW.Cloud` library
- Azure Functions or AWS Lambda
- Cosmos DB or DynamoDB for storage

---

## Glossary

| Term | Definition |
|------|------------|
| **Manifest** | JSON file defining mods, tools, and installation strategies |
| **Mod Definition** | Metadata about a mod (name, source, strategy) |
| **Install Strategy** | Method for installing/launching a mod (ERR launcher, ME2, etc.) |
| **Nexus Info** | Nexus Mods API identifiers (game domain, mod ID, file pattern) |
| **DepotDownloader** | Tool for downloading Steam games without Steam client |
| **ERR** | Elden Ring Reforged (primary mod for MVP) |
| **ME2** | Mod Engine 2 (modding framework) |

---

*Last Updated: 2025-11-09*
