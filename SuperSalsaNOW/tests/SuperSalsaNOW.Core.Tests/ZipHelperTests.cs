using System.IO.Compression;
using SuperSalsaNOW.Core.Utilities;

namespace SuperSalsaNOW.Core.Tests;

public class ZipHelperTests : IAsyncLifetime
{
    private string _tempDir = null!;

    public Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SuperSalsaNOW.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // best-effort cleanup on CI/agents
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListEntriesAsync_returns_entry_paths()
    {
        var zipPath = Path.Combine(_tempDir, "a.zip");
        CreateZip(zipPath, ("readme.txt", "hi"), ("sub/nested.cfg", "x"));

        var entries = await ZipHelper.ListEntriesAsync(zipPath);

        Assert.Contains(entries, e => e.Replace('\\', '/') == "readme.txt");
        Assert.Contains(entries, e => e.Replace('\\', '/') == "sub/nested.cfg");
    }

    [Fact]
    public async Task ExtractAsync_writes_files()
    {
        var zipPath = Path.Combine(_tempDir, "b.zip");
        CreateZip(zipPath, ("out.txt", "content"));
        var dest = Path.Combine(_tempDir, "extract-here");
        Directory.CreateDirectory(dest);

        await ZipHelper.ExtractAsync(zipPath, dest);

        var extracted = Path.Combine(dest, "out.txt");
        Assert.True(File.Exists(extracted));
        Assert.Equal("content", await File.ReadAllTextAsync(extracted));
    }

    private static void CreateZip(string zipPath, params (string EntryName, string Content)[] files)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach (var (name, content) in files)
        {
            var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);
            writer.Write(content);
        }
    }
}
