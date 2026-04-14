import 'package:arcane/arcane.dart';
import 'package:psy_now/main.dart';
import 'package:psy_now/models/app_state.dart';
import 'package:psy_now/screens/logs_screen.dart';
import 'package:psy_now/screens/settings_screen.dart';
import 'package:psy_now/services/app_installer.dart';
import 'package:psy_now/services/shortcut_service.dart';
import 'package:psy_now/services/steam_service.dart';
import 'package:psy_now/services/window_service.dart';
import 'package:psy_now/utils/constants.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  bool _isRunning = false;
  ShortcutService? _shortcutService;

  @override
  void dispose() {
    _shortcutService?.stopSyncLoop();
    WindowService.stopCustomExplorerKiller();
    super.dispose();
  }

  Future<void> _runSetup() async {
    if (_isRunning) return;

    final appState = context.appState;
    final config = context.configService;

    if (config == null || appState.globalDirectory.isEmpty) {
      appState.addLog('[!] Configuration not loaded');
      setState(() {});
      return;
    }

    setState(() => _isRunning = true);

    appState.addLog('[+] Starting PsyNow setup...');
    appState.addLog('[+] Install directory: ${appState.globalDirectory}');

    // Check environment
    if (!appState.isGfnEnvironment) {
      appState.addLog('[!] Not a GeForce NOW environment');
      appState.addLog('[!] Some features may not work');
    }

    // Start background services
    WindowService.startCustomExplorerKiller();
    appState.addLog('[+] Started CustomExplorer killer');

    // Steam server shutdown
    appState.steamStatus = InstallStatus.inProgress;
    setState(() {});

    final steamService = SteamService(
      globalDirectory: appState.globalDirectory,
      onLog: (msg) {
        appState.addLog(msg);
        setState(() {});
      },
    );
    await steamService.shutdownSteamServer();
    appState.steamStatus = InstallStatus.completed;
    setState(() {});

    // Install bundled apps
    appState.appsStatus = InstallStatus.inProgress;
    setState(() {});

    final appInstaller = AppInstaller(
      globalDirectory: appState.globalDirectory,
      config: config,
      onLog: (msg) {
        appState.addLog(msg);
        setState(() {});
      },
      onProgress: (name, progress) {
        appState.setProgress(name, progress);
        setState(() {});
      },
    );
    await appInstaller.installAll();
    appState.appsStatus = InstallStatus.completed;
    setState(() {});

    // Initialize shortcut service
    _shortcutService = ShortcutService(
      globalDirectory: appState.globalDirectory,
      onLog: (msg) {
        appState.addLog(msg);
        setState(() {});
      },
    );
    await _shortcutService!.initialize();
    await _shortcutService!.restoreShortcuts();
    _shortcutService!.startSyncLoop();
    appState.addLog('[+] Shortcut sync started');

    appState.addLog('[+] Setup complete!');
    setState(() => _isRunning = false);
  }

  @override
  Widget build(BuildContext context) {
    final appState = context.appState;

    return Screen(
      header: Bar(
        titleText: 'PsyNow',
        subtitleText: 'GeForce NOW Customization',
        trailing: [
          IconButton(
            icon: const Icon(Icons.gear_six),
            onPressed: () => Navigator.of(context).push(
              PageRouteBuilder(
                pageBuilder: (context, animation, secondaryAnimation) =>
                    const SettingsScreen(),
                transitionsBuilder:
                    (context, animation, secondaryAnimation, child) =>
                        FadeTransition(opacity: animation, child: child),
              ),
            ),
          ),
        ],
      ),
      gutter: true,
      child: Collection(
        children: [
          // Status card
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Icon(
                        appState.isGfnEnvironment
                            ? Icons.check_circle
                            : Icons.warning,
                        color: appState.isGfnEnvironment
                            ? const Color(0xFF4CAF50)
                            : const Color(0xFFFF9800),
                      ),
                      const Gap(8),
                      Expanded(
                        child: Text(
                          appState.isGfnEnvironment
                              ? 'GeForce NOW Environment Detected'
                              : 'Not a GeForce NOW Environment',
                          style: Theme.of(context).typography.large,
                        ),
                      ),
                    ],
                  ),
                  const Gap(8),
                  Text(
                    'Install to: ${appState.globalDirectory}',
                    style: Theme.of(context).typography.small,
                  ),
                ],
              ),
            ),
          ),
          const Gap(16),

          // Bundled apps info
          Section(
            titleText: 'Bundled Applications',
            child: Collection(
              children: [
                for (final app in AppConstants.bundledApps)
                  Tile(
                    leading: const Icon(Icons.package),
                    title: Text(app.name),
                    subtitle: Text(app.exeName),
                  ),
              ],
            ),
          ),
          const Gap(16),

          // Shell environments
          Section(
            titleText: 'Shell Environments',
            child: Collection(
              children: [
                for (final shell in AppConstants.shellEnvironments)
                  Tile(
                    leading: const Icon(Icons.layout),
                    title: Text(shell.name),
                    subtitle: Text(shell.exeName.isEmpty ? 'Config' : shell.exeName),
                  ),
              ],
            ),
          ),
          const Gap(16),

          // Action button
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16.0),
              child: Column(
                children: [
                  SizedBox(
                    width: double.infinity,
                    child: PrimaryButton(
                      onPressed: _isRunning ? null : _runSetup,
                      child: Padding(
                        padding: const EdgeInsets.symmetric(vertical: 12),
                        child: Text(
                          _isRunning ? 'Running...' : 'Run Setup',
                          style: Theme.of(context).typography.large,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          const Gap(16),

          // Installation status
          Section(
            titleText: 'Installation Status',
            child: Collection(
              children: [
                _buildStatusTile(
                  'Steam Integration',
                  appState.steamStatus,
                  Icons.game_controller,
                ),
                _buildStatusTile(
                  'Applications',
                  appState.appsStatus,
                  Icons.squares_four,
                ),
              ],
            ),
          ),
          const Gap(16),

          // Recent logs preview
          Section(
            titleText: 'Activity Log',
            child: Collection(
              children: [
                Card(
                  child: SizedBox(
                    height: 150,
                    child: StreamBuilder<List<String>>(
                      stream: appState.logsStream,
                      builder: (context, snapshot) {
                        final logs = snapshot.data ?? [];
                        if (logs.isEmpty) {
                          return const Center(
                            child: Text('No activity yet'),
                          );
                        }
                        return ListView.builder(
                          padding: const EdgeInsets.all(12),
                          itemCount: logs.length > 10 ? 10 : logs.length,
                          itemBuilder: (context, index) {
                            final logIndex = logs.length - 1 - index;
                            return Text(
                              logs[logIndex],
                              style: Theme.of(context).typography.small,
                            );
                          },
                        );
                      },
                    ),
                  ),
                ),
                const Gap(8),
                Tile(
                  leading: const Icon(Icons.list),
                  title: const Text('View Full Log'),
                  trailing: const Icon(Icons.chevron_forward_ionic),
                  onPressed: () => Navigator.of(context).push(
                    PageRouteBuilder(
                      pageBuilder: (context, animation, secondaryAnimation) =>
                          const LogsScreen(),
                      transitionsBuilder:
                          (context, animation, secondaryAnimation, child) =>
                              FadeTransition(opacity: animation, child: child),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusTile(String title, InstallStatus status, IconData icon) {
    final (statusText, statusColor) = switch (status) {
      InstallStatus.pending => ('Pending', const Color(0xFF9E9E9E)),
      InstallStatus.inProgress => ('In Progress', const Color(0xFF2196F3)),
      InstallStatus.completed => ('Completed', const Color(0xFF4CAF50)),
      InstallStatus.failed => ('Failed', const Color(0xFFF44336)),
    };

    return Tile(
      leading: Icon(icon),
      title: Text(title),
      subtitle: Text(statusText),
      trailing: Icon(
        status == InstallStatus.completed
            ? Icons.check_circle
            : status == InstallStatus.inProgress
                ? Icons.arrows_clockwise
                : status == InstallStatus.failed
                    ? Icons.x_circle
                    : Icons.circle,
        color: statusColor,
      ),
    );
  }
}
