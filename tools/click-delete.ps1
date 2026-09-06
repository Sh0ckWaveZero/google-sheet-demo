# ASCII-only. Click the first row's trash icon, verify the confirm dialog
# appears, capture it, press No, capture the after state.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W5 {
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W5]::SetProcessDPIAware()

function Save-Shot([IntPtr]$h, [string]$out) {
    $r = New-Object W5+R
    [void][W5]::GetWindowRect($h, [ref]$r)
    $wd = $r.Rt - $r.L
    $ht = $r.B - $r.T
    $bmp = New-Object System.Drawing.Bitmap($wd, $ht)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
    $g.Dispose()
    $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output ("SAVED " + $out + " (" + $wd + "x" + $ht + ")")
}

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$h = $proc.MainWindowRect
$main = $proc.MainWindowHandle
$mr = New-Object W5+R
[void][W5]::GetWindowRect($main, [ref]$mr)

# trash icon of the first data row, capture-relative (852,187); capture origin
# equals the window rect origin, so add them directly
$cx = $mr.L + 852
$cy = $mr.T + 187
[void][W5]::SetCursorPos($cx, $cy)
Start-Sleep -Milliseconds 250
[W5]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
[W5]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 1200

$dlg = [W5]::FindWindow('#32770', 'Confirm delete')
if ($dlg -ne [IntPtr]::Zero) {
    Write-Output 'DIALOG_FOUND=YES'
    Save-Shot $dlg 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\confirm-delete.png'
    Start-Sleep -Milliseconds 300
    [W5]::keybd_event(0x4E, 0, 0, [UIntPtr]::Zero)   # N = No
    [W5]::keybd_event(0x4E, 0, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 900
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\after-cancel.png'
} else {
    Write-Output 'DIALOG_FOUND=NO'
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\no-dialog.png'
}
