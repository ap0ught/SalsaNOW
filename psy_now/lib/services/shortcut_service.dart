import 'dart:async';
import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:psy_now/services/environment_service.dart';
import 'package:psy_now/utils/constants.dart';

/// Service for syncing shortcuts between desktop and persistent storage
class ShortcutService {
  final String globalDirectory;
  final void Function(String message)? onLog;

  Timer? _syncTimer;

  String get shortcutsDir =>
      '$globalDirectory${Platform.pathSeparator}${AppConstants.shortcutsDir}';
  String get backupShortcutsDir =>
      '$globalDirectory${Platform.pathSeparator}${AppConstants.backupShortcutsDir}';

  ShortcutService({
    required this.globalDirectory,
    this.onLog,
  });

  /// Initialize shortcuts directories
  Future<void> initialize() async {
    await Directory(shortcutsDir).create(recursive: true);
    await Directory(backupShortcutsDir).create(recursive: true);
  }

  /// Copy saved shortcuts to desktop on startup
  Future<void> restoreShortcuts() async {
    final desktopPath = EnvironmentService.getDesktopPath();

    try {
      // Delete desktop.ini if exists
      final desktopIni = File('$desktopPath${Platform.pathSeparator}desktop.ini');
      if (await desktopIni.exists()) {
        await desktopIni.delete();
      }

      // Copy all .lnk files from shortcuts dir to desktop
      final shortcuts = Directory(shortcutsDir)
          .listSync(recursive: true)
          .whereType<File>()
          .where((f) => f.path.toLowerCase().endsWith('.lnk'));

      for (final shortcut in shortcuts) {
        final relativePath = shortcut.path.substring(shortcutsDir.length + 1);
        final destPath = '$desktopPath${Platform.pathSeparator}$relativePath';

        try {
          await shortcut.copy(destPath);
          info('[ShortcutService] Restored: $relativePath');
        } catch (e) {
          // File might already exist
        }
      }
    } catch (e) {
      error('[ShortcutService] Error restoring shortcuts: $e');
    }
  }

  /// Start the background sync loop
  void startSyncLoop() {
    _syncTimer?.cancel();
    _syncTimer = Timer.periodic(const Duration(seconds: 5), (_) {
      _syncShortcuts();
    });
    info('[ShortcutService] Started sync loop');
  }

  /// Stop the background sync loop
  void stopSyncLoop() {
    _syncTimer?.cancel();
    _syncTimer = null;
    info('[ShortcutService] Stopped sync loop');
  }

  Future<void> _syncShortcuts() async {
    final desktopPath = EnvironmentService.getDesktopPath();

    try {
      // Ensure core shortcuts exist
      await _ensureCoreShortcuts(desktopPath);

      // Sync from desktop to shortcuts directory
      await _syncDesktopToStorage(desktopPath);

      // Backup removed shortcuts
      await _backupRemovedShortcuts(desktopPath);
    } catch (e) {
      error('[ShortcutService] Sync error: $e');
    }
  }

  Future<void> _ensureCoreShortcuts(String desktopPath) async {
    for (final shortcutName in AppConstants.coreShortcuts) {
      final desktopShortcut =
          File('$desktopPath${Platform.pathSeparator}$shortcutName');

      if (!await desktopShortcut.exists()) {
        // Try to restore from shortcuts dir
        final savedShortcut =
            File('$shortcutsDir${Platform.pathSeparator}$shortcutName');

        if (await savedShortcut.exists()) {
          await savedShortcut.copy(desktopShortcut.path);
          onLog?.call('[!] Restored core shortcut: $shortcutName');
        } else {
          // Try backup
          final backupShortcut =
              File('$backupShortcutsDir${Platform.pathSeparator}$shortcutName');

          if (await backupShortcut.exists()) {
            await backupShortcut.copy(desktopShortcut.path);
            onLog?.call('[!] Restored core shortcut from backup: $shortcutName');
          }
        }
      }
    }
  }

  Future<void> _syncDesktopToStorage(String desktopPath) async {
    final desktopDir = Directory(desktopPath);
    final lnkFiles = desktopDir
        .listSync()
        .whereType<File>()
        .where((f) => f.path.toLowerCase().endsWith('.lnk'));

    for (final file in lnkFiles) {
      final fileName = file.path.split(Platform.pathSeparator).last;
      final destPath = '$shortcutsDir${Platform.pathSeparator}$fileName';

      try {
        final destFile = File(destPath);
        if (!await destFile.exists()) {
          await file.copy(destPath);
          info('[ShortcutService] Synced: $fileName');
        }
      } catch (e) {
        // Skip if copy fails
      }
    }
  }

  Future<void> _backupRemovedShortcuts(String desktopPath) async {
    final savedShortcuts = Directory(shortcutsDir)
        .listSync()
        .whereType<File>()
        .where((f) => f.path.toLowerCase().endsWith('.lnk'));

    for (final savedFile in savedShortcuts) {
      final fileName = savedFile.path.split(Platform.pathSeparator).last;
      final desktopFile =
          File('$desktopPath${Platform.pathSeparator}$fileName');

      if (!await desktopFile.exists()) {
        // Shortcut was removed from desktop, back it up
        final backupPath =
            '$backupShortcutsDir${Platform.pathSeparator}$fileName';
        final backupFile = File(backupPath);

        if (await backupFile.exists()) {
          // Already backed up, just delete from shortcuts
          await savedFile.delete();
        } else {
          // Move to backup
          await savedFile.rename(backupPath);
          info('[ShortcutService] Backed up removed shortcut: $fileName');
        }
      }
    }
  }

  /// Create a Windows shortcut (.lnk file)
  /// Note: This uses PowerShell on Windows
  static Future<bool> createShortcut(String shortcutPath, String targetPath) async {
    if (!Platform.isWindows) return false;

    try {
      final workingDir = File(targetPath).parent.path;

      final script = '''
\$WshShell = New-Object -ComObject WScript.Shell
\$Shortcut = \$WshShell.CreateShortcut("$shortcutPath")
\$Shortcut.TargetPath = "$targetPath"
\$Shortcut.WorkingDirectory = "$workingDir"
\$Shortcut.Save()
''';

      final result = await Process.run(
        'powershell',
        ['-Command', script],
        runInShell: true,
      );

      if (result.exitCode == 0) {
        info('[ShortcutService] Created shortcut: $shortcutPath');
        return true;
      } else {
        error('[ShortcutService] Failed to create shortcut: ${result.stderr}');
        return false;
      }
    } catch (e) {
      error('[ShortcutService] Error creating shortcut: $e');
      return false;
    }
  }
}
