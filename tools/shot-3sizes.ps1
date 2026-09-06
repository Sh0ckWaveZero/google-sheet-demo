# Capture the app at 3 sizes for overlap inspection (BringToFront + crop).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace U -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr hWnd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr hWnd, int cmd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool MoveWindow(System.IntPtr hWnd, int x, int y, int w, int h, bool repaint);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, System.UIntPtr extra);
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT rect);
'@

$outDir = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check'
$proc = Get-Process GoogleSheetsDemo -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw 'app not running' }
$hwnd = $proc.MainWindowHandle

function Save-AppShot([string]$name) {
    Start-Sleep -Milliseconds 600
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $rect = New-Object U.Native+RECT
    [U.Native]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
    $w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
    if ($w -gt 50 -and $h -gt 50) {
        $src = New-Object System.Drawing.Rectangle($rect.Left, $rect.Top, $w, $h)
        $crop = $bmp.Clone($src, $bmp.PixelFormat)
        $crop.Save((Join-Path $outDir ("$name.png")), [System.Drawing.Imaging.ImageFormat]::Png)
        $crop.Dispose()
    }
    $g.Dispose(); $bmp.Dispose()
}

function Focus-App {
    [U.Native]::ShowWindow($hwnd, 9) | Out-Null
    [U.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
    [U.Native]::SetForegroundWindow($hwnd) | Out-Null
    [U.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
}

Focus-App
[U.Native]::MoveWindow($hwnd, 480, 180, 1000, 660, $true) | Out-Null
Save-AppShot 'A1-normal'

[U.Native]::MoveWindow($hwnd, 480, 180, 916, 580, $true) | Out-Null
Save-AppShot 'A2-minimum'

[U.Native]::MoveWindow($hwnd, 480, 180, 1250, 760, $true) | Out-Null
Save-AppShot 'A3-wide'

[U.Native]::MoveWindow($hwnd, 480, 180, 1000, 660, $true) | Out-Null
Write-Output 'DONE'
