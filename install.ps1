# One-time setup: build the exe, add it to Startup (run on login), and launch it now.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe  = Join-Path $root 'Screenshotter.exe'

# 1) Build
& (Join-Path $root 'build.ps1')

# 2) Create shortcuts: Startup (run on login) + Desktop (manual launch). Both use app.ico.
$ico   = Join-Path $root 'app.ico'
$shell = New-Object -ComObject WScript.Shell

function New-Shortcut($path) {
    $sc = $shell.CreateShortcut($path)
    $sc.TargetPath       = $exe
    $sc.WorkingDirectory = $root
    $sc.Description       = 'Screenshotter - Win+Shift+S snip to clipboard'
    $sc.WindowStyle      = 7   # minimized
    if (Test-Path $ico) { $sc.IconLocation = "$ico,0" }
    $sc.Save()
}

$lnkStartup = Join-Path ([Environment]::GetFolderPath('Startup')) 'Screenshotter.lnk'
$lnkDesktop = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Screenshotter.lnk'
New-Shortcut $lnkStartup
New-Shortcut $lnkDesktop
[System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell) | Out-Null
Write-Host "Added to Startup: $lnkStartup" -ForegroundColor Green
Write-Host "Added Desktop icon: $lnkDesktop" -ForegroundColor Green

# 3) Launch now (stop any existing instance first)
Get-Process Screenshotter -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300
Start-Process $exe
Write-Host ""
Write-Host "Screenshotter is running. Press Win+Shift+S to capture." -ForegroundColor Cyan
Write-Host "Screenshots auto-save to: $([Environment]::GetFolderPath('MyPictures'))\Screenshots" -ForegroundColor Cyan
