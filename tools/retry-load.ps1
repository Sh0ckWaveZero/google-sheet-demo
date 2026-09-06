# ASCII-only. Retry loading: alternate keyboard TAB+SPACE and direct Load
# click until the grid fills, capturing after each attempt.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class WD {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][WD]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle

for ($i = 1; $i -le 5; $i++) {
    [WD]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
    [void][WD]::SetForegroundWindow($main)
    [WD]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 700
    $fgOk = ([WD]::GetForegroundWindow() -eq $main)
    Write-Output ("attempt " + $i + " fg=" + $fgOk)

    if ($i % 2 -eq 1) {
        # keyboard: ID box -> TAB to Load -> SPACE
        [System.Windows.Forms.SendKeys]::SendWait("{TAB}")
        Start-Sleep -Milliseconds 400
        [System.Windows.Forms.SendKeys]::SendWait(" ")
    } else {
        # mouse: click the Load button
        $mr = New-Object WD+R
        [void][WD]::GetWindowRect($main, [ref]$mr)
        [void][WD]::SetCursorPos($mr.L + 820, $mr.T + 76)
        Start-Sleep -Milliseconds 350
        [WD]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 120
        [WD]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    }

    Start-Sleep -Milliseconds 7000

    $r = New-Object WD+R
    [void][WD]::GetWindowRect($main, [ref]$r)
    $wd = $r.Rt - $r.L
    $ht = $r.B - $r.T
    $bmp = New-Object System.Drawing.Bitmap($wd, $ht)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
    $g.Dispose()
    $out = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\try' + $i + '.png'
    $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output ("SAVED try" + $i + ".png")
}
