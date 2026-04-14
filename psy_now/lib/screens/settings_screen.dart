import 'package:arcane/arcane.dart';
import 'package:psy_now/main.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  @override
  Widget build(BuildContext context) {
    final config = context.configService;

    return Screen(
      header: Bar(
        titleText: 'Settings',
        leading: [
          IconButton(
            icon: const Icon(Icons.arrow_left),
            onPressed: () => Navigator.of(context).pop(),
          ),
        ],
      ),
      gutter: true,
      child: Collection(
        children: [
          // Appearance section
          Section(
            titleText: 'Appearance',
            child: Collection(
              children: [
                Tile(
                  leading: const Icon(Icons.moon),
                  title: const Text('Theme Mode'),
                  subtitle: Text(_getThemeModeText(context)),
                  trailing: const Icon(Icons.chevron_forward_ionic),
                  onPressed: () {
                    context.toggleTheme();
                    setState(() {});
                  },
                ),
              ],
            ),
          ),
          const Gap(16),

          // PsyNow config section
          if (config != null) ...[
            Section(
              titleText: 'PsyNow Options',
              child: Collection(
                children: [
                  _buildSwitchTile(
                    icon: Icons.link,
                    title: 'Skip Shortcuts Creation',
                    subtitle: 'Don\'t create desktop shortcuts on startup',
                    value: config.skipShortcutsCreation,
                    onChanged: (value) async {
                      await config.setSkipShortcutsCreation(value);
                      setState(() {});
                    },
                  ),
                  _buildSwitchTile(
                    icon: Icons.desktop,
                    title: 'Skip Seelen UI',
                    subtitle: 'Don\'t launch Seelen UI desktop shell',
                    value: config.skipSeelenUiExecution,
                    onChanged: (value) async {
                      await config.setSkipSeelenUiExecution(value);
                      setState(() {});
                    },
                  ),
                  _buildSwitchTile(
                    icon: Icons.image,
                    title: 'Bing Wallpaper',
                    subtitle: 'Use Bing photo of the day as wallpaper',
                    value: config.bingPhotoOfTheDayWallpaper,
                    onChanged: (value) async {
                      await config.setBingPhotoOfTheDayWallpaper(value);
                      setState(() {});
                    },
                  ),
                ],
              ),
            ),
            const Gap(16),
          ],

          // About section
          Section(
            titleText: 'About',
            child: Collection(
              children: [
                const Tile(
                  leading: Icon(Icons.info),
                  title: Text('Version'),
                  subtitle: Text('1.0.0'),
                ),
                const Tile(
                  leading: Icon(Icons.code),
                  title: Text('Framework'),
                  subtitle: Text('Arcane UI'),
                ),
                const Tile(
                  leading: Icon(Icons.user),
                  title: Text('Original Author'),
                  subtitle: Text('dpadGuy (SalsaNOW)'),
                ),
                Tile(
                  leading: const Icon(Icons.arrow_square_out),
                  title: const Text('GitHub'),
                  subtitle: const Text('View source code'),
                  trailing: const Icon(Icons.chevron_forward_ionic),
                  onPressed: () {
                    // Could open GitHub URL here
                  },
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSwitchTile({
    required IconData icon,
    required String title,
    required String subtitle,
    required bool value,
    required ValueChanged<bool> onChanged,
  }) {
    return Tile(
      leading: Icon(icon),
      title: Text(title),
      subtitle: Text(subtitle),
      trailing: Switch(
        value: value,
        onChanged: onChanged,
      ),
      onPressed: () => onChanged(!value),
    );
  }

  String _getThemeModeText(BuildContext context) {
    return switch (context.currentThemeMode) {
      ThemeMode.light => 'Light',
      ThemeMode.dark => 'Dark',
      ThemeMode.system => 'System',
    };
  }
}
