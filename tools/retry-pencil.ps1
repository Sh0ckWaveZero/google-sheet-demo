# Retry the row-pencil click at several x offsets inside the Edit column.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace Win32 -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint data, System.UIntPtr extra);
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
function Click-At([int]$x, [int]$y) {
    [Win32.Native]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 200
    [Win32.Native]::mouse_event(2, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 100
    [Win32.Native]::mouse_event(4, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 700
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
$gr = $grid.Current.BoundingRectangle
$rowY = [int]($gr.Y + 34 + 15)

foreach ($frac in @(0.155, 0.135, 0.175, 0.12, 0.10)) {
    if ((Get-StatusText).StartsWith('Editing')) { break }
    $x = [int]($gr.Right - ($gr.Width * $frac))
    Write-Output ("TRY x=$x  (frac $frac)")
    Click-At $x $rowY
    $s = Get-StatusText
    Write-Output ("  -> " + $s)
}
Write-Output ("FINAL: " + (Get-StatusText))
