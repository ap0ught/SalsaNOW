import 'dart:convert';
import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:http/http.dart' as http;
import 'package:psy_now/models/manifest_models.dart';
import 'package:psy_now/services/remote_manifest.dart';

/// Mirrors `SalsaNOW/Program.Startup` manifest-driven bootstrap.
class BootstrapService {
  BootstrapService._();

  static Future<String> resolveGlobalDirectory() async {
    final uri = Uri.parse(RemoteManifest.url('jsons/directory.json'));
    final res = await http.get(uri);
    if (res.statusCode != 200) {
      throw Exception('directory.json HTTP ${res.statusCode}');
    }
    final list = jsonDecode(res.body) as List<dynamic>;
    if (list.isEmpty) {
      throw Exception('directory.json is empty');
    }
    final first = list.first as Map<String, dynamic>;
    final dir = SavePathEntry.fromJson(first).directoryCreate;
    if (dir.isEmpty) {
      throw Exception('directoryCreate missing');
    }
    await Directory(dir).create(recursive: true);
    info('[Bootstrap] Global directory: $dir');
    return dir;
  }

  static Future<void> ensureConfigIni(String globalDirectory) async {
    final cfg =
        '$globalDirectory${Platform.pathSeparator}SalsaNOWConfig.ini';
    if (await File(cfg).exists()) return;

    final uri = Uri.parse(RemoteManifest.url('jsons/SalsaNOWConfig.ini'));
    final res = await http.get(uri);
    if (res.statusCode != 200) {
      warn('[Bootstrap] Could not download SalsaNOWConfig.ini (${res.statusCode})');
      return;
    }
    await File(cfg).writeAsBytes(res.bodyBytes);
    info('[Bootstrap] Wrote default SalsaNOWConfig.ini');
  }

  static Future<List<ManifestAppEntry>> loadAppsCatalog() async {
    final uri = Uri.parse(RemoteManifest.url('jsons/apps.json'));
    final res = await http.get(uri);
    if (res.statusCode != 200) {
      throw Exception('apps.json HTTP ${res.statusCode}');
    }
    final list = jsonDecode(res.body) as List<dynamic>;
    return list
        .map((e) => ManifestAppEntry.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  static Future<List<ManifestSilentAppEntry>> loadSilentAppsCatalog() async {
    final uri = Uri.parse(RemoteManifest.url('jsons/silentapps.json'));
    final res = await http.get(uri);
    if (res.statusCode != 200) {
      throw Exception('silentapps.json HTTP ${res.statusCode}');
    }
    final list = jsonDecode(res.body) as List<dynamic>;
    return list
        .map(
          (e) =>
              ManifestSilentAppEntry.fromJson(e as Map<String, dynamic>),
        )
        .toList();
  }

  static Future<List<ManifestDesktopEntry>> loadDesktopCatalog() async {
    final uri = Uri.parse(RemoteManifest.url('jsons/desktop.json'));
    final res = await http.get(uri);
    if (res.statusCode != 200) {
      throw Exception('desktop.json HTTP ${res.statusCode}');
    }
    final list = jsonDecode(res.body) as List<dynamic>;
    return list
        .map(
          (e) => ManifestDesktopEntry.fromJson(e as Map<String, dynamic>),
        )
        .toList();
  }
}
