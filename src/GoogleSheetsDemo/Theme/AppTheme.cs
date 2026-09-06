using System;
using System.Drawing;
using Microsoft.Win32;

namespace GoogleSheetsDemo.Theme
{
    /// <summary>Set of colors applied to every themed control of the main window.</summary>
    public class ThemePalette
    {
        public Color FormBack;
        public Color PrimaryText;
        public Color LabelText;
        public Color StatusText;
        public Color StatusError;

        public Color GridBack;
        public Color GridLine;
        public Color HeaderBack;
        public Color HeaderText;
        public Color CellBack;
        public Color CellText;
        public Color SelectionBack;
        public Color SelectionText;
        public Color AlternatingRow;

        public Color TextBoxBack;
        public Color TextBoxText;

        public Color SecondaryBack;
        public Color SecondaryFore;
        public Color SecondaryHover;
        public Color SecondaryPressed;

        public Color AppendBack;
        public Color AppendFore;
        public Color AppendHover;
        public Color AppendPressed;

        public Color SaveBack;
        public Color SaveFore;
        public Color SaveHover;
        public Color SavePressed;

        public Color DeleteBack;
        public Color DeleteFore;
        public Color DeleteHover;
        public Color DeletePressed;

        public Color SpinnerDisc;
        public Color SpinnerTrack;
        public Color SpinnerArc;
    }

    /// <summary>Light and dark palettes, styled after the Google apps look.</summary>
    public static class AppTheme
    {
        public static readonly ThemePalette Light = new ThemePalette
        {
            FormBack = Color.FromArgb(248, 249, 250),
            PrimaryText = Color.FromArgb(32, 33, 36),
            LabelText = Color.FromArgb(86, 96, 110),
            StatusText = Color.FromArgb(32, 33, 36),
            StatusError = Color.Firebrick,

            GridBack = Color.White,
            GridLine = Color.FromArgb(232, 234, 237),
            HeaderBack = Color.FromArgb(241, 243, 244),
            HeaderText = Color.FromArgb(32, 33, 36),
            CellBack = Color.White,
            CellText = Color.FromArgb(40, 44, 52),
            SelectionBack = Color.FromArgb(232, 240, 254),
            SelectionText = Color.FromArgb(32, 33, 36),
            AlternatingRow = Color.FromArgb(250, 250, 251),

            TextBoxBack = Color.White,
            TextBoxText = Color.FromArgb(32, 33, 36),

            SecondaryBack = Color.FromArgb(241, 243, 244),
            SecondaryFore = Color.FromArgb(32, 33, 36),
            SecondaryHover = Color.FromArgb(232, 234, 237),
            SecondaryPressed = Color.FromArgb(218, 220, 224),

            AppendBack = Color.FromArgb(232, 240, 254),
            AppendFore = Color.FromArgb(25, 103, 210),
            AppendHover = Color.FromArgb(210, 227, 252),
            AppendPressed = Color.FromArgb(174, 203, 250),

            SaveBack = Color.FromArgb(26, 115, 232),
            SaveFore = Color.White,
            SaveHover = Color.FromArgb(27, 102, 201),
            SavePressed = Color.FromArgb(23, 78, 166),

            DeleteBack = Color.FromArgb(252, 232, 230),
            DeleteFore = Color.FromArgb(179, 38, 30),
            DeleteHover = Color.FromArgb(250, 210, 205),
            DeletePressed = Color.FromArgb(245, 190, 183),

            SpinnerDisc = Color.White,
            SpinnerTrack = Color.FromArgb(232, 234, 237),
            SpinnerArc = Color.FromArgb(26, 115, 232)
        };

        public static readonly ThemePalette Dark = new ThemePalette
        {
            FormBack = Color.FromArgb(32, 33, 36),
            PrimaryText = Color.FromArgb(232, 234, 236),
            LabelText = Color.FromArgb(154, 160, 166),
            StatusText = Color.FromArgb(232, 234, 236),
            StatusError = Color.FromArgb(242, 139, 130),

            GridBack = Color.FromArgb(32, 33, 36),
            GridLine = Color.FromArgb(58, 60, 64),
            HeaderBack = Color.FromArgb(44, 46, 49),
            HeaderText = Color.FromArgb(232, 234, 236),
            CellBack = Color.FromArgb(38, 40, 43),
            CellText = Color.FromArgb(224, 227, 231),
            SelectionBack = Color.FromArgb(36, 74, 133),
            SelectionText = Color.White,
            AlternatingRow = Color.FromArgb(34, 36, 39),

            TextBoxBack = Color.FromArgb(48, 49, 52),
            TextBoxText = Color.FromArgb(232, 234, 236),

            SecondaryBack = Color.FromArgb(48, 49, 52),
            SecondaryFore = Color.FromArgb(232, 234, 236),
            SecondaryHover = Color.FromArgb(60, 64, 67),
            SecondaryPressed = Color.FromArgb(72, 76, 80),

            AppendBack = Color.FromArgb(26, 58, 110),
            AppendFore = Color.FromArgb(138, 180, 248),
            AppendHover = Color.FromArgb(36, 74, 140),
            AppendPressed = Color.FromArgb(44, 88, 164),

            SaveBack = Color.FromArgb(26, 115, 232),
            SaveFore = Color.White,
            SaveHover = Color.FromArgb(66, 133, 244),
            SavePressed = Color.FromArgb(23, 78, 166),

            DeleteBack = Color.FromArgb(60, 32, 32),
            DeleteFore = Color.FromArgb(242, 139, 130),
            DeleteHover = Color.FromArgb(78, 40, 40),
            DeletePressed = Color.FromArgb(94, 48, 48),

            SpinnerDisc = Color.FromArgb(48, 49, 52),
            SpinnerTrack = Color.FromArgb(60, 64, 67),
            SpinnerArc = Color.FromArgb(138, 180, 248)
        };
    }

    /// <summary>Reads the Windows personalization theme ("Apps dark mode").</summary>
    public static class SystemTheme
    {
        public static bool IsDarkMode()
        {
            // manual override for demos and tests: SHEETSDEMO_FORCE_DARK=1 or 0
            string forced = Environment.GetEnvironmentVariable("SHEETSDEMO_FORCE_DARK");
            if (forced == "1")
            {
                return true;
            }
            if (forced == "0")
            {
                return false;
            }

            // Windows 10+: 0 = dark apps, 1 = light apps. Missing key means
            // an older Windows that only has the light theme.
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key != null)
                {
                    object value = key.GetValue("AppsUseLightTheme");
                    if (value is int)
                    {
                        return ((int)value) == 0;
                    }
                }
            }
            return false;
        }
    }
}
