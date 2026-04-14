import 'dart:io';

import 'package:archive/archive.dart';
import 'package:fast_log/fast_log.dart';
import 'package:psy_now/models/manifest_models.dart';
import 'package:psy_now/services/config_service.dart';
import 'package:psy_now/services/download_service.dart';
import 'package:psy_now/services/environment_service.dart';
import 'package:psy_now/services/shortcut_service.dart';

/// Mirrors `SalsaNOW/AppInstaller.AppsInstallAsync` + `AppsInstallSilentAsync`.
class AppInstaller {
  AppInstaller({
    required this.globalDirectory,
    required this.config,
    this.onLog,
    this.onProgress,
  });

  final String globalDirectory;
  final ConfigService config;
  final void Function(String message)? onLog;
  final void Function(String appName, double progress)? onProgress;

  Future<void> installAppsFromManifest(List<ManifestAppEntry> apps) async {
    for (final app in apps) {
      await _installMainApp(app);
    }

    if (!config.skipShortcutsCreation) {
      await config.setSkipShortcutsCreation(true);
    }
  }

  Future<void> installSilentApps(List<ManifestSilentAppEntry> apps) async {
    final silentRoot =
        '$globalDirectory${Platform.pathSeparator}SilentApps';
    await Directory(silentRoot).create(recursive: true);

    for (final app in apps) {
      try {
        final appFolder =
            '$silentRoot${Platform.pathSeparator}${app.name}';
        final appPath =
            '$silentRoot${Platform.pathSeparator}${app.fileName}.${app.fileExtension}';
        final appZipPath =
            '$appFolder${Platform.pathSeparator}${app.fileName}.${app.fileExtension}';

        if (app.archive == 'true') {
          if (await File(appZipPath).exists()) continue;
          final zip = '$appFolder.zip';
          await Directory(appFolder).create(recursive: true);
          final ok = await DownloadService.downloadToFile(
            app.url,
            zip,
            onLog: onLog,
          );
          if (!ok) continue;
          await _extractZipFileToDir(zip, appFolder);
          try {
            await File(zip).delete();
          } catch (_) {}
          if (app.run == 'true') {
            await Process.start(appZipPath, [],
                mode: ProcessStartMode.detached);
          }
        } else {
          if (!await File(appPath).exists()) {
            await DownloadService.downloadToFile(
              app.url,
              appPath,
              onLog: onLog,
            );
          }
          if (app.run == 'true') {
            await Process.start(
              appPath,
              [],
              mode: ProcessStartMode.detached,
            );
          }
        }
      } catch (e) {
        error('[AppInstaller] silent ${app.name}: $e');
        onLog?.call('[!] Silent ${app.name}: $e');
      }
    }
  }

  Future<void> _installMainApp(ManifestAppEntry app) async {
    final desktopPath = EnvironmentService.getDesktopPath();
    final shortcutPath =
        '$desktopPath${Platform.pathSeparator}${app.name}.lnk';

    final appDir =
        '$globalDirectory${Platform.pathSeparator}${app.name}';
    final appExePath =
        '$globalDirectory${Platform.pathSeparator}${app.exeName}';
    final appZipExe =
        '$appDir${Platform.pathSeparator}${app.exeName}';

    final isZip = app.fileExtension == 'zip';
    final isExe = app.fileExtension == 'exe';

    final alreadyExists = (isZip && await Directory(appDir).exists()) ||
        (isExe && await File(appExePath).exists());

    try {
      if (!alreadyExists) {
        onLog?.call('[+] Installing ${app.name}...');
        onProgress?.call(app.name, 0);

        if (isZip) {
          final zipPath = '$appDir.zip';
          await Directory(appDir).parent.create(recursive: true);
          final ok = await DownloadService.downloadToFile(
            app.url,
            zipPath,
            onProgress: (p) => onProgress?.call(app.name, p * 0.5),
            onLog: onLog,
          );
          if (!ok) return;
          await _extractZipFileToDir(zipPath, appDir);
          try {
            await File(zipPath).delete();
          } catch (_) {}

          await _maybeShortcut(shortcutPath, appZipExe);
          if (app.run == 'true') {
            await Process.start(appZipExe, [],
                mode: ProcessStartMode.detached);
          }
        } else if (isExe) {
          final ok = await DownloadService.downloadToFile(
            app.url,
            appExePath,
            onProgress: (p) => onProgress?.call(app.name, p),
            onLog: onLog,
          );
          if (!ok) return;
          await _maybeShortcut(shortcutPath, appExePath);
          if (app.run == 'true') {
            await Process.start(appExePath, [],
                mode: ProcessStartMode.detached);
          }
        }

        onProgress?.call(app.name, 1);
        onLog?.call('[+] Installed ${app.name}');
      } else {
        onLog?.call('[!] ${app.name} already installed');
        if (isZip) {
          await _maybeShortcut(shortcutPath, appZipExe);
          if (app.run == 'true') {
            await Process.start(appZipExe, [],
                mode: ProcessStartMode.detached);
          }
        } else if (isExe) {
          await _maybeShortcut(shortcutPath, appExePath);
          if (app.run == 'true') {
            await Process.start(appExePath, [],
                mode: ProcessStartMode.detached);
          }
        }
      }
    } catch (e) {
      error('[AppInstaller] ${app.name}: $e');
      onLog?.call('[!] ${app.name}: $e');
    }
  }

  Future<void> _maybeShortcut(String shortcutPath, String targetPath) async {
    if (!config.skipShortcutsCreation) {
      await ShortcutService.createShortcut(shortcutPath, targetPath);
    }
  }

  Future<void> _extractZipFileToDir(String zipPath, String destDir) async {
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
