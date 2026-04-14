import 'package:arcane/arcane.dart';
import 'package:psy_now/main.dart';

class LogsScreen extends StatelessWidget {
  const LogsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final appState = context.appState;

    return Screen(
      header: Bar(
        titleText: 'Activity Log',
        leading: [
          IconButton(
            icon: const Icon(Icons.arrow_left),
            onPressed: () => Navigator.of(context).pop(),
          ),
        ],
        trailing: [
          IconButton(
            icon: const Icon(Icons.trash),
            onPressed: () {
              appState.clearLogs();
            },
          ),
        ],
      ),
      gutter: true,
      child: StreamBuilder<List<String>>(
        stream: appState.logsStream,
        builder: (context, snapshot) {
          final logs = snapshot.data ?? [];

          if (logs.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.list, size: 48),
                  const Gap(16),
                  const Text('No activity yet'),
                  const Gap(8),
                  Text(
                    'Run setup to see activity logs',
                    style: Theme.of(context).typography.small,
                  ),
                ],
              ),
            );
          }

          return Card(
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: logs.length,
              itemBuilder: (context, index) {
                final logIndex = logs.length - 1 - index;
                final log = logs[logIndex];

                // Color code based on log type
                Color textColor = Theme.of(context).colorScheme.primary;
                if (log.contains('[+]')) {
                  textColor = const Color(0xFF4CAF50);
                } else if (log.contains('[!]')) {
                  textColor = const Color(0xFFFF9800);
                } else if (log.contains('Error') || log.contains('error')) {
                  textColor = const Color(0xFFF44336);
                }

                return Padding(
                  padding: const EdgeInsets.symmetric(vertical: 2),
                  child: Text(
                    log,
                    style: Theme.of(context).typography.small.copyWith(
                          color: textColor,
                          fontFamily: 'monospace',
                        ),
                  ),
                );
              },
            ),
          );
        },
      ),
    );
  }
}
