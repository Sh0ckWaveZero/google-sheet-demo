# ASCII-only. Crop zoom views from the captured window screenshot.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$src = [System.Drawing.Image]::FromFile('C:\lisa\Demo_Spread_Sheet\tools\ui-check\fix-center.png')

function Crop([int]$x, [int]$y, [int]$w, [int]$h, [string]$out) {
    $b = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($b)
    $g.DrawImage($src, (New-Object System.Drawing.Rectangle(0, 0, $w, $h)), (New-Object System.Drawing.Rectangle($x, $y, $w, $h)), [System.Drawing.GraphicsUnit]::Pixel)
    $g.Dispose()
    $b.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
    $b.Dispose()
}

Crop 0 50 780 45 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\zoom-id.png'
Crop 20 480 700 50 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\zoom-name.png'
Crop 10 390 640 45 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\zoom-pager.png'
$src.Dispose()
Write-Output 'OK'
