![SalsaNOW_Banner](https://salsanowfiles.work/RepoImages/SalsaNOW_Banner.png)

**Updater: https://salsanowfiles.work/SalsaNOW/SalsaNOWUpdater.exe**

**Documentation Website: https://documentation.salsanowfiles.work**

**Discord Server: https://discord.com/invite/ZQqhh4uSU2**

This repository contains **two** clients for the same GeForce NOW customization workflow (portable apps, Steam unlock, optional shells). **SalsaNOW** in `SalsaNOW/` is the primary C# application; **psy_now** in `psy_now/` is a Flutter/Dart client derived from [NextdoorPsycho/PsyNow](https://github.com/NextdoorPsycho/PsyNow). When to use which, build steps, and trade-offs are documented in [**docs/SalsaNOW-and-psy_now.md**](docs/SalsaNOW-and-psy_now.md).

| Client | Path | Typical use |
|--------|------|----------------|
| SalsaNOW | `SalsaNOW/` | Production builds, updater, full alignment with the documentation site |
| psy_now | `psy_now/` | `flutter build windows` (or other targets); optional cross-platform UI |
