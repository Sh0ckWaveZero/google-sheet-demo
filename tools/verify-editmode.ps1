# Enter edit mode via the row pencil icon (grid-rect-relative), then capture.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace Win32 -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint data, System.UIntPtr extra);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr hWnd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern System.IntPtr GetForegroundWindow();
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ShowWindow(System.IntPtr hWnd, int cmd);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, System.UIntPtr extra);
'@

$outDir = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check'
$proc = Get-Process GoogleSheetsDemo -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
if (-not $proc) { throw 'app not running' }
$root = [System.Windows.Automation.AutomationElement]::FromHandle($proc.MainWindowHandle)

function Find-ByName($parent, $name) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $name)
    return $parent.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $cond)
}
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
    Start-Sleep -Milliseconds 150
    [Win32.Native]::mouse_event(2, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 80
    [Win32.Native]::mouse_event(4, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 500
}

[Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
[Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
[Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 400
Write-Output ("FG: " + ([Win32.Native]::GetForegroundWindow() -eq $proc.MainWindowHandle))

# find the widest empty pane = the grid
$grid = $null
foreach ($el in (Find-All $root)) {
    $r = $el.Current.BoundingRectangle
    if ($el.Current.Name -eq '' -and $r.Height -gt 120 -and $r.Width -gt 300) {
        if ($null -eq $grid -or $r.Height -gt $grid.Current.BoundingRectangle.Height) { $grid = $el }
    }
}
if (-not $grid) { throw 'grid not found' }
$gr = $grid.Current.BoundingRectangle
Write-Output ("GRID RECT: " + $gr.ToString())

# Edit column is the second-from-right: its center is about
# (Right - DeleteWidth - EditWidth/2). Delete ~ 8% of width, Edit ~ 8%.
$editX = [int]($gr.Right - ($gr.Width * 0.16))
$firstRowY = [int]($gr.Y + 34 + 15)
Write-Output ("CLICKING EDIT AT: $editX,$firstRowY")
Click-At $editX $firstRowY
Start-Sleep -Milliseconds 800
Write-Output ("STATUS: " + (Get-StatusText))

$bmp = New-Object System.Drawing.Bitmap(1920, 1080)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen(0, 0, 0, 0, $bmp.Size)
$bmp.Save((Join-Path $outDir 'A7-edit-mode.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

# click Cancel
$cancel = Find-ByName $root 'Cancel'
if ($cancel) {
    $r = $cancel.Current.BoundingRectangle
    Write-Output ("CANCEL RECT: " + $r.ToString())
    Click-At ([int]($r.X + $r.Width / 2)) ([int]($r.Y + $r.Height / 2))
    Start-Sleep -Milliseconds 700
    Write-Output ("AFTER CANCEL: " + (Get-StatusText))
}
Write-Output 'DONE'
