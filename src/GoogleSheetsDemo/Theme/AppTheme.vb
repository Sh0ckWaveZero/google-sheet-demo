Imports System
Imports System.Drawing
Imports Microsoft.Win32

Namespace Theme

    ''' <summary>Set of colors applied to every themed control of the main window.</summary>
    Public Class ThemePalette

        Public FormBack As Color
        Public PrimaryText As Color
        Public LabelText As Color
        Public StatusText As Color
        Public StatusError As Color

        Public GridBack As Color
        Public GridLine As Color
        Public HeaderBack As Color
        Public HeaderText As Color
        Public CellBack As Color
        Public CellText As Color
        Public SelectionBack As Color
        Public SelectionText As Color
        Public AlternatingRow As Color

        Public TextBoxBack As Color
        Public TextBoxText As Color

        Public SecondaryBack As Color
        Public SecondaryFore As Color
        Public SecondaryHover As Color
        Public SecondaryPressed As Color

        Public AppendBack As Color
        Public AppendFore As Color
        Public AppendHover As Color
        Public AppendPressed As Color

        Public SaveBack As Color
        Public SaveFore As Color
        Public SaveHover As Color
        Public SavePressed As Color

        Public DeleteBack As Color
        Public DeleteFore As Color
        Public DeleteHover As Color
        Public DeletePressed As Color

        Public SpinnerDisc As Color
        Public SpinnerTrack As Color
        Public SpinnerArc As Color

    End Class

    ''' <summary>Light and dark palettes, styled after the Google apps look.</summary>
    Public Module AppTheme

        Public ReadOnly Light As New ThemePalette With {
            .FormBack = Color.FromArgb(248, 249, 250),
            .PrimaryText = Color.FromArgb(32, 33, 36),
            .LabelText = Color.FromArgb(86, 96, 110),
            .StatusText = Color.FromArgb(32, 33, 36),
            .StatusError = Color.Firebrick,
            .GridBack = Color.White,
            .GridLine = Color.FromArgb(232, 234, 237),
            .HeaderBack = Color.FromArgb(241, 243, 244),
            .HeaderText = Color.FromArgb(32, 33, 36),
            .CellBack = Color.White,
            .CellText = Color.FromArgb(40, 44, 52),
            .SelectionBack = Color.FromArgb(232, 240, 254),
            .SelectionText = Color.FromArgb(32, 33, 36),
            .AlternatingRow = Color.FromArgb(250, 250, 251),
            .TextBoxBack = Color.White,
            .TextBoxText = Color.FromArgb(32, 33, 36),
            .SecondaryBack = Color.FromArgb(241, 243, 244),
            .SecondaryFore = Color.FromArgb(32, 33, 36),
            .SecondaryHover = Color.FromArgb(232, 234, 237),
            .SecondaryPressed = Color.FromArgb(218, 220, 224),
            .AppendBack = Color.FromArgb(232, 240, 254),
            .AppendFore = Color.FromArgb(25, 103, 210),
            .AppendHover = Color.FromArgb(210, 227, 252),
            .AppendPressed = Color.FromArgb(174, 203, 250),
            .SaveBack = Color.FromArgb(26, 115, 232),
            .SaveFore = Color.White,
            .SaveHover = Color.FromArgb(27, 102, 201),
            .SavePressed = Color.FromArgb(23, 78, 166),
            .DeleteBack = Color.FromArgb(252, 232, 230),
            .DeleteFore = Color.FromArgb(179, 38, 30),
            .DeleteHover = Color.FromArgb(250, 210, 205),
            .DeletePressed = Color.FromArgb(245, 190, 183),
            .SpinnerDisc = Color.White,
            .SpinnerTrack = Color.FromArgb(232, 234, 237),
            .SpinnerArc = Color.FromArgb(26, 115, 232)
        }
        Public ReadOnly Dark As New ThemePalette With {
            .FormBack = Color.FromArgb(32, 33, 36),
            .PrimaryText = Color.FromArgb(232, 234, 236),
            .LabelText = Color.FromArgb(154, 160, 166),
            .StatusText = Color.FromArgb(232, 234, 236),
            .StatusError = Color.FromArgb(242, 139, 130),
            .GridBack = Color.FromArgb(32, 33, 36),
            .GridLine = Color.FromArgb(58, 60, 64),
            .HeaderBack = Color.FromArgb(44, 46, 49),
            .HeaderText = Color.FromArgb(232, 234, 236),
            .CellBack = Color.FromArgb(38, 40, 43),
            .CellText = Color.FromArgb(224, 227, 231),
            .SelectionBack = Color.FromArgb(36, 74, 133),
            .SelectionText = Color.White,
            .AlternatingRow = Color.FromArgb(34, 36, 39),
            .TextBoxBack = Color.FromArgb(48, 49, 52),
            .TextBoxText = Color.FromArgb(232, 234, 236),
            .SecondaryBack = Color.FromArgb(48, 49, 52),
            .SecondaryFore = Color.FromArgb(232, 234, 236),
            .SecondaryHover = Color.FromArgb(60, 64, 67),
            .SecondaryPressed = Color.FromArgb(72, 76, 80),
            .AppendBack = Color.FromArgb(26, 58, 110),
            .AppendFore = Color.FromArgb(138, 180, 248),
            .AppendHover = Color.FromArgb(36, 74, 140),
            .AppendPressed = Color.FromArgb(44, 88, 164),
            .SaveBack = Color.FromArgb(26, 115, 232),
            .SaveFore = Color.White,
            .SaveHover = Color.FromArgb(66, 133, 244),
            .SavePressed = Color.FromArgb(23, 78, 166),
            .DeleteBack = Color.FromArgb(60, 32, 32),
            .DeleteFore = Color.FromArgb(242, 139, 130),
            .DeleteHover = Color.FromArgb(78, 40, 40),
            .DeletePressed = Color.FromArgb(94, 48, 48),
            .SpinnerDisc = Color.FromArgb(48, 49, 52),
            .SpinnerTrack = Color.FromArgb(60, 64, 67),
            .SpinnerArc = Color.FromArgb(138, 180, 248)
        }

    End Module

    ''' <summary>Reads the Windows personalization theme ("Apps dark mode").</summary>
    Public Module SystemTheme

        Public Function IsDarkMode() As Boolean
            ' manual override for demos and tests: SHEETSDEMO_FORCE_DARK=1 or 0
            Dim forced As String = Environment.GetEnvironmentVariable("SHEETSDEMO_FORCE_DARK")
            If forced = "1" Then
                Return True
            End If
            If forced = "0" Then
                Return False
            End If

            ' Windows 10+: 0 = dark apps, 1 = light apps. Missing key means
            ' an older Windows that only has the light theme.
            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(
                    "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")
                If key IsNot Nothing Then
                    Dim value As Object = key.GetValue("AppsUseLightTheme")
                    If TypeOf value Is Integer Then
                        Return CInt(value) = 0
                    End If
                End If
            End Using
            Return False
        End Function

    End Module


End Namespace
