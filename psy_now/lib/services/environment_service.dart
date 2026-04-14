import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:psy_now/utils/constants.dart';

/// Service for detecting GeForce NOW environment
class EnvironmentService {
  /// Check if running in GeForce NOW environment
  static Future<bool> isGeForceNowEnvironment() async {
    try {
      final asgardDir = Directory(AppConstants.gfnEnvironmentPath);
      final exists = await asgardDir.exists();

      if (exists) {
        info('[Environment] GeForce NOW environment detected (${AppConstants.gfnEnvironmentPath} exists)');
      } else {
        warn('[Environment] Not a GeForce NOW environment (${AppConstants.gfnEnvironmentPath} not found)');
      }

      return exists;
    } catch (e) {
      error('[Environment] Error checking environment: $e');
      return false;
    }
  }

  /// Get desktop path
  static String getDesktopPath() {
    if (Platform.isWindows) {
      final userProfile = Platform.environment['USERPROFILE'] ?? '';
      return '$userProfile${Platform.pathSeparator}Desktop';
    }
    // Fallback for other platforms (development)
    return Platform.environment['HOME'] ?? '';
  }

  /// Get AppData Roaming path
  static String getAppDataRoamingPath() {
    if (Platform.isWindows) {
      return Platform.environment['APPDATA'] ?? '';
    }
    return Platform.environment['HOME'] ?? '';
  }

  /// Get Local AppData path
  static String getLocalAppDataPath() {
    if (Platform.isWindows) {
      return Platform.environment['LOCALAPPDATA'] ?? '';
    }
    return Platform.environment['HOME'] ?? '';
  }
}
