using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GoogleSheetsDemo.Controls
{
    /// <summary>
    /// Small rotating-arc spinner driven by a UI timer. Start()/Stop() toggle
    /// the animation. It paints a white disc with a light ring underneath so
    /// it stays readable while floating on top of the data grid.
    /// </summary>
    public class LoadingSpinner : Control
    {
        private readonly Timer _timer = new Timer();
        private float _angle;
        private Color _discColor = Color.White;
        private Color _trackColor = Color.FromArgb(232, 234, 237);
        private Color _arcColor = Color.FromArgb(26, 115, 232);

        /// <summary>Adjusts the spinner colors to the active theme.</summary>
        public void SetColors(Color disc, Color track, Color arc)
        {
            _discColor = disc;
            _trackColor = track;
            _arcColor = arc;
            Invalidate();
        }

        public LoadingSpinner()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            Size = new Size(28, 28);
            TabStop = false;
            _timer.Interval = 40;
            _timer.Tick += delegate
            {
                _angle = (_angle + 15f) % 360f;
                Invalidate();
            };
        }

        public void Start()
        {
            _angle = 0;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float outer = Math.Min(Width, Height) - 3f;
            var disc = new RectangleF(1.5f, 1.5f, outer, outer);
            var ring = new RectangleF(disc.X + 2.5f, disc.Y + 2.5f, outer - 5f, outer - 5f);

            using (var back = new SolidBrush(_discColor))
            {
                e.Graphics.FillEllipse(back, disc);
            }

            using (var track = new Pen(_trackColor, 3f))
            {
                e.Graphics.DrawEllipse(track, ring);
            }

            using (var arc = new Pen(_arcColor, 3f))
            {
                arc.StartCap = LineCap.Round;
                arc.EndCap = LineCap.Round;
                e.Graphics.DrawArc(arc, ring, _angle, 100f);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
