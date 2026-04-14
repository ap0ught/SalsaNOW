# PsyNow (psy_now)

A GeForce NOW environment customization tool built with Flutter and the Arcane UI framework. In this monorepo it lives under `psy_now/` next to the primary **SalsaNOW** C# app; see [docs/SalsaNOW-and-psy_now.md](../docs/SalsaNOW-and-psy_now.md) for how the two relate and when to prefer each.

## Overview

PsyNow enhances your GeForce NOW gaming sessions by downloading and installing portable applications, custom shell environments, and managing system integrations - all without requiring permanent changes to the cloud VM.

## Features

- **App Downloads & Installation** - Downloads portable apps from GitHub releases and extracts to `C:\PsyNow`
- **Shell Environment Support** - Seelen UI and WinXShell desktop replacements
- **Steam Integration** - Shuts down NVIDIA's Steam proxy to unlock full library access
- **Shortcut Management** - Syncs desktop shortcuts between sessions
- **Window Management** - Automatically closes unwanted GFN windows (CustomExplorer)
- **Progress Tracking** - Real-time download and extraction progress

## Applications (Downloaded on Setup)

| App | Description |
|-----|-------------|
| 7-Zip | File archiver and manager |
| Brave | Privacy-focused web browser |
| Explorer++ | Windows Explorer replacement |
| DepotDownloader | Steam depot/game downloader |
| Notepad++ | Advanced text editor |
| qBittorrent | BitTorrent client |
| System Informer | System monitoring tool |
| Epic Games Portable | Portable Epic Games launcher |
| Epic Games Installer | GFN-compatible Epic installer |
| Ctrl+Tab | Alt-Tab replacement utility |
| Steam Web App | Lightweight Steam web interface |

## Shell Environments

| Shell | Description |
|-------|-------------|
| Seelen UI | Modern Windows shell replacement |
| WinXShell | Classic shell with taskbar |
| WinXShell Steam | Steam-themed variant |
| WinXShell Ubisoft | Ubisoft-themed variant |

## Building

```bash
# Get dependencies
flutter pub get

# Build Windows release
flutter build windows --release
```

Output: `build/windows/x64/runner/Release/psy_now.exe`

## Project Structure

```
lib/
├── main.dart                 # Entry point with Pylon state management
├── screens/
│   ├── home_screen.dart      # Main dashboard
│   ├── settings_screen.dart  # Configuration options
│   └── logs_screen.dart      # Activity log viewer
├── services/
│   ├── app_installer.dart    # Downloads and extracts apps
│   ├── download_service.dart # HTTP download with progress
│   ├── config_service.dart   # INI configuration management
│   ├── environment_service.dart  # GFN environment detection
│   ├── shortcut_service.dart # Desktop shortcut sync
│   ├── steam_service.dart    # Steam proxy shutdown
│   └── window_service.dart   # Win32 window manipulation
├── models/
│   └── app_state.dart        # Application state
└── utils/
    ├── constants.dart        # App configuration and download URLs
    └── win32_utils.dart      # FFI bindings for Windows API
```

## Download Source

All applications are downloaded from [SalsaNOWThings](https://github.com/dpadGuy/SalsaNOWThings/releases/tag/Things) GitHub releases.

## Configuration

Settings are stored in `C:\PsyNow\PsyNowConfig.ini`:

- `SkipShortcutsCreation` - Skip creating desktop shortcuts
- `SkipSeelenUiExecution` - Don't auto-launch Seelen UI
- `BingWallpaperEnabled` - Enable daily Bing wallpaper

## Technical Details

- **Framework**: Flutter with Arcane UI (no Material Design)
- **State Management**: Pylon with RxDart streams
- **Downloads**: HTTP client with streaming progress
- **Windows API**: FFI bindings via `win32` package for FindWindow, SendMessage
- **ZIP Extraction**: `archive` package for decompression

## Requirements

- Windows 10/11
- Internet connection (for downloading apps)
- GeForce NOW environment (optional - works outside GFN with limited features)
- ~500MB disk space for installed apps

## License

MIT
