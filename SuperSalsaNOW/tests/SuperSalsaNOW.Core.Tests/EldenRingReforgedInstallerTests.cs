using Microsoft.Extensions.Logging.Abstractions;
using SuperSalsaNOW.Core.Interfaces;
using SuperSalsaNOW.Core.Models;
using SuperSalsaNOW.Core.Services;

namespace SuperSalsaNOW.Core.Tests;

public class EldenRingReforgedInstallerTests
{
    private static ModDefinition SampleMod =>
        new("my-mod", "Name", "Desc", new NexusInfo("eldenring", 1, "main"), InstallStrategy.ErrLauncher);

    [Fact]
    public void GetInstallDirectory_combines_target_and_mod_id()
    {
        var installer = new EldenRingReforgedInstaller(new StubNexus(), NullLogger<EldenRingReforgedInstaller>.Instance);
        var options = new InstallOptions(@"C:\games\mods", false, false);

        var path = installer.GetInstallDirectory(SampleMod, options);

        Assert.Equal(Path.Combine(@"C:\games\mods", "my-mod"), path);
    }

    [Fact]
    public async Task VerifyInstallationAsync_returns_true_when_directory_exists()
    {
        var dir = Path.Combine(Path.GetTempPath(), "SuperSalsaNOW.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var installer = new EldenRingReforgedInstaller(new StubNexus(), NullLogger<EldenRingReforgedInstaller>.Instance);
            var ok = await installer.VerifyInstallationAsync(SampleMod, dir);
            Assert.True(ok);
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // ignore
            }
        }
    }

    [Fact]
    public async Task VerifyInstallationAsync_returns_false_when_directory_missing()
    {
        var installer = new EldenRingReforgedInstaller(new StubNexus(), NullLogger<EldenRingReforgedInstaller>.Instance);
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "nope");
        var ok = await installer.VerifyInstallationAsync(SampleMod, missing);
        Assert.False(ok);
    }

    [Fact]
    public async Task InstallAsync_returns_failure_when_no_matching_file()
    {
        var nexus = new StubNexus { SelectResult = null };
        var installer = new EldenRingReforgedInstaller(nexus, NullLogger<EldenRingReforgedInstaller>.Instance);
        var options = new InstallOptions(Path.GetTempPath(), false, false);

        var result = await installer.InstallAsync(SampleMod, options);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("No file found", StringComparison.Ordinal));
    }

    private sealed class StubNexus : INexusClient
    {
        public ModFile? SelectResult { get; init; }

        public Task<List<ModFile>> GetModFilesAsync(string gameDomain, int modId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new List<ModFile>
            {
                new(1, "x.zip", "1", 100, DateTime.UtcNow),
            });

        public Task<List<DownloadLink>> GetDownloadLinksAsync(string gameDomain, int modId, int fileId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new List<DownloadLink>());

        public ModFile? SelectFile(List<ModFile> files, string pattern) => SelectResult;
    }
}
