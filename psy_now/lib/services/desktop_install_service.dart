import 'dart:io';

import 'package:archive/archive.dart';
import 'package:fast_log/fast_log.dart';
import 'package:psy_now/models/manifest_models.dart';
import 'package:psy_now/services/config_service.dart';
import 'package:psy_now/services/download_service.dart';
import 'package:psy_now/services/window_service.dart';

/// Subset of `SalsaNOW/AppInstaller.DesktopInstallAsync`: dark mode + desktop.json zips.
/// Full Seelen config / Bing parity can be extended later.
class DesktopInstallService {
  DesktopInstallService({
    required this.globalDirectory,
    required this.config,
    this.onLog,
  });

  final String globalDirectory;
  final ConfigService config;
  final void Function(String message)? onLog;

  Future<void> installFromCatalog(List<ManifestDesktopEntry> entries) async {
    if (!Platform.isWindows) return;

    await Process.run(
      'cmd.exe',
      [
        '/c',
        'reg',
        'add',
        r'HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize',
        '/v',
        'AppsUseLightTheme',
        '/t',
        'REG_DWORD',
        '/d',
        '0',
        '/f',
      ],
      runInShell: false,
    );
    onLog?.call('[+] Dark mode preference applied');

    WindowService.closeWindowByTitle('CustomExplorer');

    final skipSeelen = config.seelenInstallEnabled;

    for (final desktop in entries) {
      try {
        final appDir =
            '$globalDirectory${Platform.pathSeparator}${desktop.name}';
        final zipFile =
            '$globalDirectory${Platform.pathSeparator}${desktop.name}.zip';
        final exePath =
            '$appDir${Platform.pathSeparator}${desktop.exeName}';

        if (!await Directory(appDir).exists()) {
          final ok = await DownloadService.downloadToFile(
            desktop.url,
            zipFile,
            onLog: onLog,
          );
          if (!ok) continue;

          await _extractZipToDir(zipFile, appDir);
          try {
            await File(zipFile).delete();
          } catch (_) {}

          if (desktop.name.toLowerCase().contains('winxshell')) {
            await Process.start(exePath, [], mode: ProcessStartMode.detached);
            await Future<void>.delayed(const Duration(milliseconds: 500));
            WindowService.closeWindowByTitle('WinXShell');
          }

          if (desktop.name.toLowerCase().contains('seelenui') &&
              skipSeelen) {
            await Process.start(exePath, [], mode: ProcessStartMode.detached);
          }
        } else {
          if (desktop.name.toLowerCase().contains('winxshell')) {
            if (!await File(exePath).exists()) {
              final wrong = '$appDir${Platform.pathSeparator}explorer.exe';
              final f = File(wrong);
              if (await f.exists()) {
                await f.rename(exePath);
              }
              await Future<void>.delayed(const Duration(seconds: 1));
            }
            await Process.start(exePath, [], mode: ProcessStartMode.detached);
            WindowService.closeWindowByTitle('WinXShell');
          }
          if (desktop.name.toLowerCase().contains('seelenui') &&
              skipSeelen) {
            await Process.start(exePath, [], mode: ProcessStartMode.detached);
          }
        }
      } catch (e) {
        error('[DesktopInstall] ${desktop.name}: $e');
        onLog?.call('[!] Desktop ${desktop.name}: $e');
      }
    }
  }

  Future<void> _extractZipToDir(String zipPath, String destDir) async {
    final bytes = await File(zipPath).readAsBytes();
    final archive = ZipDecoder().decodeBytes(bytes);
    await Directory(destDir).create(recursive: true);
    for (final file in archive) {
      final outPath =
          '$destDir${Platform.pathSeparator}${file.name}';
      if (file.isFile) {
        final outFile = File(outPath);
        await outFile.parent.create(recursive: true);
        await outFile.writeAsBytes(file.content as List<int>);
      } else {
        await Directory(outPath).create(recursive: true);
      }
    }
  }
}
