# ASCII-only. Bring the app to the foreground, wait, capture it.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class WK {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][WK]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle

for ($i = 0; $i -lt 3; $i++) {
    [WK]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
    [void][WK]::SetForegroundWindow($main)
    [WK]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 800
    if ([WK]::GetForegroundWindow() -eq $main) { break }
}
Write-Output ("FG=" + ([WK]::GetForegroundWindow() -eq $main))
Start-Sleep -Milliseconds 500

$r = New-Object WK+R
[void][WK]::GetWindowRect($main, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\pager-view.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output 'SAVED pager-view.png'
