import 'dart:convert';
import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:http/http.dart' as http;
import 'package:psy_now/models/manifest_models.dart';
import 'package:psy_now/services/remote_manifest.dart';

/// Port of `SalsaNOW/SteamManager.SetupGameSavesAsync` (junction creation).
class GameSavesService {
  GameSavesService({required this.globalDirectory, this.onLog});

  final String globalDirectory;
  final void Function(String message)? onLog;

  Future<void> setupJunctions() async {
    if (!RemoteManifest.isInitialized) {
      onLog?.call('[!] Game saves: manifest not configured, skipping');
      return;
    }

    try {
      onLog?.call('[+] Setting up cloud save junctions...');
      final uri = Uri.parse(RemoteManifest.url('jsons/GameSavesPaths.json'));
      final res = await http.get(uri);
      if (res.statusCode != 200) {
        onLog?.call('[!] GameSavesPaths.json HTTP ${res.statusCode}');
        return;
      }

      final model = GamesSavePaths.fromJson(
        jsonDecode(res.body) as Map<String, dynamic>,
      );

      final savesRoot =
          '$globalDirectory${Platform.pathSeparator}Game Saves';
      await Directory(savesRoot).create(recursive: true);

      for (final dir in model.paths) {
        final crafted =
            '$savesRoot${Platform.pathSeparator}${_fileName(dir)}';
        await Directory(crafted).create(recursive: true);

        await Process.run('cmd.exe', ['/c', 'rmdir', '/s', '/q', dir],
            runInShell: false);
        await Future<void>.delayed(const Duration(milliseconds: 500));
        await Process.run(
          'cmd.exe',
          ['/c', 'mklink', '/J', dir, crafted],
          runInShell: false,
        );

        if (dir.contains(r'C:\Users\Public\Documents')) {
          await _handlePublicDocs(dir, crafted);
        }
      }

      onLog?.call('[+] Cloud save junctions created');
    } catch (e) {
      error('[GameSavesService] $e');
      onLog?.call('[!] Game saves setup error: $e');
    }
  }

  String _fileName(String p) {
    final s = p.replaceAll('/', Platform.pathSeparator);
    return s.split(Platform.pathSeparator).lastWhere((x) => x.isNotEmpty);
  }

  Future<void> _handlePublicDocs(String dir, String crafted) async {
    for (var i = 0; i < 20; i++) {
      try {
        if (await Directory(dir).exists()) {
          await Directory(dir).delete(recursive: true);
        }
        if (!await Directory(dir).exists()) {
          await Process.run(
            'cmd.exe',
            ['/c', 'mklink', '/J', dir, crafted],
            runInShell: false,
          );
          break;
        }
      } catch (_) {}
      await Future<void>.delayed(const Duration(milliseconds: 200));
    }
  }
}
