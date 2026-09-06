# ASCII-only. Launch the app, bring it to front, capture the window rect.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class W {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern void keybd_event(byte b, byte s, uint f, UIntPtr e);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }
}
"@

[void][W]::SetProcessDPIAware()

$exe = 'C:\lisa\Demo_Spread_Sheet\src\GoogleSheetsDemo\bin\Debug\GoogleSheetsDemo.exe'
$proc = Start-Process -FilePath $exe -PassThru
Start-Sleep -Milliseconds 4500

$proc.Refresh()
$h = $proc.MainWindowHandle
if ($h -eq [IntPtr]::Zero) { Write-Output 'NO_WINDOW'; exit 1 }

# foreground with the ALT hold trick
[W]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
[void][W]::SetForegroundWindow($h)
[W]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 700
$fg = [W]::GetForegroundWindow()
Write-Output ("FG_MATCH=" + ($fg -eq $h))

$r = New-Object W+R
[void][W]::GetWindowRect($h, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
Write-Output ("RECT=" + $r.L + "," + $r.T + " " + $wd + "x" + $ht)

$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$out = 'C:\lisa\Demo_Spread_Sheet\tools\ui-check\fix-center.png'
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output ("SAVED=" + $out)
