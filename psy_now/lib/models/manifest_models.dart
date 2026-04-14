/// Mirrors `SalsaNOW/Models/SalsaModels.cs` JSON shapes from the remote manifest.

class SavePathEntry {
  SavePathEntry({required this.configName, required this.directoryCreate});

  final String configName;
  final String directoryCreate;

  factory SavePathEntry.fromJson(Map<String, dynamic> j) => SavePathEntry(
        configName: (j['configName'] ?? j['ConfigName'])?.toString() ?? '',
        directoryCreate:
            (j['directoryCreate'] ?? j['DirectoryCreate'])?.toString() ?? '',
      );
}

class GamesSavePaths {
  GamesSavePaths({required this.paths});

  final List<String> paths;

  factory GamesSavePaths.fromJson(Map<String, dynamic> j) {
    final raw = j['paths'];
    if (raw is! List) return GamesSavePaths(paths: []);
    return GamesSavePaths(
      paths: raw.map((e) => e.toString()).toList(),
    );
  }
}

class ManifestAppEntry {
  ManifestAppEntry({
    required this.name,
    required this.fileExtension,
    required this.exeName,
    required this.run,
    required this.url,
  });

  final String name;
  final String fileExtension;
  final String exeName;
  final String run;
  final String url;

  factory ManifestAppEntry.fromJson(Map<String, dynamic> j) =>
      ManifestAppEntry(
        name: j['name'] as String? ?? '',
        fileExtension: (j['fileExtension'] as String? ?? '').toLowerCase(),
        exeName: j['exeName'] as String? ?? '',
        run: j['run'] as String? ?? '',
        url: j['url'] as String? ?? '',
      );
}

class ManifestSilentAppEntry {
  ManifestSilentAppEntry({
    required this.name,
    required this.fileExtension,
    required this.fileName,
    required this.archive,
    required this.run,
    required this.url,
  });

  final String name;
  final String fileExtension;
  final String fileName;
  final String archive;
  final String run;
  final String url;

  factory ManifestSilentAppEntry.fromJson(Map<String, dynamic> j) =>
      ManifestSilentAppEntry(
        name: j['name'] as String? ?? '',
        fileExtension: (j['fileExtension'] as String? ?? '').toLowerCase(),
        fileName: j['fileName'] as String? ?? '',
        archive: j['archive'] as String? ?? '',
        run: j['run'] as String? ?? '',
        url: j['url'] as String? ?? '',
      );
}

class ManifestDesktopEntry {
  ManifestDesktopEntry({
    required this.name,
    required this.exeName,
    required this.taskbarFixer,
    required this.zipConfig,
    required this.run,
    required this.url,
  });

  final String name;
  final String exeName;
  final String taskbarFixer;
  final String zipConfig;
  final String run;
  final String url;

  factory ManifestDesktopEntry.fromJson(Map<String, dynamic> j) =>
      ManifestDesktopEntry(
        name: j['name'] as String? ?? '',
        exeName: j['exeName'] as String? ?? '',
        taskbarFixer: j['taskbarFixer'] as String? ?? '',
        zipConfig: j['zipConfig'] as String? ?? '',
        run: j['run'] as String? ?? '',
        url: j['url'] as String? ?? '',
      );
}
