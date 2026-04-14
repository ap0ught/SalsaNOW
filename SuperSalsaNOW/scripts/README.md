# Build and Test Scripts

## macOS Scripts

### build-for-windows.sh
Builds SuperSalsaNOW for Windows deployment from macOS.

**Usage:**
```bash
./scripts/build-for-windows.sh
```

**Output:** `dist/win-x64/SuperSalsaNOW.exe`

### test-macos.sh
Runs cross-platform unit tests on macOS.

**Usage:**
```bash
./scripts/test-macos.sh
```

## Windows Scripts

### windows-validate.ps1
Comprehensive validation of Windows-specific features.

**Usage:**
```powershell
# Run all tests
.\scripts\windows-validate.ps1

# Skip shortcut tests
.\scripts\windows-validate.ps1 -SkipShortcuts

# Skip DepotDownloader check
.\scripts\windows-validate.ps1 -SkipDepotDownloader

# Verbose output
.\scripts\windows-validate.ps1 -Verbose
```

**Tests:**
- .NET 8 Runtime detection
- SuperSalsaNOW.exe existence
- appsettings.json configuration
- COM shortcut creation
- Windows API P/Invoke
- Process execution
- DepotDownloader availability

## Workflow

### Development on macOS
1. Make code changes
2. Run `./scripts/test-macos.sh` (cross-platform tests)
3. Run `./scripts/build-for-windows.sh`
4. Transfer `dist/win-x64/` to Windows machine

### Testing on Windows
1. Copy `dist/win-x64/` directory from macOS
2. Run `.\scripts\windows-validate.ps1`
3. If tests pass, run `SuperSalsaNOW.exe` for manual testing
4. Report results back to development environment
