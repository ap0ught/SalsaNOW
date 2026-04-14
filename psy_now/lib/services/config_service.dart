import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:psy_now/utils/constants.dart';

/// Reads `SalsaNOWConfig.ini` in the global directory (same file name as SalsaNOW).
class ConfigService {
  ConfigService(this.globalDirectory) {
    _configFile =
        File('$globalDirectory${Platform.pathSeparator}${AppConstants.configFileName}');
  }

  final String globalDirectory;
  late File _configFile;

  bool skipShortcutsCreation = false;
  bool seelenInstallEnabled = false;
  bool bingPhotoOfTheDayWallpaper = false;
  bool nvidiaRaytracing = false;

  String get configPath => _configFile.path;

  Future<void> load() async {
    if (!await _configFile.exists()) {
      info('[Config] Config file not found, using defaults');
      return;
    }

    try {
      final lines = await _configFile.readAsLines();

      for (final line in lines) {
        final trimmed = line.trim();
        if (trimmed.isEmpty ||
            trimmed.startsWith('#') ||
            trimmed.startsWith(';')) {
          continue;
        }

        if (trimmed.contains('SkipShortcutsCreation')) {
          skipShortcutsCreation = trimmed.contains('= "1"');
        }
        // Matches SalsaNOW `SalsaSettings`: true when line contains `SkipSeelenUiExecution = "0"`
        if (trimmed.contains('SkipSeelenUiExecution')) {
          seelenInstallEnabled = trimmed.contains('= "0"');
        }
        if (trimmed.contains('BingPhotoOfTheDayWallpaper')) {
          bingPhotoOfTheDayWallpaper = trimmed.contains('= "1"');
        }
        if (trimmed.contains('NvidiaRaytracing')) {
          nvidiaRaytracing = trimmed.contains('= "1"');
        }
      }

      info(
        '[Config] Loaded shortcuts=$skipShortcutsCreation seelenFlow=$seelenInstallEnabled bing=$bingPhotoOfTheDayWallpaper rtx=$nvidiaRaytracing',
      );
    } catch (e) {
      error('[Config] Error loading config: $e');
    }
  }

  Future<void> save() async {
    try {
      final content = '''
; SalsaNOW / PsyNow configuration (same keys as desktop SalsaNOW)
; 0 = enabled, 1 = disabled/skip (for shortcut flag — PsyNow extension)

SkipShortcutsCreation = "${skipShortcutsCreation ? '1' : '0'}"
SkipSeelenUiExecution = "${seelenInstallEnabled ? '0' : '1'}"
BingPhotoOfTheDayWallpaper = "${bingPhotoOfTheDayWallpaper ? '1' : '0'}"
NvidiaRaytracing = "${nvidiaRaytracing ? '1' : '0'}"
''';

      await _configFile.writeAsString(content);
      info('[Config] Saved config to ${_configFile.path}');
    } catch (e) {
      error('[Config] Error saving config: $e');
    }
  }

  Future<void> setSkipShortcutsCreation(bool value) async {
    skipShortcutsCreation = value;
    await save();
  }

  Future<void> setSeelenInstallEnabled(bool value) async {
    seelenInstallEnabled = value;
    await save();
  }

  Future<void> setBingPhotoOfTheDayWallpaper(bool value) async {
    bingPhotoOfTheDayWallpaper = value;
    await save();
  }

  Future<void> setNvidiaRaytracing(bool value) async {
    nvidiaRaytracing = value;
    await save();
  }
}
