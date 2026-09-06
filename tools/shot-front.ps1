# Bring the app to the front and capture it (full + zoomed regions).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace Win32 -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr hWnd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr hWnd, int cmd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, System.UIntPtr extra);
'@

$outDir = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check'
$proc = Get-Process GoogleSheetsDemo -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw 'app not running' }
$hwnd = $proc.MainWindowHandle

[Win32.Native]::ShowWindow($hwnd, 9) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
[Win32.Native]::SetForegroundWindow($hwnd) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 700

$b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
$bmp.Save((Join-Path $outDir '95-baseline-aligned.png'), [System.Drawing.Imaging.ImageFormat]::Png)

# crop just the app window area for a close look
Add-Type -Namespace W -Name Native2 -MemberDefinition @'
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT rect);
'@
$rect = New-Object W.Native2+RECT
[W.Native2]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
$w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
if ($w -gt 0 -and $h -gt 0) {
    $srcRect = New-Object System.Drawing.Rectangle($rect.Left, $rect.Top, $w, $h)
    $crop = $bmp.Clone($srcRect, $bmp.PixelFormat)
    $crop.Save((Join-Path $outDir '96-baseline-crop.png'), [System.Drawing.Imaging.ImageFormat]::Png)
    $crop.Dispose()
    Write-Output ("CROP: " + $w + "x" + $h)
}
$g.Dispose(); $bmp.Dispose()
Write-Output 'DONE'
