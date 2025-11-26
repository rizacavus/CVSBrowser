namespace WinFormsApp1
{
    public enum ThemeMode
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        private static ThemeMode _currentTheme = ThemeMode.Light;
        public static event Action<ThemeMode>? ThemeChanged;

        public static ThemeMode CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(value);
                }
            }
        }

        // Light Theme Colors
        public static readonly ThemeColors LightTheme = new()
        {
            BackgroundColor = Color.White,
            ForegroundColor = Color.Black,
            ButtonBackColor = Color.FromArgb(240, 240, 240),
            ButtonForeColor = Color.Black,
            MenuBackColor = Color.White,
            MenuForeColor = Color.Black,
            TextBoxBackColor = Color.White,
            TextBoxForeColor = Color.Black,
            TabControlBackColor = Color.White,
            TabPageBackColor = Color.White,
            StatusStripBackColor = Color.FromArgb(240, 240, 240),
            StatusStripForeColor = Color.Black,
            BorderColor = Color.FromArgb(200, 200, 200),
            SelectedTabColor = Color.FromArgb(0, 120, 215),
            HoverColor = Color.FromArgb(229, 243, 255)
        };

        // Dark Theme Colors
        public static readonly ThemeColors DarkTheme = new()
        {
            BackgroundColor = Color.FromArgb(45, 45, 48),
            ForegroundColor = Color.White,
            ButtonBackColor = Color.FromArgb(62, 62, 66),
            ButtonForeColor = Color.White,
            MenuBackColor = Color.FromArgb(45, 45, 48),
            MenuForeColor = Color.White,
            TextBoxBackColor = Color.FromArgb(51, 51, 55),
            TextBoxForeColor = Color.White,
            TabControlBackColor = Color.FromArgb(37, 37, 38),
            TabPageBackColor = Color.FromArgb(45, 45, 48),
            StatusStripBackColor = Color.FromArgb(37, 37, 38),
            StatusStripForeColor = Color.White,
            BorderColor = Color.FromArgb(63, 63, 70),
            SelectedTabColor = Color.FromArgb(0, 122, 204),
            HoverColor = Color.FromArgb(62, 62, 66)
        };

        public static ThemeColors GetCurrentTheme()
        {
            return CurrentTheme == ThemeMode.Dark ? DarkTheme : LightTheme;
        }

        public static void ApplyTheme(Form form)
        {
            var theme = GetCurrentTheme();
            ApplyThemeToControl(form, theme);
        }

        public static void ApplyTheme(ContextMenuStrip contextMenu)
        {
            var theme = GetCurrentTheme();
            ApplyThemeToContextMenu(contextMenu, theme);
        }

        private static void ApplyThemeToContextMenu(ContextMenuStrip contextMenu, ThemeColors theme)
        {
            contextMenu.BackColor = theme.MenuBackColor;
            contextMenu.ForeColor = theme.MenuForeColor;
            contextMenu.Renderer = new DarkContextMenuRenderer(theme);

            foreach (ToolStripItem item in contextMenu.Items)
            {
                ApplyThemeToToolStripItem(item, theme);
            }
        }

        private static void ApplyThemeToControl(Control control, ThemeColors theme)
        {
            // Apply theme based on control type
            switch (control)
            {
                case Form form:
                    form.BackColor = theme.BackgroundColor;
                    form.ForeColor = theme.ForegroundColor;
                    break;

                case Button button:
                    button.BackColor = theme.ButtonBackColor;
                    button.ForeColor = theme.ButtonForeColor;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = theme.BorderColor;
                    break;

                case TextBox textBox:
                    textBox.BackColor = theme.TextBoxBackColor;
                    textBox.ForeColor = theme.TextBoxForeColor;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case Label label:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = theme.ForegroundColor;
                    break;

                case MenuStrip menuStrip:
                    menuStrip.BackColor = theme.MenuBackColor;
                    menuStrip.ForeColor = theme.MenuForeColor;
                    menuStrip.Renderer = new DarkMenuStripRenderer(theme);
                    break;

                case StatusStrip statusStrip:
                    statusStrip.BackColor = theme.StatusStripBackColor;
                    statusStrip.ForeColor = theme.StatusStripForeColor;
                    statusStrip.Renderer = new DarkStatusStripRenderer(theme);
                    break;

                case TabControl tabControl:
                    tabControl.BackColor = theme.TabControlBackColor;
                    tabControl.ForeColor = theme.ForegroundColor;
                    break;

                case TabPage tabPage:
                    tabPage.BackColor = theme.TabPageBackColor;
                    tabPage.ForeColor = theme.ForegroundColor;
                    break;

                case ListView listView:
                    listView.BackColor = theme.BackgroundColor;
                    listView.ForeColor = theme.ForegroundColor;
                    break;

                case Panel panel:
                    panel.BackColor = theme.BackgroundColor;
                    break;

                case ProgressBar progressBar:
                    progressBar.BackColor = theme.BackgroundColor;
                    break;

                case CheckBox checkBox:
                    checkBox.BackColor = Color.Transparent;
                    checkBox.ForeColor = theme.ForegroundColor;
                    break;
            }

            // Recursively apply theme to child controls
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child, theme);
            }

            // Handle ToolStripItems separately
            if (control is ToolStrip toolStrip)
            {
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    ApplyThemeToToolStripItem(item, theme);
                }
            }
        }

        private static void ApplyThemeToToolStripItem(ToolStripItem item, ThemeColors theme)
        {
            item.BackColor = theme.MenuBackColor;
            item.ForeColor = theme.MenuForeColor;

            if (item is ToolStripMenuItem menuItem)
            {
                foreach (ToolStripItem dropDownItem in menuItem.DropDownItems)
                {
                    ApplyThemeToToolStripItem(dropDownItem, theme);
                }
            }
        }
    }

    public class ThemeColors
    {
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color ButtonBackColor { get; set; }
        public Color ButtonForeColor { get; set; }
        public Color MenuBackColor { get; set; }
        public Color MenuForeColor { get; set; }
        public Color TextBoxBackColor { get; set; }
        public Color TextBoxForeColor { get; set; }
        public Color TabControlBackColor { get; set; }
        public Color TabPageBackColor { get; set; }
        public Color StatusStripBackColor { get; set; }
        public Color StatusStripForeColor { get; set; }
        public Color BorderColor { get; set; }
        public Color SelectedTabColor { get; set; }
        public Color HoverColor { get; set; }
    }

    public class DarkContextMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly ThemeColors _theme;

        public DarkContextMenuRenderer(ThemeColors theme)
        {
            _theme = theme;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using var brush = new SolidBrush(_theme.HoverColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
            else
            {
                using var brush = new SolidBrush(_theme.MenuBackColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(_theme.MenuBackColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(_theme.BorderColor);
            e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var pen = new Pen(_theme.BorderColor);
            var bounds = e.Item.Bounds;
            e.Graphics.DrawLine(pen, bounds.Left + 2, bounds.Height / 2, bounds.Right - 2, bounds.Height / 2);
        }
    }

    public class DarkMenuStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly ThemeColors _theme;

        public DarkMenuStripRenderer(ThemeColors theme)
        {
            _theme = theme;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using var brush = new SolidBrush(_theme.HoverColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
            else
            {
                using var brush = new SolidBrush(_theme.MenuBackColor);
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(_theme.MenuBackColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
    }

    public class DarkStatusStripRenderer : ToolStripProfessionalRenderer
    {
        private readonly ThemeColors _theme;

        public DarkStatusStripRenderer(ThemeColors theme)
        {
            _theme = theme;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(_theme.StatusStripBackColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
    }
}