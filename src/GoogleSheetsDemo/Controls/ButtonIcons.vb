Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Controls

    Public Enum ButtonIconKind
        Refresh
        Plus
        Pencil
        Save
        Cross
        Trash
        ChevronLeft
        ChevronRight
        Sun
        Moon
    End Enum

    ''' <summary>
    ''' Draws the small monochrome glyphs shown in front of the button labels.
    ''' Each icon is generated in the color of the active theme palette, so
    ''' switching light/dark automatically re-colors the buttons.
    ''' </summary>
    Public Module ButtonIcons

        Public Function Create(kind As ButtonIconKind, color As Color) As Bitmap
            Dim bmp As New Bitmap(16, 16)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.SmoothingMode = SmoothingMode.AntiAlias
                Using pen As New Pen(color, 1.8F), fill As New SolidBrush(color)
                    pen.StartCap = LineCap.Round
                    pen.EndCap = LineCap.Round

                    Select Case kind
                        Case ButtonIconKind.Refresh
                            g.DrawArc(pen, 2.5F, 2.5F, 10.5F, 10.5F, -55.0F, 275.0F)
                            g.DrawLine(pen, 12.0F, 1.5F, 12.5F, 5.0F)
                            g.DrawLine(pen, 12.5F, 5.0F, 9.0F, 5.0F)

                        Case ButtonIconKind.Plus
                            g.DrawLine(pen, 8.0F, 3.0F, 8.0F, 13.0F)
                            g.DrawLine(pen, 3.0F, 8.0F, 13.0F, 8.0F)

                        Case ButtonIconKind.Pencil
                            g.DrawLine(pen, 3.5F, 12.5F, 11.0F, 5.0F)
                            g.DrawLine(pen, 9.5F, 2.5F, 13.5F, 6.5F)
                            g.DrawLine(pen, 3.0F, 13.0F, 5.5F, 10.5F)

                        Case ButtonIconKind.Save
                            g.DrawRectangle(pen, 2.5F, 2.5F, 10.5F, 10.5F)
                            g.DrawLine(pen, 5.5F, 3.0F, 5.5F, 6.5F)
                            g.DrawLine(pen, 10.0F, 3.0F, 10.0F, 6.5F)
                            g.DrawLine(pen, 5.5F, 3.0F, 10.0F, 3.0F)
                            g.DrawRectangle(pen, 5.5F, 9.0F, 4.5F, 4.0F)

                        Case ButtonIconKind.Cross
                            g.DrawLine(pen, 4.0F, 4.0F, 12.0F, 12.0F)
                            g.DrawLine(pen, 12.0F, 4.0F, 4.0F, 12.0F)

                        Case ButtonIconKind.Trash
                            g.DrawLine(pen, 2.5F, 4.5F, 13.5F, 4.5F)
                            g.DrawLine(pen, 6.0F, 2.5F, 6.0F, 4.5F)
                            g.DrawLine(pen, 10.0F, 2.5F, 10.0F, 4.5F)
                            g.DrawLine(pen, 6.0F, 2.5F, 10.0F, 2.5F)
                            g.DrawRectangle(pen, 4.5F, 4.5F, 7.0F, 9.0F)
                            g.DrawLine(pen, 6.7F, 7.0F, 6.7F, 11.5F)
                            g.DrawLine(pen, 9.3F, 7.0F, 9.3F, 11.5F)

                        Case ButtonIconKind.ChevronLeft
                            g.DrawLine(pen, 10.0F, 3.0F, 5.0F, 8.0F)
                            g.DrawLine(pen, 5.0F, 8.0F, 10.0F, 13.0F)

                        Case ButtonIconKind.ChevronRight
                            g.DrawLine(pen, 6.0F, 3.0F, 11.0F, 8.0F)
                            g.DrawLine(pen, 11.0F, 8.0F, 6.0F, 13.0F)

                        Case ButtonIconKind.Sun
                            g.FillEllipse(fill, 5.0F, 5.0F, 6.0F, 6.0F)
                            For i As Integer = 0 To 7
                                Dim a As Double = i * Math.PI / 4.0
                                Dim x1 As Single = 8.0F + CSng(Math.Cos(a)) * 5.2F
                                Dim y1 As Single = 8.0F + CSng(Math.Sin(a)) * 5.2F
                                Dim x2 As Single = 8.0F + CSng(Math.Cos(a)) * 7.4F
                                Dim y2 As Single = 8.0F + CSng(Math.Sin(a)) * 7.4F
                                g.DrawLine(pen, x1, y1, x2, y2)
                            Next

                        Case ButtonIconKind.Moon
                            g.FillEllipse(fill, 2.5F, 2.5F, 11.0F, 11.0F)
                            g.CompositingMode = CompositingMode.SourceCopy
                            Using eraseBrush As New SolidBrush(Color.Transparent)
                                g.FillEllipse(eraseBrush, 6.5F, 1.0F, 11.0F, 11.0F)
                            End Using
                    End Select
                End Using
            End Using
            Return bmp
        End Function

    End Module


End Namespace
