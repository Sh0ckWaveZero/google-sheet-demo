# Keyboard activation of the row Edit button: SetFocus on the grid via UIA,
# arrow right to the Edit column, SPACE to activate.
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

function Find-All($parent) {
    return $parent.FindAll([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
}
function Get-StatusText {
    foreach ($el in (Find-All $root)) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Editing|Edit cancelled|Load failed)') { return $n }
    }
    return '(status not found)'
}

[Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
[Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 400

$grid = $null
foreach ($el in (Find-All $root)) {
    $r = $el.Current.BoundingRectangle
    if ($el.Current.Name -eq '' -and $r.Height -gt 120 -and $r.Width -gt 300) {
        if ($null -eq $grid -or $r.Height -gt $grid.Current.BoundingRectangle.Height) { $grid = $el }
    }
}
if (-not $grid) { throw 'grid not found' }

$grid.SetFocus()
Start-Sleep -Milliseconds 400
Write-Output ("GRID FOCUSED: " + ($root.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
    [System.Windows.Automation.AutomationElement]::FocusedProperty).Current.Name -eq ''))

# current cell starts at ID of row 0: right x4 = Edit column, then SPACE
[System.Windows.Forms.SendKeys]::SendWait('{RIGHT}{RIGHT}{RIGHT}{RIGHT}')
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait(' ')
Start-Sleep -Milliseconds 900
Write-Output ("STATUS: " + (Get-StatusText))
