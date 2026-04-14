import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:psy_now/utils/constants.dart';

/// Service for reading and writing SalsaNOWConfig.ini
class ConfigService {
  final String globalDirectory;
  late File _configFile;

  // Config values
  bool skipShortcutsCreation = false;
  bool skipSeelenUiExecution = false;
  bool bingPhotoOfTheDayWallpaper = false;

  ConfigService(this.globalDirectory) {
    _configFile =
        File('$globalDirectory${Platform.pathSeparator}${AppConstants.configFileName}');
  }

  String get configPath => _configFile.path;

  /// Load configuration from INI file
  Future<void> load() async {
    if (!await _configFile.exists()) {
      info('[Config] Config file not found, using defaults');
      return;
    }

    try {
      final lines = await _configFile.readAsLines();

      for (final line in lines) {
        final trimmed = line.trim();
        if (trimmed.isEmpty || trimmed.startsWith('#') || trimmed.startsWith(';')) {
          continue;
        }

        if (trimmed.contains('SkipShortcutsCreation')) {
          skipShortcutsCreation = trimmed.contains('= "1"');
        } else if (trimmed.contains('SkipSeelenUiExecution')) {
          skipSeelenUiExecution = trimmed.contains('= "1"');
        } else if (trimmed.contains('BingPhotoOfTheDayWallpaper')) {
          bingPhotoOfTheDayWallpaper = trimmed.contains('= "1"');
        }
      }

      info('[Config] Loaded: shortcuts=$skipShortcutsCreation, seelen=$skipSeelenUiExecution, bing=$bingPhotoOfTheDayWallpaper');
    } catch (e) {
      error('[Config] Error loading config: $e');
    }
  }

  /// Save configuration to INI file
  Future<void> save() async {
    try {
      final content = '''
; SalsaNOW/PsyNow Configuration
; 0 = enabled, 1 = disabled/skip

SkipShortcutsCreation = "${skipShortcutsCreation ? '1' : '0'}"
SkipSeelenUiExecution = "${skipSeelenUiExecution ? '1' : '0'}"
BingPhotoOfTheDayWallpaper = "${bingPhotoOfTheDayWallpaper ? '1' : '0'}"
''';

      await _configFile.writeAsString(content);
      info('[Config] Saved config to ${_configFile.path}');
    } catch (e) {
      error('[Config] Error saving config: $e');
    }
  }

  /// Set a config value and save
  Future<void> setSkipShortcutsCreation(bool value) async {
    skipShortcutsCreation = value;
    await save();
  }

  Future<void> setSkipSeelenUiExecution(bool value) async {
    skipSeelenUiExecution = value;
    await save();
  }

  Future<void> setBingPhotoOfTheDayWallpaper(bool value) async {
    bingPhotoOfTheDayWallpaper = value;
    await save();
  }
}
