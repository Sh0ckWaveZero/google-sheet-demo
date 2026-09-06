# ASCII-only. Find the input box bounds by sampling background colors.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap('C:\lisa\Demo_Spread_Sheet\tools\ui-check\fix-typed.png')

Write-Output 'column x=400, y=48..96:'
for ($y = 48; $y -le 96; $y++) {
    $c = $bmp.GetPixel(400, $y)
    Write-Output ("  y=" + $y + " rgb=" + $c.R + "," + $c.G + "," + $c.B)
}
$bmp.Dispose()
