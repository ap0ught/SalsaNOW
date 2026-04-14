namespace SuperSalsaNOW.Core.Utilities;

using System.IO.Compression;

/// <summary>
/// Helper for working with ZIP archives
/// </summary>
public static class ZipHelper
{
    /// <summary>
    /// Extract a ZIP archive to destination directory
    /// </summary>
    /// <param name="archivePath">Path to ZIP file</param>
    /// <param name="destinationDirectory">Destination directory for extraction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task ExtractAsync(string archivePath, string destinationDirectory, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            ZipFile.ExtractToDirectory(archivePath, destinationDirectory, overwriteFiles: true);
        }, cancellationToken);
    }

    /// <summary>
    /// List all entries in a ZIP archive
    /// </summary>
    /// <param name="archivePath">Path to ZIP file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of entry paths within archive</returns>
    public static async Task<List<string>> ListEntriesAsync(string archivePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var archive = ZipFile.OpenRead(archivePath);
            return archive.Entries.Select(e => e.FullName).ToList();
        }, cancellationToken);
    }
}
