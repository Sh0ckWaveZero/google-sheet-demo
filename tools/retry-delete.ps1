# ASCII-only. Retry clicking the first row's trash until the confirm dialog
# appears (max 4 tries), capture it, press No, capture the after state.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W6 {
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W6]::SetProcessDPIAware()

function Save-Shot([IntPtr]$h, [string]$out) {
    $r = New-Object W6+R
    [void][W6]::GetWindowRect($h, [ref]$r)
    $wd = $r.Rt - $r.L
    $ht = $r.B - $r.T
    $bmp = New-Object System.Drawing.Bitmap($wd, $ht)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
    $g.Dispose()
    $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output ("SAVED " + $out)
}

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle
$mr = New-Object W6+R
[void][W6]::GetWindowRect($main, [ref]$mr)

$found = [IntPtr]::Zero
for ($i = 1; $i -le 4; $i++) {
    $cx = $mr.L + 852
    $cy = $mr.T + 187
    [void][W6]::SetCursorPos($cx, $cy)
    Start-Sleep -Milliseconds 350
    [W6]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 120
    [W6]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 1300
    $found = [W6]::FindWindow('#32770', 'Confirm delete')
    Write-Output ("try " + $i + " dialog=" + ($found -ne [IntPtr]::Zero))
    if ($found -ne [IntPtr]::Zero) { break }
}

if ($found -ne [IntPtr]::Zero) {
    Save-Shot $found 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\confirm-delete.png'
    Start-Sleep -Milliseconds 300
    [W6]::keybd_event(0x4E, 0, 0, [UIntPtr]::Zero)   # N = No
    [W6]::keybd_event(0x4E, 0, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 900
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\after-cancel.png'
} else {
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\no-dialog.png'
}
