using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GoogleSheetsDemo.Controls
{
    public enum ButtonIconKind
    {
        Refresh,
        Plus,
        Pencil,
        Save,
        Cross,
        Trash,
        ChevronLeft,
        ChevronRight,
        Sun,
        Moon
    }

    /// <summary>
    /// Draws the small monochrome glyphs shown in front of the button labels.
    /// Each icon is generated in the color of the active theme palette, so
    /// switching light/dark automatically re-colors the buttons.
    /// </summary>
    public static class ButtonIcons
    {
        public static Bitmap Create(ButtonIconKind kind, Color color)
        {
            var bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(color, 1.8f))
                using (SolidBrush fill = new SolidBrush(color))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    switch (kind)
                    {
                        case ButtonIconKind.Refresh:
                            g.DrawArc(pen, 2.5f, 2.5f, 10.5f, 10.5f, -55f, 275f);
                            g.DrawLine(pen, 12f, 1.5f, 12.5f, 5f);
                            g.DrawLine(pen, 12.5f, 5f, 9f, 5f);
                            break;

                        case ButtonIconKind.Plus:
                            g.DrawLine(pen, 8f, 3f, 8f, 13f);
                            g.DrawLine(pen, 3f, 8f, 13f, 8f);
                            break;

                        case ButtonIconKind.Pencil:
                            g.DrawLine(pen, 3.5f, 12.5f, 11f, 5f);
                            g.DrawLine(pen, 9.5f, 2.5f, 13.5f, 6.5f);
                            g.DrawLine(pen, 3f, 13f, 5.5f, 10.5f);
                            break;

                        case ButtonIconKind.Save:
                            g.DrawRectangle(pen, 2.5f, 2.5f, 10.5f, 10.5f);
                            g.DrawLine(pen, 5.5f, 3f, 5.5f, 6.5f);
                            g.DrawLine(pen, 10f, 3f, 10f, 6.5f);
                            g.DrawLine(pen, 5.5f, 3f, 10f, 3f);
                            g.DrawRectangle(pen, 5.5f, 9f, 4.5f, 4f);
                            break;

                        case ButtonIconKind.Cross:
                            g.DrawLine(pen, 4f, 4f, 12f, 12f);
                            g.DrawLine(pen, 12f, 4f, 4f, 12f);
                            break;

                        case ButtonIconKind.Trash:
                            g.DrawLine(pen, 2.5f, 4.5f, 13.5f, 4.5f);
                            g.DrawLine(pen, 6f, 2.5f, 6f, 4.5f);
                            g.DrawLine(pen, 10f, 2.5f, 10f, 4.5f);
                            g.DrawLine(pen, 6f, 2.5f, 10f, 2.5f);
                            g.DrawRectangle(pen, 4.5f, 4.5f, 7f, 9f);
                            g.DrawLine(pen, 6.7f, 7f, 6.7f, 11.5f);
                            g.DrawLine(pen, 9.3f, 7f, 9.3f, 11.5f);
                            break;

                        case ButtonIconKind.ChevronLeft:
                            g.DrawLine(pen, 10f, 3f, 5f, 8f);
                            g.DrawLine(pen, 5f, 8f, 10f, 13f);
                            break;

                        case ButtonIconKind.ChevronRight:
                            g.DrawLine(pen, 6f, 3f, 11f, 8f);
                            g.DrawLine(pen, 11f, 8f, 6f, 13f);
                            break;

                        case ButtonIconKind.Sun:
                            g.FillEllipse(fill, 5f, 5f, 6f, 6f);
                            for (int i = 0; i < 8; i++)
                            {
                                double a = i * Math.PI / 4.0;
                                float x1 = 8f + (float)Math.Cos(a) * 5.2f;
                                float y1 = 8f + (float)Math.Sin(a) * 5.2f;
                                float x2 = 8f + (float)Math.Cos(a) * 7.4f;
                                float y2 = 8f + (float)Math.Sin(a) * 7.4f;
                                g.DrawLine(pen, x1, y1, x2, y2);
                            }
                            break;

                        case ButtonIconKind.Moon:
                            g.FillEllipse(fill, 2.5f, 2.5f, 11f, 11f);
                            g.CompositingMode = CompositingMode.SourceCopy;
                            using (SolidBrush erase = new SolidBrush(Color.Transparent))
                            {
                                g.FillEllipse(erase, 6.5f, 1f, 11f, 11f);
                            }
                            break;
                    }
                }
            }
            return bmp;
        }
    }
}
