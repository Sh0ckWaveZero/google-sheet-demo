Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
$procs = Get-Process GoogleSheetsDemo -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 }
Write-Output ("COUNT: " + @($procs).Count)
foreach ($p in $procs) {
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($p.MainWindowHandle)
    $status = '?'
    $all = $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($el in $all) {
        $n = $el.Current.Name
        if ($n -match '^(Ready|Loaded|Editing)') { $status = $n }
    }
    Write-Output ("PID=" + $p.Id + " HWND=" + $p.MainWindowHandle + " RECT=?" + " STATUS=[" + $status + "] STARTED=" + $p.StartTime.ToString('HH:mm:ss'))
}
