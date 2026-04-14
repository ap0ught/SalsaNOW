/// App constants for PsyNow (aligned with SalsaNOW behavior; app lists come from remote manifest).
class AppConstants {
  AppConstants._();

  static const String appName = 'PsyNow';

  static const String gfnEnvironmentPath = r'C:\Asgard';

  /// Used only when manifest cannot resolve `directory.json`.
  static const String defaultInstallDir = r'C:\PsyNow';

  /// Same file name as SalsaNOW (`Program.cs` / `SalsaSettings`).
  static const String configFileName = 'SalsaNOWConfig.ini';

  static const String steamServerUrl = 'http://127.10.0.231:9753/shutdown';

  static const String customExplorerTitle = 'CustomExplorer';
  static const String winXShellTitle = 'WinXShell';

  static const List<String> coreShortcuts = [
    'Explorer++.lnk',
  ];

  static const String shortcutsDir = 'Shortcuts';
  static const String backupShortcutsDir = 'Backup Shortcuts';
}
