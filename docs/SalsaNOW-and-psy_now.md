# SalsaNOW (C#) and psy_now (Flutter)

Both projects target the same outcome on GeForce NOW: portable tools on disk, NVIDIA Steam proxy shutdown for a full library, optional alternate shells (for example Seelen UI or WinXShell), and shortcut handling. They are maintained **side by side** in this repository; neither is deprecated by the other.

## Quick choice


| Prefer…                                                                                        | Use                        |
| ---------------------------------------------------------------------------------------------- | -------------------------- |
| The build pipeline, updater, and docs site you already ship                                    | **SalsaNOW** (`SalsaNOW/`) |
| A single codebase that can target **multiple OSes** from Flutter, or you already maintain Dart | **psy_now** (`psy_now/`)   |


## Advantages of **SalsaNOW** (C# / .NET)

- **Primary product**: Matches the hosted documentation, updater, and release story for this fork.
- **Deep Windows integration**: Native Win32 and shell behavior are first-class in a desktop .NET app; no Flutter platform channel layer for those paths.
- **Single runtime**: Users run a familiar Windows executable without installing the Flutter SDK for **consumption** (only developers need the SDK for psy_now).
- **Ecosystem**: NuGet, Visual Studio, and existing contributors on the C# stack; MSTest and CI patterns already live in this repo.
- **Upstream velocity**: Fixes and refactors land in `SalsaNOW/` first; psy_now was merged from an older snapshot and may lag until changes are ported.

## psy_now parity with `SalsaNOW/` (manifest-driven)

The Flutter app now follows the same **remote manifest** rules as `RemoteManifest.cs`: environment variable `SALSANOW_MANIFEST_BASE` or `SalsaNOW.manifest.ini` beside the executable. It resolves the install root from `**jsons/directory.json`**, seeds `**SalsaNOWConfig.ini**` from the manifest when missing, installs apps from `**jsons/apps.json**`, shells from `**jsons/desktop.json**`, silent payloads from `**jsons/silentapps.json**`, runs the **Steam proxy / USG** sequence aligned with `SteamManager.ShutdownServerAsync`, and applies `**jsons/GameSavesPaths.json`** junction logic. UI lists are filled from the same JSON catalogs so drift with hard-coded GitHub URLs is reduced.

## Advantages of **psy_now** (Flutter / Dart)

- **Cross-platform UI**: One Dart UI can target Windows today and extend to Linux/Android/macOS where Flutter is supported, with less duplication than maintaining separate native UIs.
- **Declarative UI**: Widget-based layout and theming can be faster to iterate on for designers and Dart-first teams.
- **Hot reload / dev UX**: Flutter’s development loop helps when tuning screens and flows.
- **Portable “control panel” model**: The upstream PsyNow design centers on downloading bundles and driving setup from a light client—useful if you want a non–MSBuild entry point or experimentation without touching the main solution.

## Trade-offs and maintenance

- **Two implementations**: Behavior diverges unless you deliberately port fixes (Steam, shells, shortcuts, etc.) to both trees.
- **psy_now snapshot**: The incorporated [PsyNow](https://github.com/NextdoorPsycho/PsyNow) fork branched from an earlier **SalsaNOW V1.5** era; it does not automatically include every C# commit added since. Treat parity as a manual or backlog task.
- **Build requirements**: SalsaNOW needs **.NET / Visual Studio**; psy_now needs the **Flutter** toolchain (`flutter pub get`, then `flutter build windows --release`, etc.). See `psy_now/README.md` for Flutter-specific detail.

## Build pointers

- **SalsaNOW**: Open `SalsaNOW.sln` and build the main project (see project README or documentation site).
- **psy_now**: From `psy_now/`, run `flutter pub get` and `flutter build windows --release`. Release output is under `psy_now/build/windows/x64/runner/Release/psy_now.exe`.

## License

Both trees remain under the project’s MIT license; PsyNow retained compatibility with the same upstream licensing story.