# ASCII-only. Single-instance launch + Load with handle polling + capture.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class WG {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][WG]::SetProcessDPIAware()

# kill every instance, then start exactly one
Get-Process -Name GoogleSheetsDemo -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 2000
Start-Process 'C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\GoogleSheetsDemo.exe'

# poll for a real window handle
$main = [IntPtr]::Zero
for ($i = 0; $i -lt 20; $i++) {
    Start-Sleep -Milliseconds 800
    $proc = Get-Process -Name GoogleSheetsDemo -ErrorAction SilentlyContinue
    if ($proc -ne $null -and $proc.MainWindowHandle -ne 0) {
        $main = $proc.MainWindowHandle
        break
    }
}
if ($main -eq [IntPtr]::Zero) { Write-Output 'NO_WINDOW'; exit 1 }

$mr = New-Object WG+R
[void][WG]::GetWindowRect($main, [ref]$mr)
Write-Output ("HWND=" + $main + " RECT=" + ($mr.Rt - $mr.L) + "x" + ($mr.B - $mr.T))

# click Load up to 3 times, waiting for the sheet read each time
for ($i = 1; $i -le 3; $i++) {
    [void][WG]::SetCursorPos($mr.L + 820, $mr.T + 76)
    Start-Sleep -Milliseconds 400
    [WG]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 130
    [WG]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 8000
}

$r = New-Object WG+R
[void][WG]::GetWindowRect($main, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e-loaded.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output 'SAVED e2e-loaded.png'
