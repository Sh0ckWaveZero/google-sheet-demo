# ASCII-only. Close the Load Failed dialog, restore the spreadsheet GUID,
# click Load, capture the loaded grid.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class WE {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][WE]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle

# 1. close the Load Failed dialog via WM_COMMAND IDOK=2 (focus-independent)
$dlg = [WE]::FindWindow('#32770', 'Load Failed')
if ($dlg -ne [IntPtr]::Zero) {
    [void][WE]::PostMessage($dlg, 0x0111, [IntPtr]2, [IntPtr]::Zero)
    Start-Sleep -Milliseconds 600
    Write-Output 'DIALOG_CLOSED'
}

[WE]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[void][WE]::SetForegroundWindow($main)
[WE]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700

$mr = New-Object WE+R
[void][WE]::GetWindowRect($main, [ref]$mr)

# 2. click into the ID box, select all, type the GUID back
[void][WE]::SetCursorPos($mr.L + 365, $mr.T + 76)
Start-Sleep -Milliseconds 350
[WE]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
[WE]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 500
[System.Windows.Forms.SendKeys]::SendWait("^a")
Start-Sleep -Milliseconds 250
[System.Windows.Forms.SendKeys]::SendWait("1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs")
Start-Sleep -Milliseconds 400

# 3. click Load and wait for the network read
[void][WE]::SetCursorPos($mr.L + 820, $mr.T + 76)
Start-Sleep -Milliseconds 350
[WE]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 120
[WE]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 9000

$r = New-Object WE+R
[void][WE]::GetWindowRect($main, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\loaded3.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output 'SAVED loaded3.png'
