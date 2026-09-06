# ASCII-only. E2E delete: focus grid, arrow to Delete column, Space to
# trigger, confirm Yes via WM_COMMAND, wait for save, reload, capture.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class WI {
    public delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent, EnumProc proc, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern int GetWindowText(IntPtr h, StringBuilder sb, int max);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint x, uint y, uint d, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }

    public static IntPtr FindChildByText(IntPtr parent, string text) {
        IntPtr found = IntPtr.Zero;
        EnumChildWindows(parent, delegate(IntPtr h, IntPtr lp) {
            var sb = new StringBuilder(256);
            GetWindowText(h, sb, 256);
            if (found == IntPtr.Zero && sb.ToString() == text) { found = h; }
            return true;
        }, IntPtr.Zero);
        return found;
    }
}
"@

[void][WI]::SetProcessDPIAware()

function Save-Shot([IntPtr]$h, [string]$out) {
    $r = New-Object WI+R
    [void][WI]::GetWindowRect($h, [ref]$r)
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
$mr = New-Object WI+R
[void][WI]::GetWindowRect($main, [ref]$mr)

# 1. foreground + click the Name cell of the first Test Product row (row 2)
[WI]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[void][WI]::SetForegroundWindow($main)
[WI]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700
Write-Output ("FG=" + ([WI]::GetForegroundWindow() -eq $main))

[void][WI]::SetCursorPos($mr.L + 250, $mr.T + 218)
Start-Sleep -Milliseconds 400
[WI]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 130
[WI]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700

# 2. arrow to the Delete column, Space activates the button cell
[System.Windows.Forms.SendKeys]::SendWait("{RIGHT}{RIGHT}{RIGHT}{RIGHT}")
Start-Sleep -Milliseconds 500
[System.Windows.Forms.SendKeys]::SendWait(" ")
Start-Sleep -Milliseconds 1800

# 3. confirm YES via WM_COMMAND (IDYES=6) on the dialog
$dlg = [WI]::FindWindow('#32770', 'Confirm delete')
if ($dlg -eq [IntPtr]::Zero) {
    Write-Output 'DIALOG_NOT_FOUND - aborting (nothing deleted)'
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e-nodialog.png'
    exit 1
}
Write-Output 'DIALOG_FOUND - confirming YES'
[void][WI]::PostMessage($dlg, 0x0111, [IntPtr]6, [IntPtr]::Zero)

# 4. wait for the save (clear + rewrite over the network)
Start-Sleep -Milliseconds 12000
Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e-after-delete.png'

# 5. reload from the sheet to prove the row is really gone
$loadBtn = [WI]::FindChildByText($main, 'Load')
if ($loadBtn -ne [IntPtr]::Zero) {
    [void][WI]::SendMessage($loadBtn, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero)
    Write-Output 'RELOADING'
    Start-Sleep -Milliseconds 10000
}
Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e-reloaded.png'
