import 'package:rxdart/rxdart.dart';

/// Installation status for tracking progress
enum InstallStatus {
  pending,
  inProgress,
  completed,
  failed,
}

/// App-wide state management
class AppState {
  // Environment
  bool isGfnEnvironment = false;
  String globalDirectory = '';

  // Installation status
  InstallStatus appsStatus = InstallStatus.pending;
  InstallStatus steamStatus = InstallStatus.pending;

  // Progress tracking
  Map<String, double> downloadProgress = {};

  // Logs
  final _logs = BehaviorSubject<List<String>>.seeded([]);
  Stream<List<String>> get logsStream => _logs.stream;
  List<String> get logs => _logs.value;

  /// Add a log message
  void addLog(String message) {
    final timestamp = DateTime.now().toString().substring(11, 19);
    _logs.add([...logs, '[$timestamp] $message']);
  }

  /// Clear all logs
  void clearLogs() {
    _logs.add([]);
  }

  /// Update download progress for an item
  void setProgress(String name, double progress) {
    downloadProgress = {...downloadProgress, name: progress};
  }

  /// Check if any installation is in progress
  bool get isInstalling =>
      appsStatus == InstallStatus.inProgress ||
      steamStatus == InstallStatus.inProgress;

  /// Check if all installations are complete
  bool get isComplete =>
      appsStatus == InstallStatus.completed &&
      steamStatus == InstallStatus.completed;

  /// Reset all status to pending
  void reset() {
    appsStatus = InstallStatus.pending;
    steamStatus = InstallStatus.pending;
    downloadProgress = {};
    clearLogs();
  }

  void dispose() {
    _logs.close();
  }
}
