using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SalsaNOW
{
    internal static class DesktopInstaller
    {
        internal const uint SPI_SETDESKWALLPAPER = 0x0014;
        internal const uint SPIF_UPDATEINIFILE = 0x01;
        internal const uint SPIF_SENDCHANGE = 0x02;

        static readonly string[] SupportedExtensions =
        {
        ".bmp",
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".tif",
        ".tiff",
        ".webp",
        ".jxr"
        };

        // Setup for Desktop shells and visual personalization
        public static async Task DesktopInstallAsync(string globalDirectory)
        {
            string defaultWallpaperDir = Path.Combine(globalDirectory, "DesktopWallpaper", "DefaultWallpaper");
            string userWallpaperDir = Path.Combine(globalDirectory, "DesktopWallpaper");
            const string jsonUrl = "https://salsanowfiles.work/jsons/ExplorerDesktop.json";

            // 1. Enforce Dark Mode
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    key?.SetValue("AppsUseLightTheme", 0, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
            catch (Exception ex) { SalsaLogger.Error("Failed to set Dark Mode: " + ex.Message); }

            // 2. Set default wallpaper from the Wallpapers user directory, if nothing is found then we apply the default wallpaper
            if (!Directory.Exists(userWallpaperDir))
            {
                Directory.CreateDirectory(userWallpaperDir);
                Directory.CreateDirectory(defaultWallpaperDir);

                using (var webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(new Uri("https://salsanowfiles.work/ExplorerContents/Wallpaper/WallpaperWin11.jpg"), $"{defaultWallpaperDir}\\WallpaperWin11.jpg");
                }
            }

            string wallpaper = Directory
                .EnumerateFiles(userWallpaperDir)
                .FirstOrDefault(f =>
                    SupportedExtensions.Contains(
                        Path.GetExtension(f),
                        StringComparer.OrdinalIgnoreCase));

            if (wallpaper == null)
            {
                // Apply default wallpaper if no user-defined wallpaper is found
                bool success = NativeMethods.SystemParametersInfo(
                    SPI_SETDESKWALLPAPER,
                    0,
                    $"{defaultWallpaperDir}\\WallpaperWin11.jpg",
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
                );
            }
            else
            {
                // Apply user-defined wallpaper if found
                bool success = NativeMethods.SystemParametersInfo(
                    SPI_SETDESKWALLPAPER,
                    0,
                    wallpaper,
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
                );
            }

            // 3. Fetch and install desktop from remote JSON
            try
            {
                List<DesktopInfo> desktopInfo;
                using (var wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(jsonUrl);
                    desktopInfo = JsonConvert.DeserializeObject<List<DesktopInfo>>(json);
                }

                // Close existing shells before attempting updates.
                // try the dynamic detector first
                // fall back to the hardcoded "CustomExplorer" name for legacy compatibility.
                var processes = Process.GetProcessesByName("CustomExplorer");
                var detected = GfnShellDetector.FindShellProcess();
                if (detected != null && processes.All(p => p.Id != detected.Id)) processes = processes.Concat(new[] { detected }).ToArray();
                foreach (var p in processes) { try { p.Kill(); } catch { } p.Dispose(); }

                bool skipSeelen = SalsaSettings.SkipSeelenUiExecution;
                bool seelenStarted = false;

                foreach (var desktop in desktopInfo)
                {
                    string appDir = Path.Combine(globalDirectory, desktop.name);
                    string versionMarkerFile = Path.Combine(appDir, ".version");
                    string remoteFileName = Path.GetFileName(new Uri(desktop.url).AbsolutePath);

                    bool needsInstall = !Directory.Exists(appDir) || !File.Exists(versionMarkerFile);

                    // Check if current version marker matches the remote filename
                    if (!needsInstall && File.Exists(versionMarkerFile))
                    {
                        string localVersion = File.ReadAllText(versionMarkerFile);
                        if (localVersion != remoteFileName)
                            needsInstall = true; // Version mismatch, trigger re-install
                    }

                    // Perform installation/update
                    if (needsInstall)
                    {
                        string zipFile = Path.Combine(globalDirectory, $"{desktop.name}_temp.zip");
                        string stagingDir = appDir + ".staging";
                        bool replaced = false;

                        try
                        {
                            if (Directory.Exists(stagingDir) && !SafeDeleteDirectory(stagingDir))
                                throw new IOException("Failed to clear staging directory: " + stagingDir);

                            using (var wc = new WebClient())
                            {
                                wc.Headers.Add("Cache-Control", "no-cache");
                                await wc.DownloadFileTaskAsync(new Uri(desktop.url), zipFile);
                            }

                            if (!File.Exists(zipFile) || new FileInfo(zipFile).Length == 0)
                                throw new IOException("Downloaded archive for " + desktop.name + " is empty or missing.");

                            Directory.CreateDirectory(stagingDir);
                            try { ZipFile.ExtractToDirectory(zipFile, stagingDir); }
                            catch (Exception ex) { throw new IOException("Failed to extract " + zipFile + ": " + ex.Message, ex); }

                            // The previous installation is left untouched until the replacement has been validated.
                            if (!string.IsNullOrEmpty(desktop.exeName) &&
                                !File.Exists(Path.Combine(stagingDir, desktop.exeName)))
                                throw new InvalidOperationException("Extracted archive for " + desktop.name + " is missing executable " + desktop.exeName + ".");

                            if (!SafeDeleteDirectory(appDir))
                                throw new IOException("Failed to remove existing installation of " + desktop.name + " at " + appDir + ".");

                            Directory.Move(stagingDir, appDir);
                            File.WriteAllText(versionMarkerFile, remoteFileName);
                            replaced = true;
                        }
                        finally
                        {
                            if (File.Exists(zipFile)) { try { File.Delete(zipFile); } catch { } }
                            if (!replaced && Directory.Exists(stagingDir)) SafeDeleteDirectory(stagingDir);
                        }
                    }

                    // Seelen UI: push a clean config every session unless the user opted out.
                    // Runs before the universal launch below so Seelen boots with a fresh config.
                    bool seelenDesktop = desktop.name.Contains("seelenui");
                    if (seelenDesktop && !skipSeelen)
                        await ApplySeelenConfig(desktop.zipConfig);

                    if (seelenDesktop && skipSeelen)
                        continue; // user opted out of Seelen UI

                    // Universal Launch Logic
                    if (string.Equals(desktop.run, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        if (seelenDesktop) seelenStarted = true;

                        string exePath = Path.Combine(appDir, desktop.exeName);

                        SalsaLogger.Info("Starting desktop app: " + exePath);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            WorkingDirectory = appDir,
                            UseShellExecute = false
                        });
                    }
                }

                // Suppress the initial Seelen UI settings/splash popup once it appears
                if (seelenStarted)
                    await SeelenSettingsLoop();

                if (SalsaSettings.BingWallpaperEnabled)
                {
                    await DownloadBingWallpaper(userWallpaperDir);
                }
            }
            catch (Exception ex) { SalsaLogger.Error(ex.ToString()); }
        }

        private static bool SafeDeleteDirectory(string path, int retries = 3)
        {
            if (!Directory.Exists(path)) return true;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    Directory.Delete(path, true);
                    return true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }

            SalsaLogger.Error("Failed to delete directory after " + retries + " attempts: " + path);
            return false;
        }

        // Extracts fresh Seelen UI config, cleaning the target directory beforehand to prevent corruption
        private static async Task<bool> ApplySeelenConfig(string configZipUrl)
        {
            if (string.IsNullOrWhiteSpace(configZipUrl))
                return false;

            string target = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "com.seelen.seelen-ui");

            string zip = target + ".zip";
            const int maxRetries = 5;

            try
            {
                using (var wc = new WebClient())
                    await wc.DownloadFileTaskAsync(new Uri(configZipUrl), zip);

                if (!File.Exists(zip) || new FileInfo(zip).Length == 0)
                    throw new IOException("Downloaded Seelen UI config is empty or missing.");

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        // Remove existing config
                        if (Directory.Exists(target))
                        {
                            Directory.Delete(target, true);

                            if (Directory.Exists(target))
                                throw new IOException("Failed to delete target directory.");
                        }

                        // Extract new config
                        Directory.CreateDirectory(target);
                        ZipFile.ExtractToDirectory(zip, target);

                        // Verify extraction
                        if (!Directory.EnumerateFileSystemEntries(target).Any())
                            throw new IOException("Extraction verification failed.");

                        return true;
                    }
                    catch
                    {
                        // Clean up partial extraction before retrying
                        try { if (Directory.Exists(target)) Directory.Delete(target, true); } catch { }

                        if (attempt == maxRetries)
                        {
                            SalsaLogger.Error("Failed to apply Seelen UI config after " + maxRetries + " attempts.");
                            return false;
                        }

                        await Task.Delay(500);
                    }
                }

                return false;
            }
            finally
            {
                if (File.Exists(zip)) { try { File.Delete(zip); } catch { } }
            }
        }

        // Watches for Seelen UI's initial settings/splash window and closes it once it appears.
        // Bounded so a missing window can never stall the rest of startup.
        private static async Task SeelenSettingsLoop()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);

            while (DateTime.UtcNow < deadline)
            {
                bool foundSettings = false;
                IntPtr settingsOwner = IntPtr.Zero;

                NativeMethods.EnumWindows((hWnd, lp) =>
                {
                    NativeMethods.EnumChildWindows(hWnd, (child, cLp) =>
                    {
                        var sb = new StringBuilder(512);
                        NativeMethods.GetWindowText(child, sb, sb.Capacity);
                        string title = sb.ToString();

                        if (title.Equals("tauri.localhost/settings/index.html", StringComparison.OrdinalIgnoreCase))
                        {
                            settingsOwner = hWnd;
                            foundSettings = true;

                            return false; // stop child enumeration
                        }

                        return true;
                    }, IntPtr.Zero);

                    return !foundSettings; // stop EnumWindows if found
                }, IntPtr.Zero);

                if (foundSettings && settingsOwner != IntPtr.Zero)
                {
                    await Task.Delay(500); // let the settings window finish loading before closing
                    NativeMethods.PostMessage(settingsOwner, (uint)NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    SalsaLogger.Info("Seelen UI settings window has been suppressed.");
                    return;
                }

                await Task.Delay(500);
            }
        }

        // Fetches and applies the UHD Bing Photo of the Day
        private static async Task DownloadBingWallpaper(string dir)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-AU");
                    var url = JObject.Parse(json)["images"][0]["urlbase"].ToString();
                    await wc.DownloadFileTaskAsync(new Uri($"https://www.bing.com{url}_UHD.jpg"), Path.Combine(dir, "wallpaper.jpg"));

                    // Apply bing wallpaper as the desktop background at users request from config file
                    bool success = NativeMethods.SystemParametersInfo(
                        SPI_SETDESKWALLPAPER,
                        0,
                        Path.Combine(dir, "wallpaper.jpg"),
                        SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
                    );
                }
            }
            catch { }
        }
    }
}
