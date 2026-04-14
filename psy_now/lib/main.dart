import 'dart:io';

import 'package:arcane/arcane.dart';
import 'package:fast_log/fast_log.dart';
import 'package:psy_now/models/app_state.dart';
import 'package:psy_now/screens/home_screen.dart';
import 'package:psy_now/services/config_service.dart';
import 'package:psy_now/services/environment_service.dart';
import 'package:psy_now/utils/constants.dart';

// ██████╗ ███████╗██╗   ██╗███╗   ██╗ ██████╗ ██╗    ██╗
// ██╔══██╗██╔════╝╚██╗ ██╔╝████╗  ██║██╔═══██╗██║    ██║
// ██████╔╝███████╗ ╚████╔╝ ██╔██╗ ██║██║   ██║██║ █╗ ██║
// ██╔═══╝ ╚════██║  ╚██╔╝  ██║╚██╗██║██║   ██║██║███╗██║
// ██║     ███████║   ██║   ██║ ╚████║╚██████╔╝╚███╔███╔╝
// ╚═╝     ╚══════╝   ╚═╝   ╚═╝  ╚═══╝ ╚═════╝  ╚══╝╚══╝
//
// PsyNow - GeForce NOW Environment Customization Tool
// Rebuilt with Arcane UI Framework
// Bundled apps: 7-Zip, Brave, Explorer++, DepotDownloader

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Initialize app state
  final appState = AppState();

  // Check GFN environment
  appState.isGfnEnvironment = await EnvironmentService.isGeForceNowEnvironment();

  // Use local install directory
  final globalDir = AppConstants.defaultInstallDir;
  try {
    await Directory(globalDir).create(recursive: true);
    appState.globalDirectory = globalDir;
    info('[Main] Install directory: $globalDir');
  } catch (e) {
    error('[Main] Error creating install directory: $e');
  }

  // Initialize config service
  ConfigService? configService;
  if (globalDir.isNotEmpty) {
    configService = ConfigService(globalDir);
    await configService.load();
  }

  runApp(
    'psy_now',
    MutablePylon<AppState>(
      value: appState,
      rebuildChildren: true,
      builder: (context) => Pylon<ConfigService?>(
        value: configService,
        builder: (context) => const PsyNowApp(),
      ),
    ),
  );
}

class PsyNowApp extends StatefulWidget {
  const PsyNowApp({super.key});

  @override
  State<PsyNowApp> createState() => _PsyNowAppState();
}

class _PsyNowAppState extends State<PsyNowApp> {
  ThemeMode _themeMode = ThemeMode.dark;

  void _toggleTheme() {
    setState(() {
      _themeMode = switch (_themeMode) {
        ThemeMode.light => ThemeMode.dark,
        ThemeMode.dark => ThemeMode.system,
        ThemeMode.system => ThemeMode.light,
      };
    });
  }

  @override
  Widget build(BuildContext context) {
    return ArcaneApp(
      debugShowCheckedModeBanner: false,
      theme: ArcaneTheme(
        scheme: ContrastedColorScheme(
          light: ColorSchemes.violet(ThemeMode.light),
          dark: ColorSchemes.violet(ThemeMode.dark),
        ),
        themeMode: _themeMode,
      ),
      home: const HomeScreen(),
    );
  }
}

/// Extension to provide easy access to theme mode toggle
extension PsyNowAppContext on BuildContext {
  void toggleTheme() {
    final state = findAncestorStateOfType<_PsyNowAppState>();
    state?._toggleTheme();
  }

  ThemeMode get currentThemeMode {
    final state = findAncestorStateOfType<_PsyNowAppState>();
    return state?._themeMode ?? ThemeMode.dark;
  }

  AppState get appState => pylon<AppState>();
  ConfigService? get configService => pylonOr<ConfigService?>();
}
