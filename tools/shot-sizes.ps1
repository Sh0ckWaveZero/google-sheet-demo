# Capture the app at its minimum size to expose overlapping anchored controls.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace Win32 -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr hWnd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr hWnd, int cmd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool MoveWindow(System.IntPtr hWnd, int x, int y, int w, int h, bool repaint);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, System.UIntPtr extra);
'@

$outDir = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check'
$proc = Get-Process GoogleSheetsDemo -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw 'app not running' }
$hwnd = $proc.MainWindowHandle

function Save-Shot([string]$name) {
    Start-Sleep -Milliseconds 600
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save((Join-Path $outDir ("$name.png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}

[Win32.Native]::ShowWindow($hwnd, 9) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
[Win32.Native]::SetForegroundWindow($hwnd) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 400

# normal size
[Win32.Native]::MoveWindow($hwnd, 500, 200, 900, 620, $true) | Out-Null
Save-Shot '61-normal-size'

# minimum size (860x580) - anchors squeeze everything together
[Win32.Native]::MoveWindow($hwnd, 500, 200, 860, 580, $true) | Out-Null
Save-Shot '62-min-size'

# stretched wide - right-anchored controls drift far from left-side labels
[Win32.Native]::MoveWindow($hwnd, 500, 200, 1200, 700, $true) | Out-Null
Save-Shot '63-wide-size'
Write-Output 'DONE'
