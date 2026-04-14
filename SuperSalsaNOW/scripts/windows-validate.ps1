# SuperSalsaNOW - Windows Validation Script
# Run this on Windows to test all Windows-specific features

param(
    [switch]$SkipShortcuts,
    [switch]$SkipDepotDownloader,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "SuperSalsaNOW - Windows Validation" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Test results tracking
$TestResults = @()

function Test-Feature {
    param(
        [string]$Name,
        [scriptblock]$Test
    )

    Write-Host "Testing: $Name..." -ForegroundColor Yellow

    try {
        $result = & $Test
        $TestResults += @{
            Name = $Name
            Status = "PASS"
            Message = $result
        }
        Write-Host "  ✓ PASS: $result" -ForegroundColor Green
    }
    catch {
        $TestResults += @{
            Name = $Name
            Status = "FAIL"
            Message = $_.Exception.Message
        }
        Write-Host "  ✗ FAIL: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
}

# Test 1: .NET Runtime
Test-Feature ".NET 8 Runtime" {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -match "^8\.") {
        return ".NET $dotnetVersion detected"
    }
    throw ".NET 8 not found (version: $dotnetVersion)"
}

# Test 2: SuperSalsaNOW.exe exists
Test-Feature "SuperSalsaNOW.exe" {
    if (Test-Path "SuperSalsaNOW.exe") {
        $fileInfo = Get-Item "SuperSalsaNOW.exe"
        return "Found ($([math]::Round($fileInfo.Length / 1MB, 2)) MB)"
    }
    throw "SuperSalsaNOW.exe not found in current directory"
}

# Test 3: appsettings.json exists
Test-Feature "appsettings.json" {
    if (Test-Path "appsettings.json") {
        $config = Get-Content "appsettings.json" | ConvertFrom-Json
        return "Manifest URL: $($config.Manifest.BaseUrl)"
    }
    throw "appsettings.json not found"
}

# Test 4: COM Shortcut Creation (if not skipped)
if (-not $SkipShortcuts) {
    Test-Feature "COM Shortcut Creation" {
        $testTarget = "C:\Windows\System32\notepad.exe"
        $testShortcut = "$env:TEMP\SuperSalsaNOW-Test.lnk"

        $shell = New-Object -ComObject WScript.Shell
        $shortcut = $shell.CreateShortcut($testShortcut)
        $shortcut.TargetPath = $testTarget
        $shortcut.Save()

        if (Test-Path $testShortcut) {
            Remove-Item $testShortcut -Force
            return "Successfully created and cleaned up test shortcut"
        }
        throw "Failed to create test shortcut"
    }
}

# Test 5: Windows API P/Invoke
Test-Feature "Windows API P/Invoke" {
    # Test FindWindow exists
    Add-Type @"
        using System;
        using System.Runtime.InteropServices;
        public class WinAPI {
            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string className, string windowTitle);
        }
"@

    $result = [WinAPI]::FindWindow("Shell_TrayWnd", $null)
    if ($result -ne [IntPtr]::Zero) {
        return "FindWindow working (found taskbar window)"
    }
    throw "FindWindow failed"
}

# Test 6: Process Execution
Test-Feature "Process Execution" {
    $proc = Start-Process -FilePath "cmd.exe" -ArgumentList "/c echo test" -PassThru -WindowStyle Hidden
    $proc.WaitForExit(5000) | Out-Null

    if ($proc.ExitCode -eq 0) {
        return "Successfully executed test process"
    }
    throw "Process execution failed"
}

# Test 7: DepotDownloader (if not skipped)
if (-not $SkipDepotDownloader) {
    Test-Feature "DepotDownloader Availability" {
        # Check if DepotDownloader.exe exists in Tools directory
        $depotPath = "I:\Tools\DepotDownloader\DepotDownloader.exe"

        if (Test-Path $depotPath) {
            return "Found at $depotPath"
        }

        # Check if it can be downloaded
        $url = "https://github.com/SteamRE/DepotDownloader/releases"
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -Method Head

        if ($response.StatusCode -eq 200) {
            return "Not installed, but download URL is accessible"
        }
        throw "DepotDownloader not found and download URL unreachable"
    }
}

# Summary
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$passCount = ($TestResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($TestResults | Where-Object { $_.Status -eq "FAIL" }).Count

foreach ($result in $TestResults) {
    $color = if ($result.Status -eq "PASS") { "Green" } else { "Red" }
    $symbol = if ($result.Status -eq "PASS") { "✓" } else { "✗" }
    Write-Host "  $symbol $($result.Name): $($result.Status)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Total: $passCount passed, $failCount failed" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($failCount -gt 0) {
    Write-Host "Some tests failed. See details above." -ForegroundColor Red
    exit 1
}
else {
    Write-Host "All tests passed! ✓" -ForegroundColor Green
    exit 0
}
