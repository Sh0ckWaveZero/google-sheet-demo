# Click the pencil cell, draw a red ring at the click point, capture to see
# exactly where the synthetic click landed.
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
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern System.IntPtr WindowFromPoint(POINT p);
'@

$outDir = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check'
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
$gr = $grid.Current.BoundingRectangle
$x = [int]($gr.Right - ($gr.Width * 0.155))
$y = [int]($gr.Y + 34 + 15)

# which window owns the point we are about to click?
$pt = New-Object Win32.Native+POINT
$pt.X = $x; $pt.Y = $y
$owner = [Win32.Native]::WindowFromPoint($pt)
Write-Output ("CLICK TARGET: $x,$y  OWNER HWND: $owner  APP HWND: $($proc.MainWindowHandle)")

[Win32.Native]::SetCursorPos($x, $y) | Out-Null
Start-Sleep -Milliseconds 300

# capture BEFORE the click (cursor visible at target)
$b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::Lime, 2)
$g.DrawEllipse($pen, $x - 12, $y - 12, 24, 24)
$pen.Dispose()
$bmp.Save((Join-Path $outDir 'A8-click-target.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()

[Win32.Native]::mouse_event(2, 0, 0, 0, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 90
[Win32.Native]::mouse_event(4, 0, 0, 0, [System.UIntPtr]::Zero)
Start-Sleep -Milliseconds 800
Write-Output ("STATUS: " + (Get-StatusText))
Write-Output 'DONE'
