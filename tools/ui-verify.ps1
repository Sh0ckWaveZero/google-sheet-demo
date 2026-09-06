# Verify the redesigned app + its dialogs (v7).
# Dialogs are found via Win32 FindWindow (the UIA bridge does not expose them),
# closed via WM_CLOSE. Screenshots are the evidence.
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
    $x = [int]($r.X + $r.Width / 2); $y = [int]($r.Y + $r.Height / 2)
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
    [Win32.Native]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 300
}
function Wait-Dialog([string]$title) {
    for ($i = 0; $i -lt 20; $i++) {
        $h = [Win32.Native]::FindWindow('#32770', $title)
        if ($h -ne [System.IntPtr]::Zero) { return $h }
        Start-Sleep -Milliseconds 250
    }
    return [System.IntPtr]::Zero
}
function Close-Dialog([string]$title) {
    $h = Wait-Dialog $title
    if ($h -ne [System.IntPtr]::Zero) {
        [Win32.Native]::SendMessage($h, 0x0010, [System.IntPtr]::Zero, [System.IntPtr]::Zero) | Out-Null  # WM_CLOSE
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
function Status-Text {
    foreach ($el in Find-All $root) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Appended|Saved|Append cancelled|Save cancelled|Load failed|Append failed|Save failed)') {
            return $n
        }
    }
    return '(status not found)'
}

# close any leftover dialog from an earlier run
Close-Dialog 'Cannot append row' | Out-Null
Close-Dialog 'Load Failed' | Out-Null

Focus-Main
$boxes = Get-TextBoxes
if ($boxes.Count -lt 4) { throw ("expected 4 textboxes, found " + $boxes.Count) }
$tbId = $boxes[0]; $tbName = $boxes[1]; $tbQty = $boxes[2]; $tbPrice = $boxes[3]
Write-Output ("STATUS BEFORE: " + (Status-Text))

# --- 1) validation dialog on empty form ---
Click-Center (Find-ByName $root 'Append Row')
$open1 = Wait-Dialog 'Cannot append row'
if ($open1 -ne [System.IntPtr]::Zero) {
    Write-Output 'VALIDATION DIALOG APPEARED: Cannot append row (warning icon)'
    Save-Shot '02-validation-dialog'
    Close-Dialog 'Cannot append row' | Out-Null
} else { Write-Output 'FAIL: validation dialog did not appear'; Save-Shot '02-FAIL-no-dialog' }

# --- 2) valid append ---
Focus-Main
Click-Center $tbName;  [System.Windows.Forms.SendKeys]::SendWait('UI Test')
Click-Center $tbQty;   [System.Windows.Forms.SendKeys]::SendWait('7')
Click-Center $tbPrice; [System.Windows.Forms.SendKeys]::SendWait('59.9')
Click-Center (Find-ByName $root 'Append Row')
Start-Sleep -Seconds 5
Write-Output ("STATUS AFTER APPEND: " + (Status-Text))
Close-Dialog 'Append Failed' | Out-Null
Save-Shot '03-after-append'

# --- 3) runtime error dialog: clear spreadsheet id, Load ---
Focus-Main
Click-Center $tbId
[System.Windows.Forms.SendKeys]::SendWait('^a')
[System.Windows.Forms.SendKeys]::SendWait('{DEL}')
Click-Center (Find-ByName $root 'Load')
Start-Sleep -Milliseconds 1500
$open3 = Wait-Dialog 'Load Failed'
if ($open3 -ne [System.IntPtr]::Zero) {
    Write-Output 'ERROR DIALOG APPEARED: Load Failed (error icon)'
    Save-Shot '04-error-dialog'
    Close-Dialog 'Load Failed' | Out-Null
} else { Write-Output 'FAIL: error dialog did not appear'; Save-Shot '04-FAIL-no-dialog' }

# restore the spreadsheet id
Focus-Main
Click-Center $tbId
[System.Windows.Forms.SendKeys]::SendWait('1SbOWOfgu2R4cg_PmNtFQA7RxoryUUI_UFsj5fZGIFbs')
Start-Sleep -Milliseconds 300
Write-Output ("STATUS AT END: " + (Status-Text))
Save-Shot '05-restored'
Write-Output 'DONE'
