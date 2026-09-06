# ASCII-only. Foreground the app, TAB to Load, press Enter, wait, capture.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W4 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W4]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$h = $proc.MainWindowHandle

[W4]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[void][W4]::SetForegroundWindow($h)
[W4]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700
Write-Output ("FG=" + ([W4]::GetForegroundWindow() -eq $h))

[System.Windows.Forms.SendKeys]::SendWait("{TAB}")
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")

Start-Sleep -Milliseconds 7000

$r = New-Object W4+R
[void][W4]::GetWindowRect($h, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\loaded.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output ("WINDOW=" + $r.L + "," + $r.T + " " + $wd + "x" + $ht)
Write-Output 'SAVED=loaded.png'
