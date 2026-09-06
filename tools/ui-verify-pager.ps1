# Verify pagination: page info label, Prev/Next enable states, page-size combo.
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
function Save-Shot([string]$name) {
    Start-Sleep -Milliseconds 500
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save((Join-Path $outDir ("$name.png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function Pager-Report([string]$stage) {
    $info = Find-ByName $root 'Page \d+ of \d+.*'
    if (-not $info) {
        # NameProperty is plain text; find the label by scanning
        foreach ($el in Find-All $root) {
            if ($el.Current.Name -match '^Page \d+ of \d+') { $info = $el; break }
        }
    }
    $label = '(label not found)'
    if ($info) { $label = $info.Current.Name }
    $prev = Find-ByName $root '< Prev'
    $next = Find-ByName $root 'Next >'
    $prevState = $(if ($prev -and $prev.Current.IsEnabled) { 'ON' } else { 'OFF' })
    $nextState = $(if ($next -and $next.Current.IsEnabled) { 'ON' } else { 'OFF' })
    Write-Output ("[{0}] {1} | Prev={2} Next={3}" -f $stage, $label, $prevState, $nextState)
}
function Get-StatusText {
    foreach ($el in Find-All $root) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Appended|Saved|Updated|Deleted|Editing|Filtering|Append cancelled|Save cancelled|Update cancelled|Delete cancelled|Edit cancelled|Load failed|Append failed|Save failed|Update failed|Delete failed|Cell changed)') {
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
function Get-ComboBox {
    foreach ($el in Find-All $root) {
        if ($el.Current.ClassName -match 'COMBOBOX') { return $el }
    }
    return $null
}

Focus-Main

# load the sheet (retry once if the click did not register)
Click-Center (Find-ByName $root 'Load')
$s = Wait-StatusLike 'Loaded' 25
if (-not $s.StartsWith('Loaded')) {
    Write-Output ("LOAD RETRY, status was: " + $s)
    Focus-Main
    Click-Center (Find-ByName $root 'Load')
    $s = Wait-StatusLike 'Loaded' 25
}
Write-Output ("LOAD RESULT: " + $s)
Start-Sleep -Milliseconds 500
Pager-Report '1-LOADED-DEFAULT-SIZE'
Save-Shot '30-pager-page1-default'

# shrink page size to 2: open the combo, highlight 2 (two up from 10), commit with ENTER
$combo = Get-ComboBox
if (-not $combo) { throw 'combo not found' }
Click-Center $combo
[System.Windows.Forms.SendKeys]::SendWait('{UP}{UP}')
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
Start-Sleep -Milliseconds 800
Pager-Report '2-PAGESIZE-2'
Save-Shot '31-pager-page1-of2'

# flip pages with the keyboard: combo has focus after commit;
# Shift+Tab lands on "Next >" (TabIndex 13), Space presses it
[System.Windows.Forms.SendKeys]::SendWait('+({TAB})')
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait(' ')
Start-Sleep -Milliseconds 700
Pager-Report '3-NEXT-TO-LAST-PAGE'
Save-Shot '32-pager-page2'

# Shift+Tab again lands on "< Prev" (TabIndex 12), Space presses it
[System.Windows.Forms.SendKeys]::SendWait('+({TAB})')
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait(' ')
Start-Sleep -Milliseconds 700
Pager-Report '4-PREV-BACK'

# restore page size 10 (2 -> 5 -> 10)
Click-Center $combo
[System.Windows.Forms.SendKeys]::SendWait('{DOWN}{DOWN}')
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
Start-Sleep -Milliseconds 800
Pager-Report '5-PAGESIZE-BACK-10'
Save-Shot '33-pager-restored'
Write-Output ("STATUS AT END: " + (Get-StatusText))
Write-Output 'DONE'
