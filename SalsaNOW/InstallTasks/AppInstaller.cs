using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SalsaNOW
{
    internal static class AppInstaller
    {
        // Parallel installation of user-defined apps from remote and local JSON sources
        public static async Task AppsInstallAsync(string globalDirectory, string customAppsJsonPath)
        {
            const string jsonUrl = "https://salsanowfiles.work/jsons/appsV2.json";
            try
            {
                List<Apps> apps;
                using (var wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(jsonUrl);
                    apps = JsonConvert.DeserializeObject<List<Apps>>(json);
                }

                // Load custom apps from local JSON if provided via arguments
                if (!string.IsNullOrEmpty(customAppsJsonPath) && System.IO.File.Exists(customAppsJsonPath))
                {
                    try
                    {
                        var customApps = JsonConvert.DeserializeObject<List<Apps>>(System.IO.File.ReadAllText(customAppsJsonPath));
                        if (customApps != null) apps.AddRange(customApps);
                    }
                    catch (Exception ex) { SalsaLogger.Error($"Custom JSON Error: {ex.Message}"); }
                }

                var tasks = apps.Select(app => Task.Run(async () =>
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Cache-Control", "no-cache");
                        webClient.Headers.Add("Pragma", "no-cache");

                        string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{app.name}.lnk");
                        string appDir = Path.Combine(globalDirectory, app.name);
                        string appExePath = Path.Combine(globalDirectory, app.exeName);
                        string appZipExe = Path.Combine(appDir, app.exeName);
                        bool isZip = app.fileExtension == "zip";
                        bool isExe = app.fileExtension == "exe";

                        if (isExe)
                        {
                            SalsaLogger.Info("Downloading " + app.name);
                            string tmpExe = appExePath + ".tmp";
                            try
                            {
                                await webClient.DownloadFileTaskAsync(new Uri(app.url), tmpExe);
                                if (!LooksLikeExecutable(tmpExe))
                                    throw new InvalidOperationException("Downloaded file for " + app.name + " is not a valid executable.");

                                ReplaceFileAtomic(tmpExe, appExePath);
                            }
                            finally
                            {
                                if (File.Exists(tmpExe)) { try { File.Delete(tmpExe); } catch { } }
                            }

                            if (ShouldCreateDesktopShortcut(globalDirectory, desktopPath))
                                CreateShortcut(app.name, desktopPath, appExePath, globalDirectory);
                            if (app.run == "true") Process.Start(appExePath);
                            return;
                        }

                        if (!isZip)
                            return;

                        string versionKey = GetVersionKey(app.version);
                        string versionMarkerFile = Path.Combine(appDir, ".version");
                        bool alreadyExists = Directory.Exists(appDir);
                        bool hasNewerVersion = HasRemoteUpdate(versionMarkerFile, versionKey);

                        if (!alreadyExists || hasNewerVersion)
                        {
                            if (alreadyExists && hasNewerVersion)
                                SalsaLogger.Info($"Updating {app.name} to {versionKey}");
                            else
                                SalsaLogger.Info("Installing " + app.name);

                            string zipPath = $"{appDir}.zip";
                            string stagingDir = appDir + ".staging";
                            bool replaced = false;

                            try
                            {
                                if (Directory.Exists(stagingDir) && !SafeDeleteDirectory(stagingDir))
                                    throw new IOException("Failed to clear staging directory: " + stagingDir);

                                await webClient.DownloadFileTaskAsync(new Uri(app.url), zipPath);
                                if (!File.Exists(zipPath) || new FileInfo(zipPath).Length == 0)
                                    throw new IOException("Downloaded archive for " + app.name + " is empty or missing.");

                                Directory.CreateDirectory(stagingDir);
                                try { ZipFile.ExtractToDirectory(zipPath, stagingDir); }
                                catch (Exception ex) { throw new IOException("Failed to extract " + zipPath + ": " + ex.Message, ex); }

                                // The previous installation is left untouched until the replacement has been validated.
                                if (!File.Exists(Path.Combine(stagingDir, app.exeName)))
                                    throw new InvalidOperationException("Extracted archive for " + app.name + " is missing executable " + app.exeName + ".");

                                if (alreadyExists && !SafeDeleteDirectory(appDir))
                                    throw new IOException("Failed to remove existing installation of " + app.name + " at " + appDir + ".");

                                Directory.Move(stagingDir, appDir);
                                WriteVersionMarker(versionMarkerFile, versionKey);
                                replaced = true;
                            }
                            finally
                            {
                                if (File.Exists(zipPath)) { try { File.Delete(zipPath); } catch { } }
                                if (!replaced && Directory.Exists(stagingDir)) SafeDeleteDirectory(stagingDir);
                            }

                            if (ShouldCreateDesktopShortcut(globalDirectory, desktopPath))
                                CreateShortcut(app.name, desktopPath, appZipExe, Path.GetDirectoryName(appZipExe));
                            if (app.run == "true") Process.Start(appZipExe);
                        }
                        else
                        {
                            SalsaLogger.Info($"{app.name} already exists. Skipping download and respecting user desktop layout.");
                            if (app.run == "true") Process.Start(appZipExe);
                        }
                    }
                })).ToList();

                await Task.WhenAll(tasks);
            }
            catch (Exception ex) { SalsaLogger.Error(ex.Message); }
        }

        // Silent background app deployment with automated cleanup of obsolete files/folders
        public static async Task AppsInstallSilentAsync(string globalDirectory)
        {
            const string jsonUrl = "https://salsanowfiles.work/jsons/silentappsV2.json";
            string silentAppsPath = Path.Combine(globalDirectory, "SilentApps");

            try
            {
                Directory.CreateDirectory(silentAppsPath);
                List<SilentApps> apps;
                using (var wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(jsonUrl);
                    apps = JsonConvert.DeserializeObject<List<SilentApps>>(json);
                }

                // Clean up folders and files that are no longer present in the JSON definition
                var allowedFolders = new HashSet<string>(apps.Where(a => a.archive == "true").Select(a => a.name), StringComparer.OrdinalIgnoreCase);
                var allowedFiles = new HashSet<string>(apps.Where(a => a.fileExtension == "exe" || a.fileExtension == "bat").Select(a => $"{a.fileName}.{a.fileExtension}"), StringComparer.OrdinalIgnoreCase);

                foreach (var dir in Directory.GetDirectories(silentAppsPath))
                {
                    if (!allowedFolders.Contains(Path.GetFileName(dir))) try { Directory.Delete(dir, true); } catch { }
                }
                foreach (var file in Directory.GetFiles(silentAppsPath))
                {
                    string name = Path.GetFileName(file);
                    if (name.EndsWith(".version", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!allowedFiles.Contains(name)) try { System.IO.File.Delete(file); } catch { }
                }

                var tasks = apps.Select(app => Task.Run(async () =>
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Cache-Control", "no-cache");
                        webClient.Headers.Add("Pragma", "no-cache");

                        string appFolder = Path.Combine(silentAppsPath, app.name);
                        string appPath = Path.Combine(silentAppsPath, $"{app.fileName}.{app.fileExtension}");
                        string appZipPath = Path.Combine(appFolder, $"{app.fileName}.{app.fileExtension}");
                        string versionKey = GetVersionKey(app.version);
                        bool isArchive = app.archive == "true";
                        bool isOpenShell = app.name.Equals("Open-Shell", StringComparison.OrdinalIgnoreCase);

                        string versionMarkerFile = isArchive
                            ? Path.Combine(appFolder, ".version")
                            : Path.Combine(silentAppsPath, $"{app.fileName}.version");

                        bool alreadyExists = isArchive
                            ? System.IO.File.Exists(appZipPath)
                            : System.IO.File.Exists(appPath);
                        bool hasNewerVersion = HasRemoteUpdate(versionMarkerFile, versionKey);

                        if (!isOpenShell && alreadyExists && !hasNewerVersion)
                        {
                            if (app.run == "true")
                            {
                                string runPath = isArchive ? appZipPath : appPath;
                                StartSilentApp(runPath, isArchive);
                            }
                            return;
                        }

                        if (alreadyExists && hasNewerVersion)
                            SalsaLogger.Info($"Updating silent app {app.name} to {versionKey}");
                        else if (!alreadyExists)
                            SalsaLogger.Info("Installing silent app " + app.name);

                        if (isArchive)
                        {
                            string zip = $"{appFolder}.zip";
                            string stagingDir = appFolder + ".staging";
                            bool replaced = false;

                            try
                            {
                                if (Directory.Exists(stagingDir) && !SafeDeleteDirectory(stagingDir))
                                    throw new IOException("Failed to clear staging directory: " + stagingDir);

                                await webClient.DownloadFileTaskAsync(new Uri(app.url), zip);
                                if (!File.Exists(zip) || new FileInfo(zip).Length == 0)
                                    throw new IOException("Downloaded archive for " + app.name + " is empty or missing.");

                                Directory.CreateDirectory(stagingDir);
                                try { ZipFile.ExtractToDirectory(zip, stagingDir); }
                                catch (Exception ex) { throw new IOException("Failed to extract " + zip + ": " + ex.Message, ex); }

                                // The previous installation is left untouched until the replacement has been validated.
                                if (!File.Exists(Path.Combine(stagingDir, app.fileName + "." + app.fileExtension)))
                                    throw new InvalidOperationException("Extracted archive for " + app.name + " is missing executable " + app.fileName + "." + app.fileExtension + ".");

                                if (Directory.Exists(appFolder) && !SafeDeleteDirectory(appFolder))
                                    throw new IOException("Failed to remove existing installation of " + app.name + " at " + appFolder + ".");

                                Directory.Move(stagingDir, appFolder);
                                WriteVersionMarker(versionMarkerFile, versionKey);
                                replaced = true;
                            }
                            finally
                            {
                                if (File.Exists(zip)) { try { File.Delete(zip); } catch { } }
                                if (!replaced && Directory.Exists(stagingDir)) SafeDeleteDirectory(stagingDir);
                            }

                            if (app.run == "true") Process.Start(appZipPath);
                        }
                        else
                        {
                            string tmpPath = appPath + ".tmp";
                            try
                            {
                                await webClient.DownloadFileTaskAsync(new Uri(app.url), tmpPath);
                                if (app.fileExtension == "exe" && !LooksLikeExecutable(tmpPath))
                                    throw new InvalidOperationException("Downloaded file for " + app.name + " is not a valid executable.");

                                ReplaceFileAtomic(tmpPath, appPath);
                            }
                            finally
                            {
                                if (File.Exists(tmpPath)) { try { File.Delete(tmpPath); } catch { } }
                            }

                            WriteVersionMarker(versionMarkerFile, versionKey);

                            if (app.run == "true")
                                StartSilentApp(appPath, false);
                        }
                    }
                })).ToList();

                await Task.WhenAll(tasks);
            }
            catch (Exception ex) { SalsaLogger.Error(ex.ToString()); }
        }

        private static string GetVersionKey(string version)
        {
            if (!string.IsNullOrWhiteSpace(version))
                return version.Trim();

            return "1.0.0";
        }

        private static string GetRemoteFileName(string url)
        {
            try
            {
                return Uri.UnescapeDataString(Path.GetFileName(new Uri(url).AbsolutePath));
            }
            catch
            {
                return Path.GetFileName(url ?? string.Empty);
            }
        }

        // Reads dotted versions from names like EZManifest-1.2.0.zip
        private static bool TryParseFileVersion(string fileName, out Version version)
        {
            version = null;
            if (string.IsNullOrEmpty(fileName)) return false;

            var match = Regex.Match(fileName, @"\d+\.\d+(?:\.\d+){0,2}");
            if (!match.Success) return false;

            if (!Version.TryParse(match.Value, out var parsed)) return false;

            version = NormalizeVersion(parsed);
            return true;
        }

        private static Version NormalizeVersion(Version version)
        {
            return new Version(
                version.Major,
                version.Minor,
                version.Build < 0 ? 0 : version.Build,
                version.Revision < 0 ? 0 : version.Revision);
        }

        private static void StartSilentApp(string path, bool useShellExecute)
        {
            if (!System.IO.File.Exists(path))
                return;

            if (useShellExecute)
            {
                Process.Start(path);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }

        private static readonly Version BaselineVersion = new Version(1, 0, 0, 0);

        private static bool HasRemoteUpdate(string versionMarkerFile, string versionKey)
        {
            if (string.IsNullOrEmpty(versionKey))
                return false;

            if (!TryParseFileVersion(versionKey, out var remoteVersion))
                return false;

            if (remoteVersion <= BaselineVersion)
                return false;

            if (!System.IO.File.Exists(versionMarkerFile))
                return true;

            string localText = System.IO.File.ReadAllText(versionMarkerFile).Trim();
            if (!TryParseFileVersion(localText, out var localVersion))
                return true;

            return remoteVersion > localVersion;
        }

        private static void WriteVersionMarker(string versionMarkerFile, string remoteFileName)
        {
            try
            {
                string directory = Path.GetDirectoryName(versionMarkerFile);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                System.IO.File.WriteAllText(versionMarkerFile, remoteFileName);
            }
            catch (Exception ex) { SalsaLogger.Error($"Failed to write version marker: {ex.Message}"); }
        }

        private static bool LooksLikeExecutable(string path)
        {
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length < 2)
                    return false;

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return fs.ReadByte() == 0x4D && fs.ReadByte() == 0x5A;
                }
            }
            catch { return false; }
        }

        private static void ReplaceFileAtomic(string source, string destination)
        {
            if (File.Exists(destination))
            {
                // Atomic swap on NTFS with a rollback copy retained until the replacement has succeeded.
                string backup = destination + ".bak";
                File.Replace(source, destination, backup);
                try { File.Delete(backup); } catch { }
            }
            else
            {
                File.Move(source, destination);
            }
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

        private static bool ShouldCreateDesktopShortcut(string globalDirectory, string desktopPath)
        {
            if (System.IO.File.Exists(desktopPath))
                return true;

            string fileName = Path.GetFileName(desktopPath);
            if (System.IO.File.Exists(Path.Combine(globalDirectory, "Shortcuts", fileName)))
                return false;
            if (System.IO.File.Exists(Path.Combine(globalDirectory, "Backup Shortcuts", fileName)))
                return false;

            return true;
        }

        // Generates Windows shortcuts, deleting existing dead shortcuts first to ensure proper VM binding
        public static void CreateShortcut(string name, string path, string target, string workDir)
        {
            // Attempt to remove dead/corrupt shortcut to enforce generation of a new Volume GUID
            for (int i = 0; i < 5; i++)
            {
                try 
                { 
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path); 
                    break; 
                } 
                catch { Thread.Sleep(200); }
            }

            try 
            {
                // Instantiate WScript.Shell without Interop dependencies to prevent COM thread crashes
                Type tWsh = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(tWsh);
                var lnk = shell.CreateShortcut(path);
                lnk.TargetPath = target;
                lnk.WorkingDirectory = workDir;
                lnk.Save();
            }
            catch (Exception ex) { SalsaLogger.Error($"Shortcut creation failed for {name}: {ex.Message}"); }
        }
    }
}
