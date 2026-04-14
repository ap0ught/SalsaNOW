$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

$solutionPath = Join-Path $repoRoot 'SalsaNOW.sln'

Write-Host 'Restoring solution...'
dotnet restore $solutionPath
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet restore failed.'
}

Write-Host 'Building Release...'
dotnet build $solutionPath -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    throw 'dotnet build failed.'
}

$outputExe = Join-Path $repoRoot 'SalsaNOW\bin\Release\net48\SalsaNOW.exe'
if (Test-Path $outputExe) {
    Write-Host "Built EXE: $outputExe"
} else {
    throw 'Build finished but SalsaNOW.exe was not found in expected output folder.'
}
