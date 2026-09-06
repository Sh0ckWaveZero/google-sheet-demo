# Verify the per-row Edit/Delete buttons and the theme toggle.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

Add-Type -Namespace Win32 -Name Native -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint data, System.UIntPtr extra);
[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)] public static extern System.IntPtr FindWindow(string cls, string title);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SendMessage(System.IntPtr hWnd, uint msg, System.IntPtr w, System.IntPtr l);
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
Write-Output ("APP PID: " + $proc.Id)

function Find-ByName($parent, $name) {
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $name)
    return $parent.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $cond)
}
function Find-All($parent) {
    return $parent.FindAll([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.Condition]::TrueCondition)
}
function Click-At([int]$x, [int]$y) {
    [Win32.Native]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 150
    [Win32.Native]::mouse_event(2, 0, 0, 0, [System.UIntPtr]::Zero)
    [Win32.Native]::mouse_event(4, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 350
}
function Click-Center($el) {
    $r = $el.Current.BoundingRectangle
    Click-At ([int]($r.X + $r.Width / 2)) ([int]($r.Y + $r.Height / 2))
}
function Save-Shot([string]$name) {
    Start-Sleep -Milliseconds 600
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save((Join-Path $outDir ("$name.png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function Focus-Main {
    [Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
    [Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
    [Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
    [Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
}
function Wait-Dialog([string]$title) {
    for ($i = 0; $i -lt 16; $i++) {
        $h = [Win32.Native]::FindWindow('#32770', $title)
        if ($h -ne [System.IntPtr]::Zero) { return $h }
        Start-Sleep -Milliseconds 250
    }
    return [System.IntPtr]::Zero
}
function Close-Dialog([string]$title) {
    $h = Wait-Dialog $title
    if ($h -ne [System.IntPtr]::Zero) {
        [Win32.Native]::SendMessage($h, 0x0010, [System.IntPtr]::Zero, [System.IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds 400
        Write-Output ("CLOSED: " + $title)
    }
}
function Get-StatusText {
    foreach ($el in Find-All $root) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Appended|Saved|Updated|Deleted|Editing|Filtering|Append cancelled|Save cancelled|Update cancelled|Delete cancelled|Edit cancelled|Switched|Load failed|Append failed|Save failed|Update failed|Delete failed|Cell changed)') {
            return $n
        }
    }
    return '(status not found)'
}
function Wait-StatusLike([string]$prefix, [int]$seconds) {
    for ($i = 0; $i -lt ($seconds * 2); $i++) {
        $s = Get-StatusText
        if ($s.StartsWith($prefix)) { return $s }
        Start-Sleep -Milliseconds 500
    }
    return Get-StatusText
}

Close-Dialog 'Confirm delete' | Out-Null
Focus-Main

# load data
Click-Center (Find-ByName $root 'Load')
$loaded = Wait-StatusLike 'Loaded' 20
Write-Output ("LOAD: " + $loaded)

# geometry: grid from the search-label anchor; row buttons live at the right edge
$lblSearch = Find-ByName $root 'Search (ID or Name)'
$gridX = [int]$lblSearch.Current.BoundingRectangle.X
$gridY = [int]($lblSearch.Current.BoundingRectangle.Y) + 42
$rowH = 30

# --- 1) row Edit button (right-most area, first data row) ---
$grid = $null
foreach ($el in Find-All $root) {
    $r = $el.Current.BoundingRectangle
    if ($el.Current.Name -eq '' -and $r.Height -gt 120 -and $r.Width -gt 300) { $grid = $el; break }
}
if (-not $grid) { throw 'grid element not found' }
$gr = $grid.Current.BoundingRectangle
Write-Output ("GRID RECT: " + $gr.ToString())
Click-At ([int]($gr.Right - 45)) ([int]($gr.Y + 34 + [int]($rowH / 2)))   # Edit cell of row 1
Start-Sleep -Milliseconds 600
Write-Output ("STATUS AFTER ROW-EDIT: " + (Get-StatusText))
Save-Shot '71-row-edit-clicked'

# cancel the edit
$cancel = Find-ByName $root 'Cancel Edit'
if ($cancel -and $cancel.Current.IsEnabled) { Click-Center $cancel; Write-Output 'CANCELLED EDIT' }

# --- 2) row Delete button on row 1: confirm with ENTER ---
Click-At ([int]($gr.Right - 45 - 62)) ([int]($gr.Y + 34 + [int]($rowH / 2)))   # Delete cell of row 1
$confirm = Wait-Dialog 'Confirm delete'
if ($confirm -ne [System.IntPtr]::Zero) {
    Write-Output 'CONFIRM DIALOG APPEARED'
    Save-Shot '72-row-delete-confirm'
    [Win32.Native]::SetForegroundWindow($confirm) | Out-Null
    Start-Sleep -Milliseconds 250
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    $s = Wait-StatusLike 'Deleted' 15
    Write-Output ("STATUS AFTER ROW-DELETE: " + $s)
    Save-Shot '73-after-row-delete'
} else {
    Write-Output 'FAIL: confirm dialog did not appear'
}

# --- 3) theme toggle ---
$theme = $null
foreach ($el in Find-All $root) {
    if ($el.Current.Name -match '^(Switched|)$' -and $false) { }
}
# theme button has no text (icon only) - find the small square next to Load
$btnLoad = Find-ByName $root 'Load'
$lr = $btnLoad.Current.BoundingRectangle
Click-At ([int]($lr.X - 22)) ([int]($lr.Y + $lr.Height / 2))
Start-Sleep -Milliseconds 700
Write-Output ("STATUS AFTER THEME CLICK: " + (Get-StatusText))
Save-Shot '74-theme-toggled'
Click-At ([int]($lr.X - 22)) ([int]($lr.Y + $lr.Height / 2))
Start-Sleep -Milliseconds 700
Write-Output ("STATUS AFTER THEME CLICK BACK: " + (Get-StatusText))
Write-Output 'DONE'
