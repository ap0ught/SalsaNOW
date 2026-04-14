# SuperSalsaNOW - Windows Testing Guide

## Table of Contents

1. [Why Windows Testing is Required](#why-windows-testing-is-required)
2. [Features Requiring Windows Validation](#features-requiring-windows-validation)
3. [Testing Prerequisites](#testing-prerequisites)
4. [Testing Procedure](#testing-procedure)
5. [Validation Checklist](#validation-checklist)
6. [Collecting Logs](#collecting-logs)
7. [Reporting Results](#reporting-results)
8. [Known macOS Limitations](#known-macos-limitations)
9. [Troubleshooting](#troubleshooting)

---

## Why Windows Testing is Required

SuperSalsaNOW is developed on **macOS** but targets **Windows** as the primary runtime platform. Several critical features rely on Windows-specific APIs and tools that cannot be tested on macOS:

| Feature | Technology | Why Windows-Only |
|---------|-----------|------------------|
| **Desktop Shortcuts** | Shell32.dll COM interface | macOS uses .app bundles, not .lnk files |
| **DepotDownloader** | Windows executable | Uses Windows system libraries |
| **Game Installation** | NTFS file system operations | Symlinks, permissions differ from macOS |
| **Elden Ring Execution** | Windows game binary | Game only runs on Windows |

**Development Model:**
- **Build on macOS**: Cross-platform .NET allows building Windows executables from macOS
- **Test on Windows**: Validate Windows-specific features work correctly
- **Iterate**: Fix issues on macOS, rebuild, re-test on Windows

---

## Features Requiring Windows Validation

### 1. Desktop Shortcut Creation

**What to Test:**
- Shortcut appears on Windows desktop
- Shortcut has correct name
- Shortcut target path is correct
- Shortcut working directory is set
- Icon displays (if applicable)
- Double-clicking shortcut launches target application

**Windows API Used:**
- `IShellLink` COM interface (Shell32.dll)
- `IPersistFile` for saving .lnk files

**Why It Can't Be Tested on macOS:**
- macOS doesn't have .lnk files
- Shell32.dll doesn't exist on macOS
- COM interop is Windows-specific

### 2. DepotDownloader Execution

**What to Test:**
- DepotDownloader downloads successfully
- Elden Ring game files download via DepotDownloader
- Steam authentication works (username/password)
- Steam Guard 2FA prompts appear
- Downloaded game files are complete and valid
- `steam_appid.txt` is created correctly

**Tool Used:**
- DepotDownloader (https://github.com/SteamRE/DepotDownloader)
- Windows .NET executable

**Why It Can't Be Tested on macOS:**
- DepotDownloader binary is Windows-specific
- Steam content extraction uses Windows APIs
- File system operations differ (NTFS vs APFS/HFS+)

### 3. Elden Ring Reforged Installation

**What to Test:**
- ERR mod downloads from Nexus Mods
- ZIP extraction completes successfully
- Files are placed in correct directories
- ERR launcher (errgamemodetool.exe) is present
- Configuration files are valid

**Why Full Testing Requires Windows:**
- Verifying launcher executable requires Windows
- File paths may differ (backslashes vs forward slashes)
- Permissions and attributes differ across platforms

### 4. Modded Game Launch

**What to Test:**
- Desktop shortcut launches ERR launcher
- Launcher starts Elden Ring with mods enabled
- Game runs without crashes
- Mods are active (visual confirmation in-game)

**Why It Can't Be Tested on macOS:**
- Elden Ring is Windows-only
- DirectX 12 required (Windows API)
- ERR launcher is Windows executable

---

## Testing Prerequisites

### Windows Machine Requirements

**Minimum:**
- Windows 10 (version 1909 or later) or Windows 11
- 8GB RAM
- 60GB free disk space (for Elden Ring game files)
- .NET 8.0 Runtime (or SDK for development builds)
- Internet connection (for Nexus Mods, Steam downloads)

**Recommended:**
- Windows 11
- 16GB RAM
- SSD with 100GB free space
- .NET 8.0 SDK (for building locally on Windows)

### Required Accounts

1. **Nexus Mods Account** (Free)
   - Sign up: https://www.nexusmods.com/register
   - Generate API Key: https://www.nexusmods.com/users/myaccount?tab=api
   - Note: Premium membership NOT required for testing

2. **Steam Account**
   - Sign up: https://store.steampowered.com/join/
   - **Must own Elden Ring** OR have valid Steam credentials for testing DepotDownloader
   - Note: DepotDownloader can download any game your account owns

### Software Setup

1. **Install .NET 8.0 Runtime**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose "Runtime" (not SDK) for testing
   - Verify:
     ```cmd
     dotnet --version
     ```

2. **Download Test Build**
   - Get latest release from GitHub Releases
   - Or receive `SuperSalsaNOW.exe` from developer
   - Extract to test directory (e.g., `C:\SuperSalsaNOW\`)

3. **Prepare Test Environment**
   - Create test directory: `I:\Games\ELDENRING` (or custom path)
   - Ensure Desktop is accessible (for shortcut creation)
   - Disable antivirus temporarily (may block DepotDownloader)

---

## Testing Procedure

### Phase 1: Configuration

1. **Launch SuperSalsaNOW**
   ```cmd
   cd C:\SuperSalsaNOW
   SuperSalsaNOW.exe
   ```

2. **Select "Configure Nexus API Key"**
   - Paste your Nexus API key
   - Verify confirmation message appears

3. **Verify Configuration File**
   - Open `appsettings.json` in Notepad
   - Confirm `NexusApiKey` is populated
   - Check `Paths` section matches your setup:
     ```json
     {
       "NexusApiKey": "your-key-here",
       "Paths": {
         "InstallRoot": "I:\\",
         "GameDirectory": "Games\\ELDENRING",
         "ModsDirectory": "Mods"
       }
     }
     ```

**Expected Result:** ✅ Configuration saved successfully

---

### Phase 2: Game Installation (Optional)

**Note:** Skip this if you already have Elden Ring installed. This tests DepotDownloader functionality.

1. **Select "Install Elden Ring"**

2. **Enter Steam Credentials**
   - Username: your-steam-username
   - Password: your-steam-password

3. **Handle Steam Guard (if enabled)**
   - Check email/mobile for Steam Guard code
   - Enter code when prompted

4. **Monitor Download Progress**
   - DepotDownloader will output download status
   - Expected time: 30-60 minutes (depending on internet speed)
   - Expected size: ~50GB

5. **Verify Installation**
   - Check `I:\Games\ELDENRING\Game\eldenring.exe` exists
   - Check `I:\Games\ELDENRING\steam_appid.txt` contains `1245620`

**Expected Results:**
- ✅ DepotDownloader runs without errors
- ✅ Game files downloaded completely
- ✅ `eldenring.exe` is present
- ✅ `steam_appid.txt` is created

**Common Issues:**
- **Steam Guard not triggered**: Check email/mobile app
- **Download stalls**: Restart DepotDownloader (run command again)
- **Invalid credentials**: Double-check username/password

---

### Phase 3: Vanilla Game Verification

1. **Select "Verify Vanilla Installation"**

2. **Confirm Detection**
   - Tool should report: "Elden Ring installation found at: I:\Games\ELDENRING"

3. **Check Validation Output**
   - Confirms `eldenring.exe` exists
   - Reports file size and version (if available)

**Expected Result:** ✅ Installation verified successfully

---

### Phase 4: Mod Installation

1. **Select "Install Elden Ring Reforged (ERR)"**

2. **Monitor Nexus Download**
   - Tool queries Nexus API for latest ERR files
   - Displays file name and size
   - Downloads mod archive (usually .zip)

3. **Monitor Extraction**
   - Tool extracts archive to `I:\Mods\elden-ring-reforged\`
   - Progress bar shows extraction status

4. **Verify Installation**
   - Navigate to `I:\Mods\elden-ring-reforged\`
   - Confirm files are present:
     - `errgamemodetool.exe` (ERR launcher)
     - `regulation.bin` (mod data)
     - Config files (`.ini` or `.toml`)

**Expected Results:**
- ✅ Mod downloaded from Nexus
- ✅ Archive extracted successfully
- ✅ ERR launcher executable exists
- ✅ No errors or warnings

**Common Issues:**
- **Nexus API rate limit**: Wait 60 seconds and retry
- **Invalid API key**: Reconfigure in menu
- **Extraction fails**: Check disk space

---

### Phase 5: Desktop Shortcut Creation

1. **Select "Create Desktop Shortcut"**

2. **Confirm Shortcut Appears**
   - Check Windows Desktop
   - Look for shortcut named "Elden Ring Reforged" (or similar)

3. **Inspect Shortcut Properties**
   - Right-click shortcut → Properties
   - **Target**: Should point to `I:\Mods\elden-ring-reforged\errgamemodetool.exe`
   - **Start in**: Should be mod directory
   - **Icon**: May display ERR icon (if configured)

**Expected Results:**
- ✅ Shortcut created on desktop
- ✅ Target path is correct
- ✅ Working directory is set

**Common Issues:**
- **Shortcut not visible**: Refresh desktop (F5)
- **Permission denied**: Run as Administrator
- **Target not found**: Verify mod installation path

---

### Phase 6: Launch Modded Game

1. **Double-Click Desktop Shortcut**

2. **ERR Launcher Should Open**
   - GUI window appears with mod options
   - Launcher detects game installation

3. **Click "Launch Game" (or equivalent)**

4. **Elden Ring Starts**
   - Game loads with ERR modifications
   - Visual changes should be apparent (new UI elements, balance changes, etc.)

5. **Verify Mods Active**
   - Check main menu for ERR indicators
   - Load save game or start new game
   - Confirm mod features work (new enemies, items, etc.)

**Expected Results:**
- ✅ Shortcut launches ERR launcher
- ✅ Launcher detects game
- ✅ Game starts successfully
- ✅ Mods are active and functional

**Common Issues:**
- **Launcher crashes**: Check logs in mod directory
- **Game won't start**: Verify vanilla game works first
- **Mods not active**: Check ERR launcher settings

---

## Validation Checklist

Use this checklist to track testing progress:

### Configuration
- [ ] Nexus API key configures successfully
- [ ] Configuration persists in `appsettings.json`
- [ ] No errors during configuration

### Game Installation (DepotDownloader)
- [ ] DepotDownloader executes without errors
- [ ] Steam authentication succeeds
- [ ] Steam Guard 2FA prompts (if enabled)
- [ ] Game files download completely (~50GB)
- [ ] `eldenring.exe` exists at expected path
- [ ] `steam_appid.txt` is created with correct app ID

### Game Verification
- [ ] Tool detects existing Elden Ring installation
- [ ] Verification confirms game files are valid
- [ ] No false positives or negatives

### Mod Installation
- [ ] Nexus API query succeeds
- [ ] Mod file downloads from Nexus
- [ ] Archive extraction completes
- [ ] ERR launcher executable exists
- [ ] Mod files are in correct directory structure

### Shortcut Creation
- [ ] Shortcut appears on desktop
- [ ] Shortcut name is correct
- [ ] Target path points to ERR launcher
- [ ] Working directory is set correctly
- [ ] Icon displays (if applicable)

### Game Launch
- [ ] Desktop shortcut launches ERR launcher
- [ ] ERR launcher detects game installation
- [ ] Game starts via launcher
- [ ] Mods are active in-game
- [ ] No crashes or errors during gameplay

### Overall
- [ ] All menu options work without crashes
- [ ] Error messages are clear and helpful
- [ ] Progress reporting is accurate
- [ ] Logs are generated correctly

---

## Collecting Logs

### Application Logs

Logs are written to console output. To capture for reporting:

**Method 1: Redirect to File**
```cmd
SuperSalsaNOW.exe > output.log 2>&1
```

**Method 2: Copy from Console**
- Right-click console window → Select All → Copy
- Paste into text file

### DepotDownloader Logs

DepotDownloader outputs to console during execution. SuperSalsaNOW captures this output.

**Location:** Console output when "Install Elden Ring" is selected

### ERR Launcher Logs

**Location:** `I:\Mods\elden-ring-reforged\logs\` (if ERR creates logs)

### Windows Event Viewer

If application crashes:
1. Open Event Viewer (Win + X → Event Viewer)
2. Navigate to: Windows Logs → Application
3. Look for errors from ".NET Runtime" or "SuperSalsaNOW"

---

## Reporting Results

### Success Report Template

```
## Test Report: SuperSalsaNOW v1.0.0

**Date:** 2025-11-09
**Tester:** [Your Name]
**Windows Version:** Windows 11 Pro (22H2)
**.NET Version:** 8.0.300

### Results

✅ Configuration: Passed
✅ Game Installation: Passed (DepotDownloader worked flawlessly)
✅ Game Verification: Passed
✅ Mod Installation: Passed (ERR downloaded and extracted)
✅ Shortcut Creation: Passed
✅ Game Launch: Passed (mods active, no crashes)

### Notes

- DepotDownloader took ~45 minutes for full game download
- Steam Guard 2FA worked correctly
- Desktop shortcut icon did not display (minor cosmetic issue)
- Gameplay tested for 30 minutes, no issues

### Recommendation

Ready for release. Only issue is missing shortcut icon (low priority).
```

### Failure Report Template

```
## Test Report: SuperSalsaNOW v1.0.0

**Date:** 2025-11-09
**Tester:** [Your Name]
**Windows Version:** Windows 10 Home (21H2)
**.NET Version:** 8.0.300

### Results

✅ Configuration: Passed
❌ Game Installation: Failed
⚠️  Game Verification: Skipped (no game installed)
⚠️  Mod Installation: Skipped
⚠️  Shortcut Creation: Skipped
⚠️  Game Launch: Skipped

### Failure Details

**Phase:** Game Installation (DepotDownloader)
**Error:** DepotDownloader.exe crashes immediately with "Access Denied"
**Logs:**
```
System.UnauthorizedAccessException: Access to the path 'I:\Games\ELDENRING' is denied.
   at System.IO.FileStream..ctor(String path, FileMode mode, FileAccess access)
```

**Steps to Reproduce:**
1. Launch SuperSalsaNOW
2. Select "Install Elden Ring"
3. Enter Steam credentials
4. DepotDownloader crashes before download starts

**Environment:**
- Antivirus: Windows Defender (enabled)
- User Account: Standard user (not Administrator)

### Recommendation

Investigate permission issues. May need to run as Administrator or disable antivirus temporarily.
```

### Submitting Reports

**Option 1: GitHub Issues**
- Go to: https://github.com/psingley/SuperSalsaNOW/issues
- Click "New Issue"
- Paste report using template
- Attach logs as `.txt` files

**Option 2: Email**
- Send to: [developer email]
- Subject: "SuperSalsaNOW Test Report - [Pass/Fail]"
- Attach logs and screenshots

**Option 3: Discord/Slack**
- Post in testing channel
- Include summary and link to detailed report

---

## Known macOS Limitations

These features **will not work** on macOS during development:

### 1. Desktop Shortcuts
- **Error:** `PlatformNotSupportedException`
- **Why:** Shell32.dll doesn't exist on macOS
- **Workaround:** Test on Windows only

### 2. DepotDownloader
- **Error:** Executable format error
- **Why:** DepotDownloader.exe is Windows binary
- **Workaround:** Manually download DepotDownloader for macOS (if available), or test on Windows

### 3. Elden Ring Launch
- **Error:** Game binary not compatible
- **Why:** Elden Ring is Windows-only
- **Workaround:** Cannot be tested on macOS

### What DOES Work on macOS

✅ Building the solution
✅ Running unit tests
✅ Testing Nexus API integration
✅ Testing manifest loading
✅ Testing download logic (files download, but can't run .exe)
✅ CLI menu navigation
✅ Configuration management

---

## Troubleshooting

### Issue: DepotDownloader Access Denied

**Symptoms:**
- DepotDownloader crashes with "Access Denied"
- Files not downloading

**Solutions:**
1. Run SuperSalsaNOW as Administrator (right-click → Run as Administrator)
2. Disable Windows Defender temporarily
3. Check folder permissions (ensure `I:\` is writable)

### Issue: Nexus API Returns 429 (Rate Limited)

**Symptoms:**
- Mod download fails with "Too Many Requests"

**Solutions:**
1. Wait 60 seconds and retry
2. Check Nexus API key is valid (regenerate if needed)
3. Reduce test frequency

### Issue: Shortcut Target Not Found

**Symptoms:**
- Shortcut created but clicking shows "Target not found"

**Solutions:**
1. Verify mod installation path
2. Check ERR launcher executable exists
3. Update `appsettings.json` with correct paths

### Issue: Game Crashes on Launch

**Symptoms:**
- Elden Ring starts but crashes immediately

**Solutions:**
1. Verify vanilla game works (test without mods)
2. Check ERR version compatibility with game version
3. Delete `elden-ring-reforged` folder and reinstall mod
4. Check ERR launcher logs for errors

### Issue: .NET Runtime Not Found

**Symptoms:**
- Double-clicking `SuperSalsaNOW.exe` shows ".NET runtime not found"

**Solutions:**
1. Install .NET 8.0 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
2. Choose "Desktop Runtime" (includes all components)
3. Restart terminal/command prompt

---

## PowerShell Testing Script (Phase 8)

**Future Enhancement:** Automated testing script for Windows.

**Planned Features:**
- Automated validation of all test phases
- Report generation (JSON/HTML)
- Screenshot capture on failures
- Log collection and archival

**Planned Location:** `scripts/Test-SuperSalsaNOW.ps1`

**Usage (Future):**
```powershell
.\scripts\Test-SuperSalsaNOW.ps1 -Verbose
```

**Output:**
```
SuperSalsaNOW Automated Test Suite
===================================

[✓] Phase 1: Configuration
[✓] Phase 2: Game Installation
[✓] Phase 3: Verification
[✓] Phase 4: Mod Installation
[✓] Phase 5: Shortcut Creation
[✓] Phase 6: Game Launch

Summary: 6/6 tests passed
Report: test-report-2025-11-09.html
```

---

## Contact for Testing Support

**Questions or Issues During Testing?**

- **GitHub Issues**: https://github.com/psingley/SuperSalsaNOW/issues
- **Discussions**: https://github.com/psingley/SuperSalsaNOW/discussions
- **Email**: [developer email]

**What to Include:**
- Windows version
- .NET version (`dotnet --version`)
- Error messages (exact text)
- Logs (console output)
- Steps to reproduce

---

*Last Updated: 2025-11-09*
