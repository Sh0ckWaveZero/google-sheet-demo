# ASCII-only. Find the "Load" button child window and BM_CLICK it directly.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

Add-Type -TypeDefinition @"
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public static class WH {
    public delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr parent, EnumProc proc, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern int GetWindowText(IntPtr h, StringBuilder sb, int max);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    public struct R { public int L, T, Rt, B; }

    public static List<string> ChildTexts = new List<string>();
    public static IntPtr Target = IntPtr.Zero;
    public static string Wanted = "";

    public static IntPtr FindChildByText(IntPtr parent, string text) {
        ChildTexts = new List<string>();
        Target = IntPtr.Zero;
        Wanted = text;
        EnumChildWindows(parent, delegate(IntPtr h, IntPtr lp) {
            var sb = new StringBuilder(256);
            GetWindowText(h, sb, 256);
            ChildTexts.Add(sb.ToString());
            if (Target == IntPtr.Zero && sb.ToString() == Wanted) { Target = h; }
            return true;
        }, IntPtr.Zero);
        return Target;
    }
}
"@

[void][WH]::SetProcessDPIAware()

$proc = Get-Process -Name GoogleSheetsDemo -ErrorAction Stop
$main = $proc.MainWindowHandle

$btn = [WH]::FindChildByText($main, 'Load')
Write-Output ("CHILD_TEXTS: " + ([string]::Join(' | ', [string[]][WH]::ChildTexts.ToArray())))
if ($btn -eq [IntPtr]::Zero) {
    Write-Output 'LOAD_BUTTON_NOT_FOUND'
    exit 1
}

Write-Output ('BM_CLICK Load hwnd=' + $btn)
[void][WH]::SendMessage($btn, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero)
Start-Sleep -Milliseconds 9000

$r = New-Object WH+R
[void][WH]::GetWindowRect($main, [ref]$r)
$wd = $r.Rt - $r.L
$ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($wd, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.L, $r.T, 0, 0, (New-Object System.Drawing.Size($wd, $ht)))
$g.Dispose()
$bmp.Save('C:\lisa\Demo_Spread_Sheet\tools\ui-check\e2e-loaded2.png', [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output 'SAVED e2e-loaded2.png'
