# ASCII-only. Keyboard-driven delete: click a cell to focus the grid, arrow
# to the Delete column, press Space (raises CellContentClick), capture the
# confirm dialog, press No.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W8 {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W8]::SetProcessDPIAware()

function Save-Shot([IntPtr]$h, [string]$out) {
    $r = New-Object W8+R
    [void][W8]::GetWindowRect($h, [ref]$r)
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

[W8]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[void][W8]::SetForegroundWindow($main)
[W8]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700
Write-Output ("FG=" + ([W8]::GetForegroundWindow() -eq $main))

# click the Name cell of row 1 to make it the current cell and focus the grid
$mr = New-Object W8+R
[void][W8]::GetWindowRect($main, [ref]$mr)
[void][W8]::SetCursorPos($mr.L + 250, $mr.T + 187)
Start-Sleep -Milliseconds 350
[W8]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 120
[W8]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700

# Name(1) -> Quantity(2) -> Price(3) -> Edit(4) -> Delete(5)
[System.Windows.Forms.SendKeys]::SendWait("{RIGHT}{RIGHT}{RIGHT}{RIGHT}")
Start-Sleep -Milliseconds 500

# Space activates the button cell -> CellContentClick
[System.Windows.Forms.SendKeys]::SendWait(" ")
Start-Sleep -Milliseconds 1500

$dlg = [W8]::FindWindow('#32770', 'Confirm delete')
if ($dlg -ne [IntPtr]::Zero) {
    Write-Output 'DIALOG_FOUND=YES'
    Save-Shot $dlg 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\confirm-delete.png'
    Start-Sleep -Milliseconds 300
    [W8]::keybd_event(0x4E, 0, 0, [UIntPtr]::Zero)   # N = No
    [W8]::keybd_event(0x4E, 0, 2, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 900
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\after-cancel.png'
} else {
    Write-Output 'DIALOG_FOUND=NO'
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\no-dialog2.png'
}
