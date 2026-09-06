# Verify edit / delete / search on the live app (v8).
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
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

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
    Click-At ([int]($r.X + $r.Width / 2)) ([int]($r.Y + $r.Height / 2))
}
function Click-At([int]$x, [int]$y) {
    [Win32.Native]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 150
    [Win32.Native]::mouse_event(2, 0, 0, 0, [System.UIntPtr]::Zero)
    [Win32.Native]::mouse_event(4, 0, 0, 0, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 300
}
function Save-Shot([string]$name) {
    Start-Sleep -Milliseconds 700
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap($b.Width, $b.Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save((Join-Path $outDir ("$name.png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function Focus-Main {
    [Win32.Native]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null   # SW_RESTORE
    # hold ALT for a moment: Windows allows SetForegroundWindow while a key is down
    [Win32.Native]::keybd_event(0x12, 0, 0, [System.UIntPtr]::Zero)
    [Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
    [Win32.Native]::keybd_event(0x12, 0, 2, [System.UIntPtr]::Zero)
    Start-Sleep -Milliseconds 400
    $fg = [Win32.Native]::GetForegroundWindow()
    if ($fg -ne $proc.MainWindowHandle) { Write-Output 'WARN: app window is NOT foreground - clicks may miss' }
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
        Write-Output ("CLOSED DIALOG: " + $title)
        return $true
    }
    return $false
}
function Get-TextBoxes {
    $list = @()
    foreach ($el in Find-All $root) {
        $r = $el.Current.BoundingRectangle
        $n = $el.Current.Name
        $looksLikeId = ($n -match '^[A-Za-z0-9_\-]{20,}$')
        if (($n -eq '' -or $looksLikeId) -and $r.Width -gt 40 -and $r.Height -gt 12 -and $r.Height -lt 45) {
            $list += $el
        }
    }
    return ($list | Sort-Object { [math]::Round($_.Current.BoundingRectangle.Y) }, { $_.Current.BoundingRectangle.X })
}
function Get-Grid {
    $best = $null
    foreach ($el in Find-All $root) {
        $r = $el.Current.BoundingRectangle
        if ($el.Current.Name -eq '' -and $r.Height -gt 150 -and $r.Width -gt 300) {
            if ($null -eq $best -or $r.Height -gt $best.Current.BoundingRectangle.Height) { $best = $el }
        }
    }
    return $best
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
function Wait-StatusLike([string]$prefix, [int]$seconds) {
    for ($i = 0; $i -lt ($seconds * 2); $i++) {
        $s = Status-Text
        if ($s.StartsWith($prefix)) { return $s }
        Start-Sleep -Milliseconds 500
    }
    return Status-Text
}

Close-Dialog 'Cannot append row' | Out-Null
Close-Dialog 'Load Failed' | Out-Null
Close-Dialog 'Confirm delete' | Out-Null
Close-Dialog 'No row selected' | Out-Null

Focus-Main
$boxes = Get-TextBoxes
if ($boxes.Count -lt 5) { throw ("expected 5 textboxes (id, search, name, qty, price), found " + $boxes.Count) }
$tbId = $boxes[0]; $tbSearch = $boxes[1]; $tbName = $boxes[2]; $tbQty = $boxes[3]; $tbPrice = $boxes[4]

# load the sheet first: the grid starts empty and row clicks need data
Click-Center (Find-ByName $root 'Load')
$loaded = Wait-StatusLike 'Loaded' 20
Write-Output ("STATUS AFTER LOAD: " + $loaded)
if (-not $loaded.StartsWith('Loaded')) { throw 'Load did not complete' }
Save-Shot '10-toolbar-main'

# grid geometry from anchors: the search label sits at the toolbar row and the
# grid starts ~42px below the search box, left-aligned with it
$lblSearch = Find-ByName $root 'Search (ID or Name)'
if (-not $lblSearch) { throw 'search label not found' }
$gridX = [int]$lblSearch.Current.BoundingRectangle.X
$gridY = [int]$tbSearch.Current.BoundingRectangle.Y + 42
$rowH = 30
# click row 1 (below the 34px header)
Click-At ($gridX + 80) ($gridY + 40 + [int]($rowH / 2))

# --- 1) edit mode: Edit Selected loads the row into the form ---
Click-Center (Find-ByName $root 'Edit Selected')
Start-Sleep -Milliseconds 500
$nameText = $tbName.Current.Name
Write-Output ("EDIT MODE NAME FIELD: [" + $nameText + "]")
Write-Output ("STATUS IN EDIT: " + (Status-Text))
Save-Shot '11-edit-mode'

# --- 2) change quantity and update ---
Click-Center $tbQty
[System.Windows.Forms.SendKeys]::SendWait('^({HOME})')
[System.Windows.Forms.SendKeys]::SendWait('+^({END})')
[System.Windows.Forms.SendKeys]::SendWait('{DEL}30')
Click-Center (Find-ByName $root 'Update Row')
Start-Sleep -Seconds 5
Write-Output ("STATUS AFTER UPDATE: " + (Status-Text))
Close-Dialog 'Update Failed' | Out-Null
Save-Shot '12-after-update'

# --- 3) search filter ---
Focus-Main
Click-Center $tbSearch
[System.Windows.Forms.SendKeys]::SendWait('UI')
Start-Sleep -Milliseconds 700
Write-Output ("STATUS WHILE FILTERING: " + (Status-Text))
Save-Shot '13-filtering'
Click-Center $tbSearch
[System.Windows.Forms.SendKeys]::SendWait('^({HOME})+^({END}){DEL}')
Start-Sleep -Milliseconds 500
Write-Output ("STATUS AFTER CLEAR SEARCH: " + (Status-Text))

# --- 4) delete row 2 (UI Test) with confirm dialog ---
Click-At ($gridX + 80) ($gridY + 40 + $rowH + $rowH + [int]($rowH / 2))   # second data row
Click-Center (Find-ByName $root 'Delete Selected')
$confirm = Wait-Dialog 'Confirm delete'
if ($confirm -ne [System.IntPtr]::Zero) {
    Write-Output 'CONFIRM DIALOG APPEARED'
    Save-Shot '14-delete-confirm'
    [Win32.Native]::SetForegroundWindow($confirm) | Out-Null
    Start-Sleep -Milliseconds 250
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')   # Yes is the default button
    Start-Sleep -Seconds 5
    Write-Output ("STATUS AFTER DELETE: " + (Status-Text))
    Save-Shot '15-after-delete'
} else {
    Write-Output 'FAIL: confirm dialog did not appear'
    Save-Shot '14-FAIL-no-confirm'
}
Write-Output 'DONE'
