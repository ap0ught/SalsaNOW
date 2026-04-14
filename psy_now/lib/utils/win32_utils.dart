import 'dart:ffi';
import 'dart:io';

import 'package:ffi/ffi.dart';
import 'package:win32/win32.dart';

/// Win32 utilities for window manipulation
class Win32Utils {
  static const int wmClose = 0x0010;
  static const int swHide = 0;
  static const int swShow = 5;

  /// Find a window by its title
  static int? findWindowByTitle(String title) {
    if (!Platform.isWindows) return null;

    final titlePtr = title.toNativeUtf16();
    try {
      final hwnd = FindWindow(nullptr, titlePtr);
      return hwnd != 0 ? hwnd : null;
    } finally {
      calloc.free(titlePtr);
    }
  }

  /// Find a window by class name
  static int? findWindowByClass(String className) {
    if (!Platform.isWindows) return null;

    final classPtr = className.toNativeUtf16();
    try {
      final hwnd = FindWindow(classPtr, nullptr);
      return hwnd != 0 ? hwnd : null;
    } finally {
      calloc.free(classPtr);
    }
  }

  /// Send WM_CLOSE message to a window
  static bool closeWindow(int hwnd) {
    if (!Platform.isWindows) return false;

    return PostMessage(hwnd, wmClose, 0, 0) != 0;
  }

  /// Send WM_CLOSE via SendMessage (synchronous)
  static bool closeWindowSync(int hwnd) {
    if (!Platform.isWindows) return false;

    SendMessage(hwnd, wmClose, 0, 0);
    return true;
  }

  /// Hide a window
  static bool hideWindow(int hwnd) {
    if (!Platform.isWindows) return false;

    return ShowWindow(hwnd, swHide) != 0;
  }

  /// Show a window
  static bool showWindow(int hwnd) {
    if (!Platform.isWindows) return false;

    return ShowWindow(hwnd, swShow) != 0;
  }

  /// Get window text (title)
  static String? getWindowText(int hwnd) {
    if (!Platform.isWindows) return null;

    final length = GetWindowTextLength(hwnd);
    if (length == 0) return null;

    final buffer = wsalloc(length + 1);
    try {
      GetWindowText(hwnd, buffer, length + 1);
      return buffer.toDartString();
    } finally {
      calloc.free(buffer);
    }
  }

  /// Find windows by partial title match (simple implementation without EnumWindows)
  /// This is a simplified approach that finds a specific window by title
  static int? findWindowContainingTitle(String partialTitle) {
    // For simplicity, we'll just try to find the exact window
    // In production, you might want to enumerate all windows
    return findWindowByTitle(partialTitle);
  }
}
