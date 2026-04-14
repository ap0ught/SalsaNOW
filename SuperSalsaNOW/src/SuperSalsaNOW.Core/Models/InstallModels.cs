namespace SuperSalsaNOW.Core.Models;

/// <summary>
/// Result of a mod installation attempt
/// </summary>
public record InstallResult(
    bool Success,
    string? InstalledPath,
    List<string> Errors,
    List<string> Warnings
);

/// <summary>
/// Options controlling mod installation behavior
/// </summary>
public record InstallOptions(
    string TargetDirectory,
    bool OverwriteExisting,
    bool CreateShortcut
);

/// <summary>
/// Information about a downloadable mod file from Nexus
/// </summary>
public record ModFile(
    int FileId,
    string FileName,
    string Version,
    long SizeBytes,
    DateTime UploadedDate
);

/// <summary>
/// Time-limited download URL from Nexus CDN
/// </summary>
public record DownloadLink(
    string Url,
    DateTime ExpiresAt
);
