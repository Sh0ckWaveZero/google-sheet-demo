# Cleanup after the CRUD verification: clear the search box properly and
# re-append the "Test Product" row that the delete test removed.
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
function Click-Center($el) {
    $r = $el.Current.BoundingRectangle
    [Win32.Native]::SetCursorPos([int]($r.X + $r.Width / 2), [int]($r.Y + $r.Height / 2)) | Out-Null
    Start-Sleep -Milliseconds 150
    [Win32.Native]::mouse_event(2, 0, 0, 0, [System.UIntPtr]::Zero)
    [Win32.Native]::mouse_event(4, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 300
}
function Focus-Main {
    [Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
    [Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
    [Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
    [Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
}
function Status-Text {
    foreach ($el in Find-All $root) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Appended|Saved|Updated|Deleted|Editing|Filtering|Append cancelled|Save cancelled|Update cancelled|Delete cancelled|Edit cancelled|Load failed|Append failed|Save failed|Update failed|Delete failed|Cell changed)') {
            return $n
        }
    }
    return '(status not found)'
}

Focus-Main
$tbSearch = Find-ByName $root ''
$boxes = @()
foreach ($el in Find-All $root) {
    $r = $el.Current.BoundingRectangle
    $n = $el.Current.Name
    if (($n -eq '' -or $n -match '^[A-Za-z0-9_\-]{20,}$') -and $r.Width -gt 40 -and $r.Height -gt 12 -and $r.Height -lt 45) {
        $boxes += $el
    }
}
$boxes = $boxes | Sort-Object { [math]::Round($_.Current.BoundingRectangle.Y) }, { $_.Current.BoundingRectangle.X }
$tbId = $boxes[0]; $tbSearch = $boxes[1]; $tbName = $boxes[2]; $tbQty = $boxes[3]; $tbPrice = $boxes[4]

Write-Output ("SEARCH BOX CONTENT: [" + $tbSearch.Current.Name + "]")
Click-Center $tbSearch
[System.Windows.Forms.SendKeys]::SendWait('^({HOME})')
Start-Sleep -Milliseconds 120
[System.Windows.Forms.SendKeys]::SendWait('+^({END})')
Start-Sleep -Milliseconds 120
[System.Windows.Forms.SendKeys]::SendWait('{DEL}')
Start-Sleep -Milliseconds 500
Write-Output ("SEARCH BOX AFTER CLEAR: [" + $tbSearch.Current.Name + "]")
Write-Output ("STATUS: " + (Status-Text))

Click-Center $tbName;  [System.Windows.Forms.SendKeys]::SendWait('Test Product')
Click-Center $tbQty;   [System.Windows.Forms.SendKeys]::SendWait('25')
Click-Center $tbPrice; [System.Windows.Forms.SendKeys]::SendWait('50')
Click-Center (Find-ByName $root 'Append Row')
Start-Sleep -Seconds 5
Write-Output ("STATUS AFTER APPEND: " + (Status-Text))

$bmp = $null
$b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
$bmp.Save((Join-Path $outDir '16-final-state.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output 'DONE'
