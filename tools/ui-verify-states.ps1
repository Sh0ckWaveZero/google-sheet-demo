# Verify button enable/disable states at every stage (fresh, loaded, filtered,
# no-results). Reads IsEnabled through UIA and captures screenshots.
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
function Get-TextBoxes {
    $list = @()
    foreach ($el in Find-All $root) {
        $r = $el.Current.BoundingRectangle
        $n = $el.Current.Name
        if (($n -eq '' -or $n -match '^[A-Za-z0-9_\-]{20,}$') -and $r.Width -gt 40 -and $r.Height -gt 12 -and $r.Height -lt 45) {
            $list += $el
        }
    }
    return ($list | Sort-Object { [math]::Round($_.Current.BoundingRectangle.Y) }, { $_.Current.BoundingRectangle.X })
}
function Report([string]$stage) {
    $states = @()
    foreach ($btn in @('Load', 'Append Row', 'Update Row', 'Edit Selected', 'Delete Selected', 'Save Changes', 'Cancel Edit')) {
        $el = Find-ByName $root $btn
        if ($el) {
            $states += ("{0}={1}" -f $btn, ($(if ($el.Current.IsEnabled) { 'ON' } else { 'OFF' })))
        }
    }
    Write-Output ("[{0}] {1}" -f $stage, ($states -join ' '))
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

Focus-Main
$boxes = Get-TextBoxes
$tbSearch = $boxes[1]

# --- stage 1: fresh, empty grid ---
Report '1-FRESH-EMPTY'
Save-Shot '20-initial-disabled'

# --- stage 2: loaded (first row auto-selected) ---
Click-Center (Find-ByName $root 'Load')
$null = Wait-StatusLike 'Loaded' 20
Start-Sleep -Milliseconds 500
Report '2-LOADED'
Save-Shot '21-loaded-enabled'

# --- stage 3: filter with no results ---
Focus-Main
Click-Center $tbSearch
[System.Windows.Forms.SendKeys]::SendWait('zzz')
Start-Sleep -Milliseconds 800
Report '3-FILTER-NO-RESULT'
Save-Shot '22-filter-no-result-disabled'

# --- stage 4: filter cleared ---
Click-Center $tbSearch
[System.Windows.Forms.SendKeys]::SendWait('^({HOME})+^({END}){DEL}')
Start-Sleep -Milliseconds 800
Report '4-FILTER-CLEARED'
Save-Shot '23-filter-cleared'
Write-Output ("STATUS AT END: " + (Get-StatusText))
Write-Output 'DONE'
