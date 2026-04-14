# SuperSalsaNOW

> A cross-platform mod manager with pluggable **game profiles** (Elden Ring is the first shipped profile, not an implicit default). Profiles drive Steam app IDs, install folders, and verification paths.

In the **SalsaNOW** monorepo this tree lives under `**SuperSalsaNOW/`** next to `**SalsaNOW/`** (.NET Framework 4.8) and `**psy_now/**` (Flutter). There is **no** assembly reference between `SalsaNOW` and `SuperSalsaNOW.Core` (net48 vs net8); treat integration as repo-level or copy patterns (for example manifest-hosted JSON) rather than a shared DLL until frameworks align.

[.NET](https://dotnet.microsoft.com/)
[License](LICENSE)
[Platform](https://www.microsoft.com/windows)

---

## Overview

SuperSalsaNOW automates mod installation and related setup for whichever **active game** you choose at launch (or via optional `ActiveGameProfileId` in `appsettings.json`). Inspired by the original SalsaNOW project by dpadGuy, this tool provides a modern, cross-platform foundation for mod management.

### Key Features

- **One-Click Mod Installation**: Download and install mods from Nexus Mods automatically
- **Automated Game Setup**: Install Elden Ring via DepotDownloader (no Steam client required)
- **Desktop Shortcuts**: Create shortcuts to launch modded game directly
- **Nexus Integration**: Direct integration with Nexus Mods API for latest mod versions
- **Cross-Platform Development**: Build on macOS/Linux, run on Windows
- **Extensible Architecture**: Clean separation of concerns for easy feature additions

### Current Status

**Phase 1 (MVP)** - Under Active Development

Focus: Elden Ring Reforged (ERR) installation and launch automation.

---

## Quick Start

### Prerequisites

- **Windows 10/11** (primary platform)
- **.NET 8.0 Runtime** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Nexus Mods Account** (free) with API key ([Get API Key](https://www.nexusmods.com/users/myaccount?tab=api))
- **Steam Account** (for game installation via DepotDownloader)

### Installation

1. **Download Latest Release**
  ```
   Download SuperSalsaNOW.exe from GitHub Releases
  ```
2. **Run Application**
  ```cmd
   SuperSalsaNOW.exe
  ```
3. **Configure Nexus API Key**
  - Select "Configure Nexus API Key" from menu
  - Paste your API key from Nexus Mods
4. **Install Elden Ring (Optional)**
  - Select "Install Elden Ring"
  - Enter Steam credentials
  - Wait for download (~50GB, 30-60 minutes)
5. **Install Elden Ring Reforged**
  - Select "Install Elden Ring Reforged (ERR)"
  - Mod downloads and installs automatically
6. **Create Shortcut**
  - Select "Create Desktop Shortcut"
  - Shortcut appears on desktop
7. **Launch Modded Game**
  - Double-click desktop shortcut
  - ERR launcher opens
  - Click "Launch Game"

---

## Architecture Overview

SuperSalsaNOW is built as a multi-project .NET solution with clear separation between cross-platform business logic and Windows-specific implementations.

```
┌─────────────────────────────────────────────────────────────┐
│              SuperSalsaNOW.Cli (Console UI)                 │
│         Spectre.Console | Interactive Menus                 │
└─────────────────────┬───────────────────────────────────────┘
                      │
      ┌───────────────┴───────────────┐
      │                               │
┌─────┴──────────────┐   ┌────────────┴──────────────┐
│  SuperSalsaNOW.Core│   │  SuperSalsaNOW.Windows    │
│  Platform-Agnostic │   │  Windows-Specific         │
│  • Mod Installation│   │  • Desktop Shortcuts      │
│  • Nexus API       │   │  • DepotDownloader        │
│  • Manifest Mgmt   │   │  • Windows P/Invoke       │
└────────────────────┘   └───────────────────────────┘
```

### Projects


| Project                      | Purpose                          | Platform       |
| ---------------------------- | -------------------------------- | -------------- |
| **SuperSalsaNOW.Cli**        | Interactive console application  | Cross-platform |
| **SuperSalsaNOW.Core**       | Business logic, models, services | Cross-platform |
| **SuperSalsaNOW.Windows**    | Windows-specific features        | Windows only   |
| **SuperSalsaNOW.Core.Tests** | Unit and integration tests       | Cross-platform |


**See:** [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed architecture documentation.

---

## Building from Source

### Prerequisites for Development

- **.NET 8.0 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Git**
- **Code Editor** (Visual Studio, VS Code, or Rider)

### Build Steps

```bash
# Clone repository
git clone https://github.com/psingley/SuperSalsaNOW.git
cd SuperSalsaNOW

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run CLI application
cd src/SuperSalsaNOW.Cli
dotnet run
```

### Publish for Windows

```bash
# Framework-dependent (requires .NET 8 runtime on target)
dotnet publish src/SuperSalsaNOW.Cli -c Release -r win-x64 --self-contained false

# Self-contained (bundles .NET runtime, ~60MB larger)
dotnet publish src/SuperSalsaNOW.Cli -c Release -r win-x64 --self-contained true

# Single-file executable
dotnet publish src/SuperSalsaNOW.Cli -c Release -r win-x64 \
  --self-contained false \
  /p:PublishSingleFile=true
```

**Output:** `src/SuperSalsaNOW.Cli/bin/Release/net8.0/win-x64/publish/SuperSalsaNOW.exe`

**See:** [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for full developer guide.

---

## Configuration

Configuration is managed via `appsettings.json`:

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

### Configuration Options


| Setting               | Description                      | Default           |
| --------------------- | -------------------------------- | ----------------- |
| `ManifestBaseUrl`     | URL for mod manifest repository  | GitHub repo       |
| `NexusApiKey`         | Your Nexus Mods API key          | (empty)           |
| `Paths.InstallRoot`   | Root directory for installations | `I:\`             |
| `Paths.GameDirectory` | Elden Ring game directory        | `Games\ELDENRING` |
| `Paths.ModsDirectory` | Mods installation directory      | `Mods`            |


---

## Technology Stack

### Core Technologies

- **.NET 8.0** - Runtime framework
- **C# 12.0** - Programming language
- **Spectre.Console** - Rich terminal UI
- **FluentNexus** - Nexus Mods API client
- **xUnit** - Unit testing framework

### External Services

- **Nexus Mods API** - Mod metadata and downloads
- **GitHub** - Manifest hosting
- **DepotDownloader** - Steam content downloader (Windows)

---

## Documentation

Comprehensive documentation is available in the [docs/](docs/) directory:


| Document                                      | Description                              |
| --------------------------------------------- | ---------------------------------------- |
| [MVP_SPEC.md](docs/MVP_SPEC.md)               | MVP specification and user stories       |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md)       | System architecture and design decisions |
| [DEVELOPMENT.md](docs/DEVELOPMENT.md)         | Developer guide and workflows            |
| [WINDOWS_TESTING.md](docs/WINDOWS_TESTING.md) | Windows testing procedures               |


---

## Roadmap

### Phase 1: MVP (Current)

- Core architecture and project structure
- Nexus Mods API integration
- Manifest loading from GitHub
- Elden Ring Reforged installer
- DepotDownloader wrapper for game installation
- Desktop shortcut creation
- Windows end-to-end testing
- v1.0.0 release

### Phase 2: Enhanced Features

- Dependency resolution (mod requires other mods)
- Conflict detection (file conflicts between mods)
- Multiple mod support (install any mod from manifest)
- Mod profiles (switch between mod configurations)

### Phase 3: Advanced Features

- Automatic mod updates
- Safe mod uninstallation
- Save game compatibility checking
- GUI interface (Avalonia or Blazor)

### Phase 4: Ecosystem

- Multiple game support (Dark Souls, Sekiro, etc.)
- Community manifest repository
- Mod compatibility database
- Integration with other mod managers

---

## Contributing

Contributions are welcome! This project is in early development and needs:

- **Windows Testers**: Validate Windows-specific features
- **Code Contributors**: New features, bug fixes, refactoring
- **Documentation**: Improve guides, add examples
- **Mod Manifest Entries**: Add more mods to manifest

### How to Contribute

1. **Fork Repository**
  ```bash
   git clone https://github.com/YOUR-USERNAME/SuperSalsaNOW.git
  ```
2. **Create Feature Branch**
  ```bash
   git checkout -b feature/my-new-feature
  ```
3. **Make Changes**
  - Write tests first (TDD)
  - Follow code conventions (see [DEVELOPMENT.md](docs/DEVELOPMENT.md))
  - Update documentation
4. **Test Thoroughly**
  ```bash
   dotnet test
   dotnet build -c Release
  ```
5. **Submit Pull Request**
  - Clear description of changes
  - Reference any related issues
  - Include screenshots/logs if applicable

### Development Guidelines

- **Test-Driven Development**: Write tests before code
- **Clean Code**: Follow C# conventions and SOLID principles
- **Documentation**: Update docs for any public API changes
- **Cross-Platform**: Keep Core library platform-agnostic
- **Windows Testing**: Validate Windows features on real hardware

---

## Testing

### Unit Tests

```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity normal

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Windows Integration Testing

Windows-specific features require testing on Windows hardware:

- DepotDownloader execution
- Desktop shortcut creation
- Game launch verification

**See:** [docs/WINDOWS_TESTING.md](docs/WINDOWS_TESTING.md) for detailed testing procedures.

---

## Troubleshooting

### Common Issues

**Issue:** "SuperSalsaNOW.exe not recognized"

**Solution:** Install .NET 8.0 Runtime from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)

---

**Issue:** DepotDownloader fails with "Access Denied"

**Solution:**

1. Run SuperSalsaNOW as Administrator
2. Temporarily disable antivirus
3. Check folder permissions

---

**Issue:** Nexus API returns "401 Unauthorized"

**Solution:**

1. Verify API key is correct
2. Regenerate API key on Nexus Mods website
3. Reconfigure via menu option

---

**Issue:** Desktop shortcut shows "Target not found"

**Solution:**

1. Verify mod installation path
2. Check ERR launcher executable exists
3. Update `appsettings.json` paths

---

**See:** [docs/DEVELOPMENT.md#troubleshooting](docs/DEVELOPMENT.md#troubleshooting) for more solutions.

---

## License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 SuperSalsaNOW Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## Acknowledgments

### Inspiration and Tools

- **dpadGuy** - Creator of the original SalsaNOW concept that inspired this project
- **Nexus Mods** - For providing the mod hosting platform and robust API
- **SteamRE Team** - For DepotDownloader, enabling Steam content downloads without the client
- **Elden Ring Reforged Team** - For creating an incredible overhaul mod

### Libraries and Frameworks

- **Pathoschild.FluentNexus** - Excellent .NET client for Nexus Mods API
- **Spectre.Console** - Beautiful terminal UI library
- **.NET Team** - For the amazing cross-platform framework

### Community

Thank you to all contributors, testers, and users who help make this project better!

---

## Contact

- **GitHub Issues**: [Report bugs or request features](https://github.com/psingley/SuperSalsaNOW/issues)
- **Discussions**: [Ask questions or share ideas](https://github.com/psingley/SuperSalsaNOW/discussions)
- **Project Repository**: [https://github.com/psingley/SuperSalsaNOW](https://github.com/psingley/SuperSalsaNOW)

---

## Disclaimer

This tool is not affiliated with FromSoftware, Bandai Namco, Steam, or Nexus Mods. Elden Ring is a trademark of FromSoftware, Inc. Use this tool at your own risk. Always backup your game saves before modding.

**Modding Notice:** Installing mods may:

- Void your game warranty
- Cause game instability or crashes
- Be incompatible with online play
- Require specific game versions

Always verify mod compatibility and safety before installation.

---

*Built with .NET 8.0 | Inspired by SalsaNOW | Cross-platform development for Windows deployment*