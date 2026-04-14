import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:http/http.dart' as http;
import 'package:psy_now/services/remote_manifest.dart';

/// Port of `SalsaNOW/SteamManager.ShutdownServerAsync`.
class SteamService {
  SteamService({
    required this.globalDirectory,
    this.onLog,
  });

  final String globalDirectory;
  final void Function(String message)? onLog;

  static const _defaultKakaLockdownJson =
      '{"server_port":9753,"server_address":"127.10.0.231","flavor":"","BlockedFunctions":{}}';

  /// Steam proxy shutdown, lockdown JSON swap, appcache clear, USG helper download.
  Future<bool> shutdownSteamServer() async {
    try {
      onLog?.call('[+] Initiating Steam proxy shutdown sequence...');

      try {
        await http.post(Uri.parse('http://127.10.0.231:9753/shutdown'));
      } catch (_) {}

      final dummyJson =
          '$globalDirectory${Platform.pathSeparator}kaka.json';
      final usgMask =
          '$globalDirectory${Platform.pathSeparator}conhost.exe';

      await File(dummyJson).writeAsString(_defaultKakaLockdownJson);

      await Process.start(
        r'C:\Program Files (x86)\Steam\lockdown\server\server.exe',
        [dummyJson],
        mode: ProcessStartMode.detached,
      );

      final cache = r'C:\Program Files (x86)\Steam\appcache';
      final cacheDir = Directory(cache);
      if (await cacheDir.exists()) {
        await cacheDir.delete(recursive: true);
      }

      if (!RemoteManifest.isInitialized) {
        onLog?.call(
          '[!] USG step skipped: manifest not configured (need USG/bleh.exe URL)',
        );
        onLog?.call('[+] Steam proxy sequence complete (partial)');
        return true;
      }

      final usgUri = Uri.parse(RemoteManifest.url('USG/bleh.exe'));
      final res = await http.get(usgUri);
      if (res.statusCode != 200) {
        onLog?.call('[!] USG download HTTP ${res.statusCode}');
        return false;
      }
      await File(usgMask).writeAsBytes(res.bodyBytes);

      final usg = await Process.start(
        usgMask,
        [],
        mode: ProcessStartMode.normal,
      );
      await usg.exitCode;
      await Future<void>.delayed(const Duration(milliseconds: 200));
      try {
        if (await File(usgMask).exists()) await File(usgMask).delete();
      } catch (_) {}

      onLog?.call('[+] Steam proxy successfully bypassed.');
      return true;
    } catch (e) {
      error('[SteamService] $e');
      onLog?.call('[!] Steam proxy error: $e');
      return false;
    }
  }
}
