# Generates app.ico — a blue rounded tile with white "selection corner" brackets
# (the universal snip/crop symbol). Produces a multi-size PNG-based .ico (16/32/48/256).

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$out  = Join-Path $root 'app.ico'

function New-FramePng([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap($s, $s, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode     = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.Clear([System.Drawing.Color]::Transparent)

    # Rounded tile with a blue gradient.
    $m = [double]$s * 0.07
    $rectF = New-Object System.Drawing.RectangleF($m, $m, ($s - 2*$m), ($s - 2*$m))
    $r = [double]$s * 0.20
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = [float](2*$r)
    $path.AddArc([float]$rectF.Left,              [float]$rectF.Top,                 $d, $d, 180, 90)
    $path.AddArc([float]($rectF.Right - 2*$r),    [float]$rectF.Top,                 $d, $d, 270, 90)
    $path.AddArc([float]($rectF.Right - 2*$r),    [float]($rectF.Bottom - 2*$r),     $d, $d,   0, 90)
    $path.AddArc([float]$rectF.Left,              [float]($rectF.Bottom - 2*$r),     $d, $d,  90, 90)
    $path.CloseFigure()

    $c1 = [System.Drawing.Color]::FromArgb(255, 0, 120, 215)
    $c2 = [System.Drawing.Color]::FromArgb(255, 0, 184, 255)
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush($rectF, $c1, $c2, 55.0)
    $g.FillPath($brush, $path)

    # White corner brackets (the "selection" look).
    $inset = [double]$s * 0.26
    $len   = [double]$s * 0.20
    $thick = [float]([Math]::Max(1.5, $s * 0.075))
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, $thick)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap   = [System.Drawing.Drawing2D.LineCap]::Round
    $lo = $inset
    $hi = $s - $inset
    # top-left
    $g.DrawLine($pen, [float]$lo, [float]$lo, [float]($lo+$len), [float]$lo)
    $g.DrawLine($pen, [float]$lo, [float]$lo, [float]$lo, [float]($lo+$len))
    # top-right
    $g.DrawLine($pen, [float]$hi, [float]$lo, [float]($hi-$len), [float]$lo)
    $g.DrawLine($pen, [float]$hi, [float]$lo, [float]$hi, [float]($lo+$len))
    # bottom-left
    $g.DrawLine($pen, [float]$lo, [float]$hi, [float]($lo+$len), [float]$hi)
    $g.DrawLine($pen, [float]$lo, [float]$hi, [float]$lo, [float]($hi-$len))
    # bottom-right
    $g.DrawLine($pen, [float]$hi, [float]$hi, [float]($hi-$len), [float]$hi)
    $g.DrawLine($pen, [float]$hi, [float]$hi, [float]$hi, [float]($hi-$len))

    # Center dot (skip on the tiny 16px frame to avoid clutter).
    if ($s -ge 32) {
        $dot = [double]$s * 0.085
        $cx = ($s/2.0) - $dot; $cy = ($s/2.0) - $dot
        $wb = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $g.FillEllipse($wb, [float]$cx, [float]$cy, [float](2*$dot), [float](2*$dot))
        $wb.Dispose()
    }

    $pen.Dispose(); $brush.Dispose(); $path.Dispose(); $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    return ,$ms.ToArray()
}

$sizes = @(16, 32, 48, 256)
$pngs  = @{}
foreach ($s in $sizes) { $pngs[$s] = New-FramePng $s }

$fs = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($fs)
# ICONDIR
$bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]$sizes.Count)
$offset = 6 + (16 * $sizes.Count)
foreach ($s in $sizes) {
    $data = $pngs[$s]
    $dim = if ($s -ge 256) { 0 } else { $s }
    $bw.Write([byte]$dim); $bw.Write([byte]$dim)   # width, height (0 == 256)
    $bw.Write([byte]0); $bw.Write([byte]0)         # colors, reserved
    $bw.Write([UInt16]1); $bw.Write([UInt16]32)    # planes, bitcount
    $bw.Write([UInt32]$data.Length)                # bytes in resource
    $bw.Write([UInt32]$offset)                      # image offset
    $offset += $data.Length
}
foreach ($s in $sizes) { $bw.Write($pngs[$s]) }
$bw.Flush()
[System.IO.File]::WriteAllBytes($out, $fs.ToArray())
$bw.Dispose(); $fs.Dispose()
Write-Host "Wrote $out ($([Math]::Round((Get-Item $out).Length/1KB,1)) KB)" -ForegroundColor Green
