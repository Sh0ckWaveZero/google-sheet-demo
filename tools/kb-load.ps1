# Keyboard-only: focus app, Tab to Load, Enter; poll status. Then Tab to the
# grid path for edit mode via the pencil (CellContentClick needs a mouse hit,
# so for edit mode use double-click keyboard alternative: none - skip; the
# per-row icon test already passed earlier; here we only re-verify Load).
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

function Get-StatusText {
    $all = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($el in $all) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Editing|Edit cancelled|Load failed)') { return $n }
    }
    return '(status not found)'
}

[Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
[Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 500
Write-Output ("FG: " + ([Win32.Native]::GetForegroundWindow() -eq $proc.MainWindowHandle) + "  BEFORE: " + (Get-StatusText))

# focus the ID box explicitly (TabIndex 0), then Tab once = Load button
[System.Windows.Forms.SendKeys]::SendWait('{TAB}')
Start-Sleep -Milliseconds 250
[System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
Start-Sleep -Seconds 4
Write-Output ("AFTER LOAD: " + (Get-StatusText))
Write-Output 'DONE'
