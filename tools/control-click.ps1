# ASCII-only. Control test: click a plain Name cell in the first row and
# check whether the grid selection moves (clicks reaching the app at all).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W7 {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W7]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle
$mr = New-Object W7+R
[void][W7]::GetWindowRect($main, [ref]$mr)

for ($i = 1; $i -le 3; $i++) {
    $cx = $mr.L + 250
    $cy = $mr.T + 187
    [void][W7]::SetCursorPos($cx, $cy)
    Start-Sleep -Milliseconds 350
    [W7]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 120
    [W7]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 900
}

$r = New-Object W7+R
[void][W7]::GetWindowRect($main, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\control-click.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output 'SAVED control-click.png'
