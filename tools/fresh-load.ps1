# ASCII-only. Restart the app (ID auto-fills from App.config), click Load
# until the grid fills (no typing anywhere), capture the result.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class WF {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][WF]::SetProcessDPIAware()

taskkill /IM GoogleSheetsDemo.exe /F 2>$null | Out-Null
Start-Sleep -Milliseconds 1500
Start-Process 'C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\GoogleSheetsDemo.exe'
Start-Sleep -Seconds 5

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle

for ($i = 1; $i -le 4; $i++) {
    $mr = New-Object WF+R
    [void][WF]::GetWindowRect($main, [ref]$mr)
    [void][WF]::SetCursorPos($mr.L + 820, $mr.T + 76)
    Start-Sleep -Milliseconds 400
    [WF]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 130
    [WF]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 8000

    $r = New-Object WF+R
    [void][WF]::GetWindowRect($main, [ref]$r)
    $wd = $r.Rt - $r.L
    $ht = $r.B - $r.T
    $bmp = New-Object System.Drawing.Bitmap($wd, $ht)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
    $g.Dispose()
    $out = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\fresh' + $i + '.png'
    $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output ("SAVED fresh" + $i + ".png")
}
