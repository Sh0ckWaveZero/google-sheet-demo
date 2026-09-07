Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Namespace Controls

    ''' <summary>
    ''' Small rotating-arc spinner driven by a UI timer. Start()/Stop() toggle
    ''' the animation. It paints a white disc with a light ring underneath so
    ''' it stays readable while floating on top of the data grid.
    ''' </summary>
    Public Class LoadingSpinner
        Inherits Control

        Private ReadOnly _timer As New Timer()
        Private _angle As Single
        Private _discColor As Color = Color.White
        Private _trackColor As Color = Color.FromArgb(232, 234, 237)
        Private _arcColor As Color = Color.FromArgb(26, 115, 232)

        ''' <summary>Adjusts the spinner colors to the active theme.</summary>
        Public Sub SetColors(disc As Color, track As Color, arc As Color)
            _discColor = disc
            _trackColor = track
            _arcColor = arc
            Invalidate()
        End Sub

        Public Sub New()
            SetStyle(
                ControlStyles.UserPaint Or
                ControlStyles.AllPaintingInWmPaint Or
                ControlStyles.OptimizedDoubleBuffer Or
                ControlStyles.ResizeRedraw,
                True)

            Size = New Size(28, 28)
            TabStop = False
            _timer.Interval = 40
            AddHandler _timer.Tick,
                Sub()
                    _angle = (_angle + 15.0F) Mod 360.0F
                    Invalidate()
                End Sub
        End Sub

        Public Sub Start()
            _angle = 0
            _timer.Start()
        End Sub

        Public Sub [Stop]()
            _timer.Stop()
            Invalidate()
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

            Dim outer As Single = Math.Min(Width, Height) - 3.0F
            Dim disc As New RectangleF(1.5F, 1.5F, outer, outer)
            Dim ring As New RectangleF(disc.X + 2.5F, disc.Y + 2.5F, outer - 5.0F, outer - 5.0F)

            Using back As New SolidBrush(_discColor)
                e.Graphics.FillEllipse(back, disc)
            End Using

            Using track As New Pen(_trackColor, 3.0F)
                e.Graphics.DrawEllipse(track, ring)
            End Using

            Using arc As New Pen(_arcColor, 3.0F)
                arc.StartCap = LineCap.Round
                arc.EndCap = LineCap.Round
                e.Graphics.DrawArc(arc, ring, _angle, 100.0F)
            End Using
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                _timer.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

    End Class


End Namespace
