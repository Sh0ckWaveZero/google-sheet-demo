# ASCII-only. Close the MessageBox as "No" by posting WM_COMMAND(IDNO=7)
# directly to the dialog window - does not depend on keyboard focus.
$ErrorActionPreference = 'Stop'

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class WA {
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string cls, string title);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern IntPtr SendMessageGetString(IntPtr h, int msg, IntPtr wp, System.Text.StringBuilder lp);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
}
"@

[void][WA]::SetProcessDPIAware()

$dlg = [WA]::FindWindow('#32770', 'Confirm delete')
if ($dlg -eq [IntPtr]::Zero) {
    Write-Output 'DIALOG_OPEN=NO (already closed)'
    exit 0
}

# try to find the No button first and click it
$noBtn = [IntPtr]::Zero
$after = [IntPtr]::Zero
while ($true) {
    $btn = [WA]::FindWindowEx($dlg, $after, 'Button', $null)
    if ($btn -eq [IntPtr]::Zero) { break }
    $sb = New-Object System.Text.StringBuilder 256
    [void][WA]::SendMessageGetString($btn, 0x000D, [IntPtr]256, $sb)   # WM_GETTEXT
    $text = $sb.ToString()
    Write-Output ("button: '" + $text + "'")
    if ($text -match 'No') { $noBtn = $btn }
    $after = $btn
}

if ($noBtn -ne [IntPtr]::Zero) {
    Write-Output 'CLICKING No BUTTON'
    [void][WA]::SendMessage($noBtn, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero)   # BM_CLICK
} else {
    Write-Output 'POSTING WM_COMMAND IDNO'
    [void][WA]::PostMessage($dlg, 0x0111, [IntPtr]7, [IntPtr]::Zero)          # WM_COMMAND, IDNO=7
}
Start-Sleep -Milliseconds 1000

$dlg = [WA]::FindWindow('#32770', 'Confirm delete')
Write-Output ("STILL_OPEN=" + ($dlg -ne [IntPtr]::Zero))
