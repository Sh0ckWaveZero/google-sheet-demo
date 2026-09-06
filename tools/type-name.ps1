# ASCII-only. Click into the Name field, type text, capture the window.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W2 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W2]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$h = $proc.MainWindowHandle
if ($h -eq [IntPtr]::Zero) { Write-Output 'NO_WINDOW'; exit 1 }

[W2]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[void][W2]::SetForegroundWindow($h)
[W2]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 600

$r = New-Object W2+R
[void][W2]::GetWindowRect($h, [ref]$r)
Write-Output ("RECT=" + $r.L + "," + $r.T + " " + ($r.Rt - $r.L) + "x" + ($r.B - $r.T))

# Name field sits at form-relative (62,45) inside the group box at (16,442);
# window chrome offset is about 8px left / 31px top at this size.
$cx = $r.L + 8 + 62 + 115
$cy = $r.T + 31 + 442 + 62
Write-Output ("CLICK=" + $cx + "," + $cy)

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class M {
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
}
"@

[void][M]::SetCursorPos($cx, $cy)
Start-Sleep -Milliseconds 250
[M]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
[M]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 400

[System.Windows.Forms.SendKeys]::SendWait("Longan juice")
Start-Sleep -Milliseconds 500

$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$out = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\fix-typed.png'
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output ("SAVED=" + $out)
