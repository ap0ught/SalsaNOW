namespace SuperSalsaNOW.Core.Utilities;

/// <summary>
/// Helper for downloading files with progress reporting
/// </summary>
public static class DownloadHelper
{
    /// <summary>
    /// Download a file from URL to local path with progress reporting
    /// </summary>
    /// <param name="url">Source URL</param>
    /// <param name="destinationPath">Destination file path</param>
    /// <param name="progress">Progress reporter (0-100 percentage)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task DownloadFileAsync(string url, string destinationPath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? 0;
        var bytesRead = 0L;

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[8192];
        int read;

        while ((read = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            bytesRead += read;

            if (totalBytes > 0 && progress != null)
            {
                var percentComplete = (double)bytesRead / totalBytes * 100;
                progress.Report(percentComplete);
            }
        }
    }
}
