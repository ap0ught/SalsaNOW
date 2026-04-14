import 'dart:io';

import 'package:fast_log/fast_log.dart';

/// Same contract as SalsaNOW `RemoteManifest`: [envManifestBase] or
/// `SalsaNOW.manifest.ini` next to the executable with `ManifestBaseUrl=...`.
class RemoteManifest {
  RemoteManifest._();

  static const envManifestBase = 'SALSANOW_MANIFEST_BASE';
  static const iniFileName = 'SalsaNOW.manifest.ini';

  static String? _baseUrl;

  static bool get isInitialized =>
      _baseUrl != null && _baseUrl!.trim().isNotEmpty;

  /// Returns false if neither env nor ini provides a base (caller may use a local fallback path only).
  static Future<bool> initialize() async {
    _baseUrl = Platform.environment[envManifestBase]?.trim();
    _baseUrl = _trimTrailingSlash(_baseUrl);

    if (_baseUrl == null || _baseUrl!.isEmpty) {
      _baseUrl = await _readManifestBaseFromIni();
    }

    if (_baseUrl == null || _baseUrl!.isEmpty) {
      warn(
        '[RemoteManifest] No manifest host. Set $envManifestBase or place $iniFileName beside the executable with ManifestBaseUrl=https://...',
      );
      return false;
    }

    info('[RemoteManifest] Base URL: $_baseUrl');
    return true;
  }

  /// Throws if not initialized.
  static String url(String relativePath) {
    if (!isInitialized) {
      throw StateError(
        'RemoteManifest is not configured. Set $envManifestBase or $iniFileName.',
      );
    }
    final rel = relativePath.replaceFirst(RegExp(r'^/+'), '');
    return '$_baseUrl/$rel';
  }

  static String? _trimTrailingSlash(String? s) {
    if (s == null) return null;
    var t = s.trim();
    while (t.endsWith('/')) {
      t = t.substring(0, t.length - 1);
    }
    return t.isEmpty ? null : t;
  }

  static Future<String?> _readManifestBaseFromIni() async {
    try {
      final exeDir = File(Platform.resolvedExecutable).parent.path;
      final path = '$exeDir${Platform.pathSeparator}$iniFileName';
      final f = File(path);
      if (!await f.exists()) return null;

      for (final raw in await f.readAsLines()) {
        var line = raw.trim();
        if (line.isEmpty || line.startsWith('#') || line.startsWith(';')) {
          continue;
        }
        final eq = line.indexOf('=');
        if (eq <= 0) continue;
        final key = line.substring(0, eq).trim();
        if (!key.toLowerCase().contains('manifestbaseurl')) continue;
        return _trimTrailingSlash(line.substring(eq + 1).trim());
      }
    } catch (e) {
      warn('[RemoteManifest] Error reading ini: $e');
    }
    return null;
  }

  /// For tests.
  static void resetForTests() => _baseUrl = null;
}
