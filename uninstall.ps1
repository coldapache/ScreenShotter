# Clean removal: stop the app and remove its Startup shortcut.
# (The exe stays on disk so you can re-run install.ps1 later; delete the folder to remove fully.)

$ErrorActionPreference = 'SilentlyContinue'

Get-Process Screenshotter -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 200

foreach ($lnk in @(
        (Join-Path ([Environment]::GetFolderPath('Startup')) 'Screenshotter.lnk'),
        (Join-Path ([Environment]::GetFolderPath('Desktop')) 'Screenshotter.lnk'))) {
    if (Test-Path $lnk) {
        Remove-Item $lnk -Force
        Write-Host "Removed shortcut: $lnk" -ForegroundColor Green
    }
}

Write-Host "Screenshotter stopped. Win+Shift+S now returns to the Windows built-in snip." -ForegroundColor Cyan
