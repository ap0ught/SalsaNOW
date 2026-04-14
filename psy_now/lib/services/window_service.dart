import 'dart:async';
import 'dart:io';

import 'package:fast_log/fast_log.dart';
import 'package:psy_now/utils/constants.dart';
import 'package:psy_now/utils/win32_utils.dart';

/// Service for managing windows on GeForce NOW
class WindowService {
  static Timer? _customExplorerTimer;

  /// Close a window by its exact title
  static bool closeWindowByTitle(String title) {
    if (!Platform.isWindows) return false;

    final hwnd = Win32Utils.findWindowByTitle(title);
    if (hwnd != null) {
      Win32Utils.closeWindowSync(hwnd);
      info('[WindowService] Closed window: $title');
      return true;
    }
    return false;
  }

  /// Close windows matching partial title (simplified - tries exact match)
  static int closeWindowsByPartialTitle(String partialTitle) {
    if (!Platform.isWindows) return 0;

    final hwnd = Win32Utils.findWindowByTitle(partialTitle);
    if (hwnd != null) {
      Win32Utils.closeWindowSync(hwnd);
      info('[WindowService] Closed window: $partialTitle');
      return 1;
    }

    return 0;
  }

  /// Start background task to continuously close CustomExplorer
  static void startCustomExplorerKiller() {
    if (!Platform.isWindows) return;

    _customExplorerTimer?.cancel();
    _customExplorerTimer = Timer.periodic(const Duration(milliseconds: 500), (_) {
      closeWindowByTitle(AppConstants.customExplorerTitle);
    });

    info('[WindowService] Started CustomExplorer killer');
  }

  /// Stop the CustomExplorer killer
  static void stopCustomExplorerKiller() {
    _customExplorerTimer?.cancel();
    _customExplorerTimer = null;
    info('[WindowService] Stopped CustomExplorer killer');
  }

  /// Wait for a window to appear and close it
  static Future<bool> waitAndCloseWindow(
    String title, {
    int timeoutMs = 5000,
    bool partial = false,
  }) async {
    if (!Platform.isWindows) return false;

    final stopwatch = Stopwatch()..start();

    while (stopwatch.elapsedMilliseconds < timeoutMs) {
      bool closed;
      if (partial) {
        closed = closeWindowsByPartialTitle(title) > 0;
      } else {
        closed = closeWindowByTitle(title);
      }

      if (closed) return true;

      await Future.delayed(const Duration(milliseconds: 500));
    }

    return false;
  }
}
