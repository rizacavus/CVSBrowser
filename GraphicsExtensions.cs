using System.Drawing.Drawing2D;

namespace WinFormsApp1
{
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = CreateRoundedRectanglePath(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using (var path = CreateRoundedRectanglePath(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            // Top-left arc
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Top-right arc  
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Bottom-right arc
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Bottom-left arc
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}