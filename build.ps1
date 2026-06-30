# Builds Screenshotter.exe from Screenshotter.cs using the built-in .NET Framework
# C# compiler (csc.exe). No SDK or downloads required.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src  = Join-Path $root 'Screenshotter.cs'
$out  = Join-Path $root 'Screenshotter.exe'

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'  # 32-bit fallback
}
if (-not (Test-Path $csc)) {
    throw "Could not find csc.exe (.NET Framework 4.x). Looked in Framework64 and Framework."
}

# Stop a running instance so the exe isn't locked during rebuild.
Get-Process Screenshotter -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

$args = @(
    '/nologo'
    '/target:winexe'                 # GUI app: no console window
    "/out:$out"
    '/optimize+'
    '/reference:System.dll'
    '/reference:System.Drawing.dll'
    '/reference:System.Windows.Forms.dll'
    '/reference:Microsoft.CSharp.dll'  # needed for dynamic (WScript.Shell COM)
    $src
)

$icon = Join-Path $root 'app.ico'
if (-not (Test-Path $icon)) {
    $mk = Join-Path $root 'make-icon.ps1'
    if (Test-Path $mk) { & $mk }
}
if (Test-Path $icon) { $args = @("/win32icon:$icon") + $args }

Write-Host "Compiling Screenshotter.exe ..." -ForegroundColor Cyan
& $csc @args

if ($LASTEXITCODE -ne 0) { throw "Compilation failed (csc exit $LASTEXITCODE)." }
Write-Host "Built: $out" -ForegroundColor Green
