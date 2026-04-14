import 'dart:io';
import 'dart:typed_data';

import 'package:fast_log/fast_log.dart';
import 'package:http/http.dart' as http;

/// Service for downloading files with progress tracking
class DownloadService {
  /// Download a file from URL with progress callback
  /// Returns the downloaded bytes or null on failure
  static Future<Uint8List?> downloadFile(
    String url, {
    void Function(double progress)? onProgress,
    void Function(String message)? onLog,
  }) async {
    try {
      onLog?.call('[+] Downloading from: $url');

      final request = http.Request('GET', Uri.parse(url));
      final response = await http.Client().send(request);

      if (response.statusCode != 200) {
        error('[DownloadService] HTTP ${response.statusCode} for $url');
        onLog?.call('[!] Download failed: HTTP ${response.statusCode}');
        return null;
      }

      final contentLength = response.contentLength ?? 0;
      final bytes = <int>[];
      int received = 0;

      await for (final chunk in response.stream) {
        bytes.addAll(chunk);
        received += chunk.length;

        if (contentLength > 0) {
          final progress = received / contentLength;
          onProgress?.call(progress);
        }
      }

      onLog?.call('[+] Downloaded ${_formatBytes(bytes.length)}');
      return Uint8List.fromList(bytes);
    } catch (e) {
      error('[DownloadService] Error downloading $url: $e');
      onLog?.call('[!] Download error: $e');
      return null;
    }
  }

  /// Download a file and save it to disk
  static Future<bool> downloadToFile(
    String url,
    String destPath, {
    void Function(double progress)? onProgress,
    void Function(String message)? onLog,
  }) async {
    final bytes = await downloadFile(
      url,
      onProgress: onProgress,
      onLog: onLog,
    );

    if (bytes == null) return false;

    try {
      final file = File(destPath);
      await file.parent.create(recursive: true);
      await file.writeAsBytes(bytes);
      return true;
    } catch (e) {
      error('[DownloadService] Error saving file: $e');
      onLog?.call('[!] Error saving file: $e');
      return false;
    }
  }

  static String _formatBytes(int bytes) {
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) return '${(bytes / 1024).toStringAsFixed(1)} KB';
    if (bytes < 1024 * 1024 * 1024) {
      return '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';
    }
    return '${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(1)} GB';
  }
}
