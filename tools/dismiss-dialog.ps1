# ASCII-only. Dismiss the open confirm dialog with Alt+N (= No), verify it closed.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W9 {
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W9]::SetProcessDPIAware()

$dlg = [W9]::FindWindow('#32770', 'Confirm delete')
if ($dlg -ne [IntPtr]::Zero) {
    Write-Output 'DIALOG_OPEN=YES - sending Alt+N'
    [W9]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)   # Alt down
    [W9]::keybd_event(0x4E, 0, 0, [UIntPtr]::Zero)   # N down
    [W9]::keybd_event(0x4E, 0, 2, [UIntPtr]::Zero)   # N up
    [W9]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)   # Alt up
    Start-Sleep -Milliseconds 1000
} else {
    Write-Output 'DIALOG_OPEN=NO'
}

$dlg = [W9]::FindWindow('#32770', 'Confirm delete')
Write-Output ("STILL_OPEN=" + ($dlg -ne [IntPtr]::Zero))

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle
$r = New-Object W9+R
[void][W9]::GetWindowRect($main, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\after-cancel2.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output 'SAVED after-cancel2.png'
