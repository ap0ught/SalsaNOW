![SalsaNOW_Banner](https://salsanowfiles.work/RepoImages/SalsaNOW_Banner.png)

**Updater: https://salsanowfiles.work/SalsaNOW/SalsaNOWUpdater.exe**

**Documentation Website: https://documentation.salsanowfiles.work**

**Discord Server: https://discord.com/invite/ZQqhh4uSU2**

This repository contains **SalsaNOW** and **psy_now** for the GeForce NOW customization workflow (portable apps, Steam unlock, optional shells), plus **SuperSalsaNOW** ([psingley/SuperSalsaNOW](https://github.com/psingley/SuperSalsaNOW)) under `SuperSalsaNOW/` — a .NET mod-manager CLI with pluggable game profiles (Elden Ring is the first catalog entry, not a silent default). **psy_now** is a Flutter/Dart client derived from [NextdoorPsycho/PsyNow](https://github.com/NextdoorPsycho/PsyNow). SalsaNOW vs psy_now trade-offs: [**docs/SalsaNOW-and-psy_now.md**](docs/SalsaNOW-and-psy_now.md).

| Component | Path | Typical use |
|-----------|------|----------------|
| SalsaNOW | `SalsaNOW/` | Production GFN builds, updater, documentation site |
| psy_now | `psy_now/` | `flutter build windows` (or other targets); optional Flutter UI |
| SuperSalsaNOW | `SuperSalsaNOW/` | `dotnet build SuperSalsaNOW/SuperSalsaNOW.sln`; Steam/mod workflow (Windows-focused) |
