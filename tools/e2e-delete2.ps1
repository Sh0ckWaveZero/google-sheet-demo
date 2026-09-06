# ASCII-only. E2E delete with retries: repeat (foreground, click row-2 Name
# cell, arrows, Space) until the confirm dialog appears, confirm YES, wait
# for the save, reload via BM_CLICK, capture the final grid.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class WJ {
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

[void][WJ]::SetProcessDPIAware()

function Save-Shot([IntPtr]$h, [string]$out) {
    $r = New-Object WJ+R
    [void][WJ]::GetWindowRect($h, [ref]$r)
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

function Assert-Foreground([IntPtr]$main) {
    for ($k = 0; $k -lt 3; $k++) {
        [WJ]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
        [void][WJ]::SetForegroundWindow($main)
        [WJ]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 600
        if ([WJ]::GetForegroundWindow() -eq $main) { return $true }
    }
    return $false
}

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle

$triggered = $false
for ($attempt = 1; $attempt -le 5; $attempt++) {
    Write-Output ("delete attempt " + $attempt)
    if (-not (Assert-Foreground $main)) { Write-Output '  no foreground'; continue }

    $mr = New-Object WJ+R
    [void][WJ]::GetWindowRect($main, [ref]$mr)

    # select the Name cell of row 2 (first Test Product) as the current cell
    [void][WJ]::SetCursorPos($mr.L + 250, $mr.T + 218)
    Start-Sleep -Milliseconds 400
    [WJ]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 130
    [WJ]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 800

    # arrow Name(1) -> Delete(5), then Space activates the button cell
    [System.Windows.Forms.SendKeys]::SendWait("{RIGHT}{RIGHT}{RIGHT}{RIGHT}")
    Start-Sleep -Milliseconds 600
    [System.Windows.Forms.SendKeys]::SendWait(" ")
    Start-Sleep -Milliseconds 2000

    $dlg = [WJ]::FindWindow('#32770', 'Confirm delete')
    Write-Output ("  dialog=" + ($dlg -ne [IntPtr]::Zero))
    if ($dlg -ne [IntPtr]::Zero) { $triggered = $true; break }
}

if (-not $triggered) {
    Write-Output 'DELETE_NEVER_TRIGGERED'
    Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e2-stuck.png'
    exit 1
}

# confirm YES (IDYES=6)
[void][WJ]::PostMessage([WJ]::FindWindow('#32770', 'Confirm delete'), 0x0111, [IntPtr]6, [IntPtr]::Zero)
Write-Output 'CONFIRMED YES'
Start-Sleep -Milliseconds 13000
Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e2-after-delete.png'

# reload from the sheet
if (-not (Assert-Foreground $main)) { Write-Output 'reload: no foreground (continuing)' }
$loadBtn = [WJ]::FindChildByText($main, 'Load')
if ($loadBtn -ne [IntPtr]::Zero) {
    [void][WJ]::SendMessage($loadBtn, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero)
    Write-Output 'RELOADED'
    Start-Sleep -Milliseconds 10000
}
Save-Shot $main 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e2-reloaded.png'
