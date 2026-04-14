/// App constants for PsyNow
class AppConstants {
  AppConstants._();

  // App name
  static const String appName = 'PsyNow';

  // GeForce NOW environment detection
  static const String gfnEnvironmentPath = r'C:\Asgard';

  // Default install directory (will be created on first run)
  static const String defaultInstallDir = r'C:\PsyNow';

  // Config file name
  static const String configFileName = 'PsyNowConfig.ini';

  // Steam server proxy
  static const String steamServerUrl = 'http://127.10.0.231:9753/shutdown';
  static const String steamServerPath =
      r'C:\Program Files (x86)\Steam\lockdown\server\server.exe';
  static const String steamProtocol = 'steam://open/library';

  // Window titles to close
  static const String customExplorerTitle = 'CustomExplorer';
  static const String winXShellTitle = 'WinXShell';

  // Core shortcuts that cannot be removed
  static const List<String> coreShortcuts = [
    'Explorer++.lnk',
  ];

  // Directory names
  static const String shortcutsDir = 'Shortcuts';
  static const String backupShortcutsDir = 'Backup Shortcuts';

  // Base URL for app downloads
  static const String _baseUrl =
      'https://github.com/dpadGuy/SalsaNOWThings/releases/download/Things';

  // Downloadable apps configuration
  static const List<DownloadableApp> bundledApps = [
    // File manager
    DownloadableApp(
      name: '7-Zip',
      downloadUrl: '$_baseUrl/7-Zip.zip',
      fileName: '7-Zip.zip',
      exeName: '7zFM.exe',
      isZip: true,
      createShortcut: true,
    ),
    // Web browser
    DownloadableApp(
      name: 'Brave',
      downloadUrl: '$_baseUrl/Brave.zip',
      fileName: 'Brave.zip',
      exeName: 'brave.exe',
      isZip: true,
      createShortcut: true,
    ),
    // File explorer replacement
    DownloadableApp(
      name: 'Explorer++',
      downloadUrl: '$_baseUrl/Explorer%2B%2B.exe',
      fileName: 'Explorer++.exe',
      exeName: 'Explorer++.exe',
      isZip: false,
      createShortcut: true,
      runAfterInstall: true,
    ),
    // Steam depot downloader
    DownloadableApp(
      name: 'DepotDownloader',
      downloadUrl: '$_baseUrl/DepotDownloader-windows-x64.zip',
      fileName: 'DepotDownloader-windows-x64.zip',
      exeName: 'DepotDownloader.exe',
      isZip: true,
      createShortcut: true,
    ),
    // Text editor
    DownloadableApp(
      name: 'Notepad++',
      downloadUrl: '$_baseUrl/Notepad%2B%2B.zip',
      fileName: 'Notepad++.zip',
      exeName: 'notepad++.exe',
      isZip: true,
      createShortcut: true,
    ),
    // Torrent client
    DownloadableApp(
      name: 'qBittorrent',
      downloadUrl: '$_baseUrl/qBittorrent.zip',
      fileName: 'qBittorrent.zip',
      exeName: 'qbittorrent.exe',
      isZip: true,
      createShortcut: true,
    ),
    // System monitor
    DownloadableApp(
      name: 'System Informer',
      downloadUrl: '$_baseUrl/System.Informer.zip',
      fileName: 'System.Informer.zip',
      exeName: 'SystemInformer.exe',
      isZip: true,
      createShortcut: true,
    ),
    // Epic Games portable launcher
    DownloadableApp(
      name: 'Epic Games Portable',
      downloadUrl: '$_baseUrl/EpicGames.Portable.zip',
      fileName: 'EpicGames.Portable.zip',
      exeName: 'EpicGamesLauncher.exe',
      isZip: true,
      createShortcut: true,
    ),
    // Epic Games installer for GFN
    DownloadableApp(
      name: 'Epic Games Installer',
      downloadUrl: '$_baseUrl/EpicGamesInstallerGFN.exe',
      fileName: 'EpicGamesInstallerGFN.exe',
      exeName: 'EpicGamesInstallerGFN.exe',
      isZip: false,
      createShortcut: true,
    ),
    // Alt-Tab replacement
    DownloadableApp(
      name: 'Ctrl+Tab',
      downloadUrl: '$_baseUrl/ctrl_tab.exe',
      fileName: 'ctrl_tab.exe',
      exeName: 'ctrl_tab.exe',
      isZip: false,
      createShortcut: false,
      runAfterInstall: true,
    ),
    // Steam Web App
    DownloadableApp(
      name: 'Steam Web App',
      downloadUrl: '$_baseUrl/SWA.zip',
      fileName: 'SWA.zip',
      exeName: 'SWA.exe',
      isZip: true,
      createShortcut: true,
    ),
  ];

  // Shell environments (separate from apps)
  static const List<DownloadableApp> shellEnvironments = [
    DownloadableApp(
      name: 'Seelen UI',
      downloadUrl: '$_baseUrl/seelenui.zip',
      fileName: 'seelenui.zip',
      exeName: 'seelen-ui.exe',
      isZip: true,
      createShortcut: false,
      runAfterInstall: true,
    ),
    DownloadableApp(
      name: 'Seelen UI Config',
      downloadUrl: '$_baseUrl/SeelenUiConfig.zip',
      fileName: 'SeelenUiConfig.zip',
      exeName: '',
      isZip: true,
      createShortcut: false,
    ),
    DownloadableApp(
      name: 'WinXShell',
      downloadUrl: '$_baseUrl/WinXShell_x64.zip',
      fileName: 'WinXShell_x64.zip',
      exeName: 'WinXShell.exe',
      isZip: true,
      createShortcut: false,
      runAfterInstall: true,
    ),
    DownloadableApp(
      name: 'WinXShell Steam',
      downloadUrl: '$_baseUrl/WinXShell_Steam.zip',
      fileName: 'WinXShell_Steam.zip',
      exeName: 'WinXShell.exe',
      isZip: true,
      createShortcut: false,
    ),
    DownloadableApp(
      name: 'WinXShell Ubisoft',
      downloadUrl: '$_baseUrl/WinXShell_Ubisoft.zip',
      fileName: 'WinXShell_Ubisoft.zip',
      exeName: 'WinXShell.exe',
      isZip: true,
      createShortcut: false,
    ),
  ];
}

/// Configuration for a downloadable app
class DownloadableApp {
  final String name;
  final String downloadUrl;
  final String fileName;
  final String exeName;
  final bool isZip;
  final bool createShortcut;
  final bool runAfterInstall;

  const DownloadableApp({
    required this.name,
    required this.downloadUrl,
    required this.fileName,
    required this.exeName,
    required this.isZip,
    this.createShortcut = false,
    this.runAfterInstall = false,
  });
}
