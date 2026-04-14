import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:http/http.dart' as http;
import 'package:psy_now/utils/constants.dart';

/// Service for managing Steam server proxy on GeForce NOW
class SteamService {
  final String globalDirectory;
  final void Function(String message)? onLog;

  SteamService({
    required this.globalDirectory,
    this.onLog,
  });

  /// Shutdown NVIDIA's Steam Server proxy and restart Steam
  /// This allows all opted-in games to show up in the Steam library
  Future<bool> shutdownSteamServer() async {
    try {
      // Send POST request to shutdown Steam Server
      onLog?.call('[+] Shutting down Steam Server...');

      try {
        final response = await http.post(Uri.parse(AppConstants.steamServerUrl));
        onLog?.call('[+] Steam Server response: ${response.body}');
      } catch (e) {
        onLog?.call('[!] Steam Server is not running or not accessible');
      }

      // Kill Steam processes
      await _killSteamProcesses();

      // Reopen Steam library
      await Process.start(
        'cmd',
        ['/c', 'start', '', AppConstants.steamProtocol],
        mode: ProcessStartMode.detached,
        runInShell: true,
      );

      onLog?.call('[+] Steam library reopened');
      return true;
    } catch (e) {
      error('[SteamService] Error: $e');
      onLog?.call('[!] Steam Server error: $e');
      return false;
    }
  }

  /// Kill all Steam processes
  Future<void> _killSteamProcesses() async {
    try {
      if (Platform.isWindows) {
        await Process.run('taskkill', ['/F', '/IM', 'steam.exe']);
        onLog?.call('[+] Killed Steam processes');
      }
    } catch (e) {
      warn('[SteamService] Error killing Steam: $e');
    }
  }
}
