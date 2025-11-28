using System.Runtime.InteropServices;
using WinFormsApp1;
using System.Drawing.Drawing2D;

namespace CVSBrowser
{
    public class ChromeTabControl : TabControl
    {
        private int hoveredTabIndex = -1;
        private const string NEW_TAB_TEXT = "+";
        private const int NEW_TAB_WIDTH = 40; // Make new tab button smaller
        private const int REGULAR_TAB_WIDTH = 200;
        private const int TAB_HEIGHT = 32;

        public event EventHandler? NewTabRequested;

        public ChromeTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw |
                    ControlStyles.Selectable | ControlStyles.StandardClick |
                    ControlStyles.StandardDoubleClick | ControlStyles.OptimizedDoubleBuffer, true);
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(REGULAR_TAB_WIDTH, TAB_HEIGHT); // Default size for regular tabs
            TabStop = true;
        }

        // Override GetTabRect to provide custom sizing for the new tab button
        public Rectangle GetTabRect(int index)
        {
            if (index < 0 || index >= TabCount)
                return Rectangle.Empty;

            int x = 0;
            int y = 2;

            // Calculate position based on previous tabs
            for (int i = 0; i < index; i++)
            {
                bool isNewTab = TabPages[i].Text == NEW_TAB_TEXT;
                x += isNewTab ? NEW_TAB_WIDTH : REGULAR_TAB_WIDTH;
            }

            // Determine width for current tab
            bool isCurrentTabNewTab = TabPages[index].Text == NEW_TAB_TEXT;
            int width = isCurrentTabNewTab ? NEW_TAB_WIDTH : REGULAR_TAB_WIDTH;

            return new Rectangle(x, y, width, TAB_HEIGHT);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Check if clicking on the new tab (last tab)
            if (TabCount > 0)
            {
                var lastTabIndex = TabCount - 1;
                var lastTabRect = GetTabRect(lastTabIndex);

                if (lastTabRect.Contains(e.Location) && e.Button == MouseButtons.Left)
                {
                    var lastTab = TabPages[lastTabIndex];
                    if (lastTab.Text == NEW_TAB_TEXT)
                    {
                        System.Diagnostics.Debug.WriteLine("New tab clicked! Firing event.");
                        NewTabRequested?.Invoke(this, EventArgs.Empty);
                        return; // Don't call base to prevent tab selection
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Check tab hover
            int newHoveredIndex = -1;
            for (int i = 0; i < TabCount; i++)
            {
                if (GetTabRect(i).Contains(e.Location))
                {
                    newHoveredIndex = i;
                    Cursor = Cursors.Hand;
                    break;
                }
            }

            if (newHoveredIndex == -1)
            {
                Cursor = Cursors.Default;
            }

            if (newHoveredIndex != hoveredTabIndex)
            {
                hoveredTabIndex = newHoveredIndex;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            bool needsRedraw = hoveredTabIndex != -1;
            hoveredTabIndex = -1;
            Cursor = Cursors.Default;

            if (needsRedraw)
            {
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Paint background
            e.Graphics.Clear(Color.FromArgb(51, 51, 55));

            // Draw all tabs
            for (int i = 0; i < TabCount; i++)
            {
                DrawTab(e.Graphics, TabPages[i], i);
            }
        }

        private void DrawTab(Graphics g, TabPage tabPage, int index)
        {
            Rectangle tabRect = GetTabRect(index);
            bool isSelected = SelectedIndex == index;
            bool isHovered = index == hoveredTabIndex;
            bool isNewTabButton = tabPage.Text == NEW_TAB_TEXT;

            // Chrome-style rounded tab shape
            using (var path = CreateTabPath(tabRect, isSelected && !isNewTabButton))
            {
                // Tab background - new tab button has different colors
                Color tabColor;
                if (isNewTabButton)
                {
                    tabColor = isHovered ? Color.FromArgb(70, 70, 74) : Color.FromArgb(60, 60, 64);
                }
                else
                {
                    tabColor = isSelected ? Color.FromArgb(37, 37, 38) :
                               isHovered ? Color.FromArgb(62, 62, 66) : Color.FromArgb(51, 51, 55);
                }

                using (var brush = new SolidBrush(tabColor))
                {
                    g.FillPath(brush, path);
                }

                // Tab border for selected tab (not for new tab button)
                if (isSelected && !isNewTabButton)
                {
                    using (var pen = new Pen(Color.FromArgb(63, 63, 70), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else if (isNewTabButton)
                {
                    // Draw border for new tab button
                    var borderColor = isHovered ? Color.FromArgb(90, 90, 94) : Color.FromArgb(80, 80, 84);
                    using (var pen = new Pen(borderColor, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            if (isNewTabButton)
            {
                // Draw plus sign for new tab button - smaller font for smaller button
                using (var textBrush = new SolidBrush(Color.White))
                using (var font = new Font("Segoe UI", 12F, FontStyle.Bold)) // Reduced from 14F
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("+", font, textBrush, tabRect, format);
                }
            }
            else
            {
                // Regular tab - draw text and close button
                string text = tabPage.Text.Length > 25 ? tabPage.Text[..22] + "..." : tabPage.Text;
                var textRect = new Rectangle(tabRect.X + 16, tabRect.Y + 8,
                    Math.Max(1, tabRect.Width - 48), tabRect.Height);

                using (var textBrush = new SolidBrush(Color.White))
                using (var font = new Font("Segoe UI", 9F))
                {
                    var textFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    g.DrawString(text, font, textBrush, textRect, textFormat);
                }

                // Close button (X) - only for regular tabs
                var closeRect = new Rectangle(tabRect.Right - 20, tabRect.Y + 10, 12, 12);
                using (var closeBrush = new SolidBrush(isHovered ?
                    Color.FromArgb(220, 220, 220) : Color.FromArgb(180, 180, 180)))
                using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    g.DrawString("×", font, closeBrush, closeRect);
                }
            }
        }

        private GraphicsPath CreateTabPath(Rectangle rect, bool isSelected)
        {
            var path = new GraphicsPath();
            int radius = 8;

            if (isSelected)
            {
                // Selected tab - full rounded rectangle
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom + 50);
                path.AddLine(rect.Right, rect.Bottom + 50, rect.X, rect.Bottom + 50);
                path.AddLine(rect.X, rect.Bottom + 50, rect.X, rect.Y + radius);
            }
            else
            {
                // Non-selected tab - simple rounded top
                path.AddArc(rect.X + 4, rect.Y + 4, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius - 4, rect.Y + 4, radius, radius, 270, 90);
                path.AddLine(rect.Right - 4, rect.Y + 8, rect.Right - 4, rect.Bottom);
                path.AddLine(rect.Right - 4, rect.Bottom, rect.X + 4, rect.Bottom);
                path.AddLine(rect.X + 4, rect.Bottom, rect.X + 4, rect.Y + 8);
            }

            path.CloseFigure();
            return path;
        }

        // Public method to add the new tab button
        public void EnsureNewTabButton()
        {
            // Check if we already have a new tab button
            if (TabCount == 0 || TabPages[TabCount - 1].Text != NEW_TAB_TEXT)
            {
                var newTabPage = new TabPage(NEW_TAB_TEXT)
                {
                    BackColor = Color.FromArgb(37, 37, 38)
                };
                TabPages.Add(newTabPage);
                Invalidate(); // Redraw to apply new sizing
            }
        }

        // Public method to remove the new tab button (if needed)
        public void RemoveNewTabButton()
        {
            if (TabCount > 0 && TabPages[TabCount - 1].Text == NEW_TAB_TEXT)
            {
                TabPages.RemoveAt(TabCount - 1);
                Invalidate(); // Redraw to apply new sizing
            }
        }

        // Windows API constants for dragging
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
    }
}