# Windows Testing Required

This project compiles on macOS but contains Windows-specific code that **must be tested on Windows**.

## Features Requiring Windows Testing

### 1. WindowsShortcutService
- **Technology**: COM Interop (IWshRuntimeLibrary)
- **Test**: Create a desktop shortcut, verify it works
- **Expected**: `.lnk` file on desktop launches target application

### 2. DepotDownloaderService
- **Technology**: Process execution, Steam integration
- **Test**: Install Elden Ring using Steam credentials
- **Expected**: Game files downloaded to specified directory

### 3. WindowsAPI (P/Invoke)
- **Technology**: Win32 API calls
- **Test**: Find/close windows by title
- **Expected**: Can detect and close running applications

## How to Test on Windows

1. Build on macOS: `dotnet publish -r win-x64 --self-contained`
2. Transfer `bin/Release/net8.0-windows/win-x64/publish/` to Windows machine
3. Run executable on Windows
4. Use test menu options to verify each feature
5. Report results back to development environment

## Known Limitations on macOS

- **COM objects will throw**: `PlatformNotSupportedException`
- **P/Invoke will fail**: No user32.dll on macOS
- **DepotDownloader.exe**: Windows executable won't run

All business logic is testable on macOS. Only platform-specific integration requires Windows.
