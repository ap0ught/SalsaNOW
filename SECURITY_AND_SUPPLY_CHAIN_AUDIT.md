# Security, supply chain, and external dependency audit

This document records findings from a static review of the SalsaNOW repository (April 2026). It covers **security-relevant behavior**, **outdated or legacy technology choices**, and **every path that retrieves bytes from outside this repository** (runtime, build scripts, and documentation).

---

## Critical: TLS server certificate validation is disabled

In `Program.Main`, before any network activity, the code registers a callback that **accepts all TLS certificates regardless of validity**:

```30:31:SalsaNOW/Program.cs
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
```

**Impact:** Any HTTPS download performed through `WebClient` / `ServicePointManager` can be intercepted or replaced by an active network attacker (MITM). That attacker can serve malicious JSON, executables, zip archives, or configuration in place of legitimate `https://salsanowfiles.work` (or other) content.

**Recommendation:** Remove this callback entirely and rely on default certificate validation. If custom validation is required (e.g., pinning), implement explicit pinning for specific hosts instead of returning `true` for all connections.

---

## High: Remote-controlled download and execute (supply chain)

The application **does not pin, hash, or sign** remote manifests or payloads. URLs and install instructions come from JSON hosted on **[https://salsanowfiles.work](https://salsanowfiles.work)** (and Bing for wallpaper). JSON fields such as `url` are deserialized into models and passed directly to `DownloadFileTaskAsync` and then to `Process.Start` or `ZipFile.ExtractToDirectory`.

**Relevant model fields:**

```17:43:SalsaNOW/Models/SalsaModels.cs
    public class Apps
    {
        public string name { get; set; }
        public string fileExtension { get; set; }
        public string exeName { get; set; }
        public string run { get; set; }
        public string url { get; set; }
    }
    // ... SilentApps, DesktopInfo similarly carry url / zipConfig ...
```

**Impact:**

- Compromise of the **salsanowfiles.work** infrastructure (or successful MITM combined with the TLS bypass above) can lead to **arbitrary download and execution** on the machine.
- A local attacker who can supply `--apps-json` / `-a` can merge additional apps from a **local JSON file** with the remote list (`AppInstaller.AppsInstallAsync`), extending the attack surface to malicious local manifests.

**Recommendation:** Use signed manifests, content hashes verified after download, or least-privilege installers; avoid executing binaries immediately after download without integrity checks.

---

## High: Downloaded executable saved under a misleading name and run

`SteamManager.ShutdownServerAsync` downloads `https://salsanowfiles.work/USG/bleh.exe` to a path ending in `**conhost.exe`**, then starts that process:

```24:51:SalsaNOW/SteamManager.cs
                string dummyJson = Path.Combine(globalDirectory, "kaka.json");
                string usgMask = Path.Combine(globalDirectory, "conhost.exe");
                // ...
                    await wc.DownloadFileTaskAsync(new Uri("https://salsanowfiles.work/USG/bleh.exe"), usgMask);
                var usg = Process.Start(usgMask);
```

**Impact:** Regardless of intent, this pattern matches **disguised payload** behavior (wrong filename on disk vs. real binary). Security tools and reviewers will flag it; it also makes incident response harder.

**Recommendation:** Use the real filename (or a neutral tool-specific name), verify integrity, and document why a separate binary is required.

---

## Medium: Zip extraction from remote URLs (ZipSlip and trust)

Several code paths download a `.zip` and call `ZipFile.ExtractToDirectory` without visible path sanitization (e.g. `AppInstaller` for apps, silent apps, desktop shells, Seelen config).

**Impact:** If an attacker controls zip content (via compromised URL or MITM), **directory traversal inside archives** can be a classic **ZipSlip** risk unless the framework API guarantees full extraction path checks for this target framework.

**Recommendation:** Extract only after hash/signature verification; validate each `ZipArchiveEntry.FullName` resolves under the intended destination (defense in depth).

---

## Medium / policy: Anti-cheat and Steam-related behavior

These are not “CVE-style” bugs in the repo, but they are **high-risk and policy-sensitive** behaviors that affect security posture and compliance:


| Area            | Behavior                                                                                                                                          | Location                                    |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| Easy Anti-Cheat | Polls and **kills** EAC-related processes                                                                                                         | `BackgroundTasks.StartEacWatcherAsync`      |
| Steam           | POST to local proxy `http://127.10.0.231:9753/shutdown`, replaces lockdown server arguments, **deletes** `Steam\appcache`, runs downloaded helper | `SteamManager.ShutdownServerAsync`          |
| Steam userdata  | Watches `localconfig.vdf` and **deletes** the file if it contains `"LaunchOptions"`, then kills Steam                                             | `BackgroundTasks.StartBrickPreventionAsync` |


**Impact:** Violates game/anti-cheat and platform terms in many contexts; may be treated as evasion tooling.

---

## Deprecated API: `WebClient`

All HTTP(S) I/O uses `**System.Net.WebClient`**, which is **obsolete** as of .NET 5+ and discouraged even on .NET Framework in favor of `HttpClient` with proper configuration.

**Impact:** Harder to maintain secure defaults (e.g. timeouts, connection pooling, modern TLS usage); `WebClient` is sync-oriented and easier to misuse under load.

---

## Outdated stack and NuGet packages (repository state)


| Item                            | Version / note                                                                                                                                     |
| ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| Target framework                | **.NET Framework 4.8** (`TargetFrameworkVersion` in `SalsaNOW.csproj`) — still supported by Microsoft but not the modern **.NET (Core)** LTS stack |
| Newtonsoft.Json                 | **13.0.4** (`packages.config`)                                                                                                                     |
| Fody                            | **6.9.3**                                                                                                                                          |
| Costura.Fody                    | **6.0.0**                                                                                                                                          |
| NvAPIWrapper.Net                | **0.8.1.101**                                                                                                                                      |
| System.IO.Compression / ZipFile | **4.3.0** (NuGet meta-packages over BCL on net48)                                                                                                  |


**Note:** This solution uses **packages.config** and a `**packages`** directory layout. Automated vulnerability scanning is easiest after **SDK-style** projects or by running `**nuget.exe audit`** / IDE package audit against the lockfile or restore graph. Re-run those tools after any package upgrades.

**Other project metadata:** `SalsaNOW.csproj` references code signing material (`ManifestCertificateThumbprint`, `SalsaNOW_TemporaryKey.pfx`). Ensure `**.pfx` keys are never committed** (not observed in-repo by filename search; verify with `.gitignore` and git history).

---

## Inventory: everything that downloads from outside this repo

### Runtime (application code)


| Source                                                             | Purpose                                                                                 |
| ------------------------------------------------------------------ | --------------------------------------------------------------------------------------- |
| `https://salsanowfiles.work/jsons/directory.json`                  | Resolve `globalDirectory`                                                               |
| `https://salsanowfiles.work/jsons/SalsaNOWConfig.ini`              | Default config if missing                                                               |
| `https://salsanowfiles.work/jsons/apps.json`                       | App list + URLs                                                                         |
| `https://salsanowfiles.work/jsons/silentapps.json`                 | Silent app list + URLs                                                                  |
| `https://salsanowfiles.work/jsons/desktop.json`                    | Desktop/shell list + URLs                                                               |
| URLs in JSON (`app.url`, `desktop.url`, `desktop.zipConfig`, etc.) | Arbitrary download targets chosen by manifest                                           |
| `https://salsanowfiles.work/jsons/kaka.json`                       | Steam lockdown dummy JSON                                                               |
| `https://salsanowfiles.work/USG/bleh.exe`                          | Downloaded binary (saved as `conhost.exe` under global dir)                             |
| `https://salsanowfiles.work/jsons/GameSavesPaths.json`             | Save path junction config                                                               |
| `http://127.10.0.231:9753/shutdown`                                | Local GFN/Steam proxy shutdown (not Internet; cleartext HTTP to loopback-range address) |
| `https://www.bing.com/HPImageArchive.aspx?...`                     | Bing Photo of the Day JSON                                                              |
| `https://www.bing.com{urlbase}_UHD.jpg`                            | Wallpaper image                                                                         |
| `https://paste.rs/`                                                | POST crash logs (plain text body)                                                       |


### Build / tooling (repository script)


| Source                                                        | Purpose                                                                  |
| ------------------------------------------------------------- | ------------------------------------------------------------------------ |
| `https://dist.nuget.org/win-x86-commandline/latest/nuget.exe` | Download `nuget.exe` into `.tools` when missing (`build-single-exe.ps1`) |


### Documentation only (README; not executed by app)


| Source                                                      | Purpose                                                                  |
| ----------------------------------------------------------- | ------------------------------------------------------------------------ |
| `https://salsanowfiles.work/RepoImages/SalsaNOW_Banner.png` | Banner image in README                                                   |
| `https://salsanowfiles.work/SalsaNOW/SalsaNOWUpdater.exe`   | Linked **external updater** binary (supply chain: users trust this host) |
| `https://documentation.salsanowfiles.work`                  | Documentation site                                                       |
| `https://discord.com/invite/ZQqhh4uSU2`                     | Discord invite                                                           |


### Third-party packages (NuGet restore, not vendored in git by default)

Building restores **Costura.Fody**, **Fody**, **Newtonsoft.Json**, **NvAPIWrapper.Net**, **System.IO.Compression** packages from **nuget.org** (or configured feeds). Those packages are **not** source in this repo but are **downloaded during restore**.

---

## Privacy: crash log upload

`SalsaLogger.UploadLogAndShowError` POSTs the full log and error text to **[https://paste.rs/](https://paste.rs/)** and shows the returned URL in a message box. That can include **paths, timings, and other session context**.

---

## Summary of prioritized recommendations

1. **Remove** global `ServerCertificateValidationCallback` that returns `true`.
2. Add **integrity** (signatures or hashes) for remote JSON and binaries; pin or harden trust for `salsanowfiles.work`.
3. Replace `**WebClient`** with `**HttpClient**` and explicit timeouts.
4. Sanitize **zip extraction** paths; assume zips are hostile if URLs are not fully trusted.
5. Rename and document `**bleh.exe` → `conhost.exe`** flow; verify hash before execute.
6. Run `**nuget audit**` / IDE security analysis on restored packages; consider migrating to **SDK-style** projects for better tooling.
7. Clearly disclose **data sent to paste.rs** and **remote install** behavior in user-facing documentation.

---

*This audit is based on static analysis of the repository as checked out; it does not assert absence of issues in binaries built elsewhere or in third-party services.*