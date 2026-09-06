# Tab-walk through controls, reporting the focused element each step, until
# focus lands inside the grid (wide empty pane).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace Win32 -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr hWnd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern System.IntPtr GetForegroundWindow();
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr hWnd, int cmd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, System.UIntPtr extra);
'@

$proc = Get-Process GoogleSheetsDemo -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw 'app not running' }
$root = [System.Windows.Automation.AutomationElement]::FromHandle($proc.MainWindowHandle)

function Get-Focused {
    $el = $root.FindFirst([System.Windows.Automation.TreeScope]::Subtree,
        [System.Windows.Automation.AutomationElement]::FocusedProperty)
    if (-not $el) { return '(none)' }
    $r = $el.Current.BoundingRectangle
    return ("name=[" + $el.Current.Name + "] class=[" + $el.Current.ClassName + "] rect=" + $r.Width + "x" + $r.Height)
}

[Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
[Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 400

for ($i = 1; $i -le 10; $i++) {
    [System.Windows.Forms.SendKeys]::SendWait('{TAB}')
    Start-Sleep -Milliseconds 350
    Write-Output ("TAB $i -> " + (Get-Focused))
}
