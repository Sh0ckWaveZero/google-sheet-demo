# ASCII-only. Measure the ID input: box bounds from background color at x=400,
# text rows from bright pixels in x=25..350.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap('C:\lisa\Demo_Spread_Sheet\tools\ui-check\final.png')

# find the box: rows at x=400 whose color differs from the form background
$formBg = $bmp.GetPixel(400, 45)
$boxTop = -1
$boxBottom = -1
for ($y = 40; $y -lt 110; $y++) {
    $c = $bmp.GetPixel(400, $y)
    $same = [Math]::Abs($c.R - $formBg.R) -lt 6 -and [Math]::Abs($c.G - $formBg.G) -lt 6 -and [Math]::Abs($c.B - $formBg.B) -lt 6
    if (-not $same) {
        if ($boxTop -lt 0) { $boxTop = $y }
        $boxBottom = $y
    }
}

$textTop = -1
$textBottom = -1
for ($y = $boxTop; $y -le $boxBottom; $y++) {
    $count = 0
    for ($x = 25; $x -lt 350; $x++) {
        $c = $bmp.GetPixel($x, $y)
        if ($c.R -gt 150 -and $c.G -gt 150 -and $c.B -gt 150) { $count++ }
    }
    if ($count -gt 0) {
        if ($textTop -lt 0) { $textTop = $y }
        $textBottom = $y
    }
}
$bmp.Dispose()

Write-Output ("formBg rgb=" + $formBg.R + "," + $formBg.G + "," + $formBg.B)
Write-Output ("box rows: y " + $boxTop + ".." + $boxBottom + " (height " + ($boxBottom - $boxTop + 1) + ")")
Write-Output ("text rows: y " + $textTop + ".." + $textBottom)
$gapTop = $textTop - $boxTop
$gapBottom = $boxBottom - $textBottom
Write-Output ("gap above text = " + $gapTop + "px, gap below text = " + $gapBottom + "px")
if ([Math]::Abs($gapTop - $gapBottom) -le 2) { Write-Output 'RESULT=CENTERED' } else { Write-Output 'RESULT=OFF_CENTER' }
