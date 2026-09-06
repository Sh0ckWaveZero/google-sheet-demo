# ASCII-only. Measure text pixel rows inside the ID input box region.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap('C:\lisa\Demo_Spread_Sheet\tools\ui-check\fix-typed.png')

# box region: window capture starts at window origin; ID box is approx y=58..88, x=24..720
# scan for bright text pixels (light gray/white) inside the box
$rows = @{}
for ($y = 52; $y -lt 95; $y++) {
    $count = 0
    for ($x = 30; $x -lt 350; $x++) {
        $c = $bmp.GetPixel($x, $y)
        if ($c.R -gt 140 -and $c.G -gt 140 -and $c.B -gt 140) { $count++ }
    }
    if ($count -gt 0) { $rows[$y] = $count }
}
$bmp.Dispose()

Write-Output 'bright-pixel rows (y: count):'
$keys = $rows.Keys | Sort-Object
foreach ($k in $keys) { Write-Output ("  y=" + $k + " n=" + $rows[$k]) }
if ($keys.Count -gt 0) {
    $first = $keys[0]
    $last = $keys[$keys.Count - 1]
    Write-Output ("text extent: y " + $first + ".." + $last + " height=" + ($last - $first + 1))
}
