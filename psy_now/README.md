# PsyNow (psy_now)

A GeForce NOW environment customization tool built with Flutter and the Arcane UI framework. In this monorepo it lives under `psy_now/` next to the primary **SalsaNOW** C# app; see [docs/SalsaNOW-and-psy_now.md](../docs/SalsaNOW-and-psy_now.md) for how the two relate and when to prefer each.

## Overview

PsyNow drives the same GeForce NOW setup as **SalsaNOW**, using the **same remote manifest** as the C# app: set `SALSANOW_MANIFEST_BASE` or place `SalsaNOW.manifest.ini` (with `ManifestBaseUrl=...`) next to the Flutter executable. The install root comes from **`jsons/directory.json`**; apps, desktop shells, silent tools, Steam proxy + USG, and save junctions follow the same `jsons/*` contracts as `SalsaNOW/Program.cs`. If no manifest is configured, PsyNow falls back to `C:\PsyNow` and logs a warning (USG and JSON-driven installs require the host).

## Features

- **Manifest-aligned installs** — `apps.json`, `desktop.json`, `silentapps.json`, `GameSavesPaths.json`
- **Steam integration** — POST shutdown, lockdown JSON, appcache clear, `USG/bleh.exe` from manifest
- **SalsaNOWConfig.ini** — same filename and key semantics as desktop SalsaNOW (`SkipSeelenUiExecution`, etc.)
- **Shortcuts & CustomExplorer** — same helper behavior as before

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
