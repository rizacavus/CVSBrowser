using CVSBrowser;
using System.Runtime.InteropServices;

namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Panel tabStripPanel;
        private ChromeTabControl chromeTabControl;
        private Panel navigationPanel;
        private Panel webContentPanel;
        private TextBox addressBar;
        private Button backButton;
        private Button forwardButton;
        private Button refreshButton;
        private Button homeButton;
        private Button bookmarkButton;
        private Button settingsButton;
        private Button closeButton;
        private Button maximizeButton;
        private Button minimizeButton;
        private Button darkModeExtensionButton;
        private Button adBlockerButton;
        private ProgressBar progressBar;
        private Panel addressBarBorder;
        private Panel addressBarContainer;

        // Windows API for custom window frame and resizing
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_SYSMENU = 0x00080000;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_FRAMECHANGED = 0x0020;

        // Constants for custom resize
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int WM_NCHITTEST = 0x0084;
        private const int HT_CAPTION = 0x2;
        private const int HT_LEFT = 10;
        private const int HT_RIGHT = 11;
        private const int HT_TOP = 12;
        private const int HT_TOPLEFT = 13;
        private const int HT_TOPRIGHT = 14;
        private const int HT_BOTTOM = 15;
        private const int HT_BOTTOMLEFT = 16;
        private const int HT_BOTTOMRIGHT = 17;

        private const int ResizeBorderWidth = 2; // Made thinner (was 5)

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabStripPanel = new Panel();
            chromeTabControl = new ChromeTabControl();
            minimizeButton = new Button();
            maximizeButton = new Button();
            closeButton = new Button();
            navigationPanel = new Panel();
            settingsButton = new Button();
            bookmarkButton = new Button();
            adBlockerButton = new Button();
            darkModeExtensionButton = new Button();
            addressBarBorder = new Panel();
            addressBarContainer = new Panel();
            addressBar = new TextBox();
            homeButton = new Button();
            refreshButton = new Button();
            forwardButton = new Button();
            backButton = new Button();
            progressBar = new ProgressBar();
            webContentPanel = new Panel();
            tabStripPanel.SuspendLayout();
            navigationPanel.SuspendLayout();
            addressBarBorder.SuspendLayout();
            addressBarContainer.SuspendLayout();
            SuspendLayout();
            // 
            // tabStripPanel
            // 
            tabStripPanel.BackColor = Color.FromArgb(51, 51, 55);
            tabStripPanel.Controls.Add(chromeTabControl);
            tabStripPanel.Controls.Add(minimizeButton);
            tabStripPanel.Controls.Add(maximizeButton);
            tabStripPanel.Controls.Add(closeButton);
            tabStripPanel.Dock = DockStyle.Top;
            tabStripPanel.Location = new Point(0, 0);
            tabStripPanel.Name = "tabStripPanel";
            tabStripPanel.Size = new Size(1200, 36);
            tabStripPanel.TabIndex = 3;
            tabStripPanel.MouseDown += tabStripPanel_MouseDown;
            tabStripPanel.MouseMove += tabStripPanel_MouseMove;
            tabStripPanel.MouseUp += tabStripPanel_MouseUp;
            tabStripPanel.DoubleClick += tabStripPanel_DoubleClick;
            // 
            // chromeTabControl
            // 
            chromeTabControl.Appearance = TabAppearance.FlatButtons;
            chromeTabControl.Dock = DockStyle.Fill;
            chromeTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            chromeTabControl.ItemSize = new Size(200, 32);
            chromeTabControl.Location = new Point(0, 0);
            chromeTabControl.Name = "chromeTabControl";
            chromeTabControl.SelectedIndex = 0;
            chromeTabControl.Size = new Size(1065, 36);
            chromeTabControl.SizeMode = TabSizeMode.Fixed;
            chromeTabControl.TabIndex = 0;
            chromeTabControl.DrawItem += ChromeTabControl_DrawItem;
            chromeTabControl.SelectedIndexChanged += ChromeTabControl_SelectedIndexChanged;
            chromeTabControl.MouseClick += ChromeTabControl_MouseClick;
            // 
            // minimizeButton
            // 
            minimizeButton.BackColor = Color.Transparent;
            minimizeButton.Dock = DockStyle.Right;
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.FlatStyle = FlatStyle.Flat;
            minimizeButton.Font = new Font("Segoe UI", 10F);
            minimizeButton.ForeColor = Color.White;
            minimizeButton.Location = new Point(1065, 0);
            minimizeButton.Name = "minimizeButton";
            minimizeButton.Size = new Size(45, 36);
            minimizeButton.TabIndex = 1;
            minimizeButton.Text = "🗕";
            minimizeButton.UseVisualStyleBackColor = false;
            minimizeButton.Click += MinimizeButton_Click;
            minimizeButton.MouseEnter += ButtonMouseEnter;
            minimizeButton.MouseLeave += ButtonMouseLeave;
            // 
            // maximizeButton
            // 
            maximizeButton.BackColor = Color.Transparent;
            maximizeButton.Dock = DockStyle.Right;
            maximizeButton.FlatAppearance.BorderSize = 0;
            maximizeButton.FlatStyle = FlatStyle.Flat;
            maximizeButton.Font = new Font("Segoe UI", 10F);
            maximizeButton.ForeColor = Color.White;
            maximizeButton.Location = new Point(1110, 0);
            maximizeButton.Name = "maximizeButton";
            maximizeButton.Size = new Size(45, 36);
            maximizeButton.TabIndex = 2;
            maximizeButton.Text = "🗖";
            maximizeButton.UseVisualStyleBackColor = false;
            maximizeButton.Click += MaximizeButton_Click;
            maximizeButton.MouseEnter += ButtonMouseEnter;
            maximizeButton.MouseLeave += ButtonMouseLeave;
            // 
            // closeButton
            // 
            closeButton.BackColor = Color.Transparent;
            closeButton.Dock = DockStyle.Right;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 10F);
            closeButton.ForeColor = Color.White;
            closeButton.Location = new Point(1155, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(45, 36);
            closeButton.TabIndex = 3;
            closeButton.Text = "✕";
            closeButton.UseVisualStyleBackColor = false;
            closeButton.Click += CloseButton_Click;
            closeButton.MouseEnter += closeButtonMouseEnter;
            closeButton.MouseLeave += ButtonMouseLeave;
            // 
            // navigationPanel
            // 
            navigationPanel.BackColor = Color.FromArgb(51, 51, 55);
            navigationPanel.Controls.Add(settingsButton);
            navigationPanel.Controls.Add(bookmarkButton);
            navigationPanel.Controls.Add(adBlockerButton);
            navigationPanel.Controls.Add(darkModeExtensionButton);
            navigationPanel.Controls.Add(addressBarBorder);
            navigationPanel.Controls.Add(homeButton);
            navigationPanel.Controls.Add(refreshButton);
            navigationPanel.Controls.Add(forwardButton);
            navigationPanel.Controls.Add(backButton);
            navigationPanel.Dock = DockStyle.Top;
            navigationPanel.Location = new Point(0, 36);
            navigationPanel.Name = "navigationPanel";
            navigationPanel.Padding = new Padding(8, 9, 8, 9);
            navigationPanel.Size = new Size(1200, 50);
            navigationPanel.TabIndex = 2;
            navigationPanel.MouseDown += EnableDragging;
            // 
            // settingsButton
            // 
            settingsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            settingsButton.BackColor = Color.Transparent;
            settingsButton.FlatAppearance.BorderSize = 0;
            settingsButton.FlatStyle = FlatStyle.Flat;
            settingsButton.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            settingsButton.ForeColor = Color.White;
            settingsButton.Location = new Point(ClientSize.Width - 42, 9);
            settingsButton.Name = "settingsButton";
            settingsButton.Size = new Size(32, 32);
            settingsButton.TabIndex = 8;
            settingsButton.Text = "⋮";
            settingsButton.UseVisualStyleBackColor = false;
            settingsButton.Click += settingsButton_Click;
            SetupButtonHover(settingsButton);
            // 
            // bookmarkButton
            // 
            bookmarkButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            bookmarkButton.BackColor = Color.Transparent;
            bookmarkButton.FlatAppearance.BorderSize = 0;
            bookmarkButton.FlatStyle = FlatStyle.Flat;
            bookmarkButton.Font = new Font("Segoe UI", 12F);
            bookmarkButton.ForeColor = Color.White;
            bookmarkButton.Location = new Point(ClientSize.Width - 80, 9);
            bookmarkButton.Name = "bookmarkButton";
            bookmarkButton.Size = new Size(32, 32);
            bookmarkButton.TabIndex = 7;
            bookmarkButton.Text = "★";
            bookmarkButton.UseVisualStyleBackColor = false;
            bookmarkButton.Click += bookmarkButton_Click;
            SetupButtonHover(bookmarkButton);
            // 
            // adBlockerButton
            // 
            adBlockerButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            adBlockerButton.BackColor = Color.Transparent;
            adBlockerButton.FlatAppearance.BorderSize = 0;
            adBlockerButton.FlatStyle = FlatStyle.Flat;
            adBlockerButton.Font = new Font("Segoe UI", 12F);
            adBlockerButton.ForeColor = Color.White;
            adBlockerButton.Location = new Point(ClientSize.Width - 118, 9);
            adBlockerButton.Name = "adBlockerButton";
            adBlockerButton.Size = new Size(32, 32);
            adBlockerButton.TabIndex = 6;
            adBlockerButton.Text = "🛡️";
            adBlockerButton.UseVisualStyleBackColor = false;
            adBlockerButton.Click += AdBlockerButton_Click;
            SetupButtonHover(adBlockerButton);
            // 
            // darkModeExtensionButton
            // 
            darkModeExtensionButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            darkModeExtensionButton.BackColor = Color.Transparent;
            darkModeExtensionButton.FlatAppearance.BorderSize = 0;
            darkModeExtensionButton.FlatStyle = FlatStyle.Flat;
            darkModeExtensionButton.Font = new Font("Segoe UI", 12F);
            darkModeExtensionButton.ForeColor = Color.White;
            darkModeExtensionButton.Location = new Point(ClientSize.Width - 156, 9);
            darkModeExtensionButton.Name = "darkModeExtensionButton";
            darkModeExtensionButton.Size = new Size(32, 32);
            darkModeExtensionButton.TabIndex = 5;
            darkModeExtensionButton.Text = "🌙";
            darkModeExtensionButton.UseVisualStyleBackColor = false;
            darkModeExtensionButton.Click += darkModeExtensionButton_Click;
            SetupButtonHover(darkModeExtensionButton);
            // 
            // addressBarBorder
            // 
            addressBarBorder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addressBarBorder.BackColor = Color.FromArgb(63, 63, 70);
            addressBarBorder.Controls.Add(addressBarContainer);
            addressBarBorder.Location = new Point(160, 7);
            addressBarBorder.Name = "addressBarBorder";
            addressBarBorder.Size = new Size(ClientSize.Width - 328, 36);
            addressBarBorder.TabIndex = 4;
            // 
            // addressBarContainer
            // 
            addressBarContainer.BackColor = Color.FromArgb(37, 37, 38);
            addressBarContainer.Controls.Add(addressBar);
            addressBarContainer.Dock = DockStyle.Fill;
            addressBarContainer.Location = new Point(0, 0);
            addressBarContainer.Margin = new Padding(1);
            addressBarContainer.Name = "addressBarContainer";
            addressBarContainer.Padding = new Padding(8);
            addressBarContainer.Size = new Size(ClientSize.Width - 328, 36);
            addressBarContainer.TabIndex = 0;
            // 
            // addressBar
            // 
            addressBar.BackColor = Color.FromArgb(37, 37, 38);
            addressBar.BorderStyle = BorderStyle.None;
            addressBar.Dock = DockStyle.Fill;
            addressBar.Font = new Font("Segoe UI", 11F);
            addressBar.ForeColor = Color.White;
            addressBar.Location = new Point(8, 8);
            addressBar.Name = "addressBar";
            addressBar.Size = new Size(ClientSize.Width - 344, 20);
            addressBar.TabIndex = 0;
            addressBar.KeyDown += addressBar_KeyDown;
            // 
            // homeButton
            // 
            homeButton.BackColor = Color.Transparent;
            homeButton.FlatAppearance.BorderSize = 0;
            homeButton.FlatStyle = FlatStyle.Flat;
            homeButton.Font = new Font("Segoe UI", 10F);
            homeButton.ForeColor = Color.White;
            homeButton.Location = new Point(122, 9);
            homeButton.Name = "homeButton";
            homeButton.Size = new Size(32, 32);
            homeButton.TabIndex = 3;
            homeButton.Text = "🏠";
            homeButton.UseVisualStyleBackColor = false;
            homeButton.Click += homeButton_Click;
            SetupButtonHover(homeButton);
            // 
            // refreshButton
            // 
            refreshButton.BackColor = Color.Transparent;
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Font = new Font("Segoe UI", 12F);
            refreshButton.ForeColor = Color.White;
            refreshButton.Location = new Point(84, 9);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(32, 32);
            refreshButton.TabIndex = 2;
            refreshButton.Text = "⟳";
            refreshButton.UseVisualStyleBackColor = false;
            refreshButton.Click += refreshButton_Click;
            SetupButtonHover(refreshButton);
            // 
            // forwardButton
            // 
            forwardButton.BackColor = Color.Transparent;
            forwardButton.FlatAppearance.BorderSize = 0;
            forwardButton.FlatStyle = FlatStyle.Flat;
            forwardButton.Font = new Font("Segoe UI", 12F);
            forwardButton.ForeColor = Color.White;
            forwardButton.Location = new Point(46, 9);
            forwardButton.Name = "forwardButton";
            forwardButton.Size = new Size(32, 32);
            forwardButton.TabIndex = 1;
            forwardButton.Text = "▶";
            forwardButton.UseVisualStyleBackColor = false;
            forwardButton.Click += forwardButton_Click;
            SetupButtonHover(forwardButton);
            // 
            // backButton
            // 
            backButton.BackColor = Color.Transparent;
            backButton.FlatAppearance.BorderSize = 0;
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.Font = new Font("Segoe UI", 12F);
            backButton.ForeColor = Color.White;
            backButton.Location = new Point(8, 9);
            backButton.Name = "backButton";
            backButton.Size = new Size(32, 32);
            backButton.TabIndex = 0;
            backButton.Text = "◀";
            backButton.UseVisualStyleBackColor = false;
            backButton.Click += backButton_Click;
            SetupButtonHover(backButton);
            // 
            // progressBar
            // 
            progressBar.BackColor = Color.FromArgb(37, 37, 38);
            progressBar.Dock = DockStyle.Top;
            progressBar.ForeColor = Color.FromArgb(0, 122, 204);
            progressBar.Location = new Point(0, 86);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(1200, 3);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 1;
            progressBar.Visible = false;
            // 
            // webContentPanel
            // 
            webContentPanel.BackColor = Color.FromArgb(37, 37, 38);
            webContentPanel.Dock = DockStyle.Fill;
            webContentPanel.Location = new Point(0, 89);
            webContentPanel.Margin = new Padding(0);
            webContentPanel.Name = "webContentPanel";
            webContentPanel.Size = new Size(1200, 611);
            webContentPanel.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(37, 37, 38);
            ClientSize = new Size(1200, 700);
            Controls.Add(webContentPanel);
            Controls.Add(progressBar);
            Controls.Add(navigationPanel);
            Controls.Add(tabStripPanel);
            FormBorderStyle = FormBorderStyle.None;
            Icon = CVSBrowser.Properties.Resources.AppIcon;
            KeyPreview = true;
            MinimumSize = new Size(800, 600);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CVSBrowser";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            Resize += Form1_Resize;
            tabStripPanel.ResumeLayout(false);
            navigationPanel.ResumeLayout(false);
            addressBarBorder.ResumeLayout(false);
            addressBarContainer.ResumeLayout(false);
            addressBarContainer.PerformLayout();
            ResumeLayout(false);
        }

        private void ButtonMouseLeave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.Transparent;
        }

        private void closeButtonMouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(232, 17, 35);
        }

        private void ButtonMouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(62, 62, 66);
        }

        private void SetupButtonHover(Button button)
        {
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(62, 62, 66);
            button.MouseLeave += (s, e) => button.BackColor = Color.Transparent;
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (value && !DesignMode)
            {
                RemoveWindowBorder();
            }
        }
        private const int GWL_EXSTYLE = -20;
        private const int WS_BORDER = 0x00800000;
        private const int WS_DLGFRAME = 0x00400000;
        private const uint SWP_DRAWFRAME = 0x0020;
        private void RemoveWindowBorder()
        {
            if (Handle != IntPtr.Zero)
            {
                var style = GetWindowLong(Handle, GWL_STYLE);

                // Remove caption and system menu but keep thick frame for resizing
                style &= ~(WS_CAPTION | WS_SYSMENU);
                style |= WS_THICKFRAME; // Essential for window resizing

                SetWindowLong(Handle, GWL_STYLE, style);
                SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                    SWP_NOZORDER | SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
            }
        }

        // Custom paint - no border drawing needed
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // No line drawing - completely clean look
        }

        // Keep your existing WndProc for enhanced resize detection
        protected override void WndProc(ref Message m)
        {
            const int RESIZE_HANDLE_SIZE = 5;

            if (m.Msg == WM_NCHITTEST && WindowState != FormWindowState.Maximized)
            {
                var cursor = PointToClient(Cursor.Position);

                // Check for resize zones
                if (cursor.X <= RESIZE_HANDLE_SIZE && cursor.Y <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_TOPLEFT;
                    return;
                }
                if (cursor.X >= ClientSize.Width - RESIZE_HANDLE_SIZE && cursor.Y <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_TOPRIGHT;
                    return;
                }
                if (cursor.X <= RESIZE_HANDLE_SIZE && cursor.Y >= ClientSize.Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_BOTTOMLEFT;
                    return;
                }
                if (cursor.X >= ClientSize.Width - RESIZE_HANDLE_SIZE && cursor.Y >= ClientSize.Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_BOTTOMRIGHT;
                    return;
                }
                if (cursor.X <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_LEFT;
                    return;
                }
                if (cursor.X >= ClientSize.Width - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_RIGHT;
                    return;
                }
                if (cursor.Y <= RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_TOP;
                    return;
                }
                if (cursor.Y >= ClientSize.Height - RESIZE_HANDLE_SIZE)
                {
                    m.Result = (IntPtr)HT_BOTTOM;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        // Enhanced dragging functionality
        private void EnableDragging(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            {
                // Check if clicking on a control (button or textbox)
                var control = navigationPanel.GetChildAtPoint(e.Location);
                if (control == null || control == navigationPanel)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e) => Close();

        private void MaximizeButton_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                maximizeButton.Text = "🗖";
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                maximizeButton.Text = "🗗";
            }
        }

        private void MinimizeButton_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void Form1_Resize(object sender, EventArgs e)
        {
            // Update layout on resize
            if (addressBarBorder != null)
            {
                // Calculate new width for address bar (form width minus buttons and spacing)
                var newWidth = Math.Max(200, ClientSize.Width - 328); // Updated to account for ad blocker button
                addressBarBorder.Size = new Size(newWidth, 36);

                // Update right-side button positions
                if (darkModeExtensionButton != null)
                    darkModeExtensionButton.Location = new Point(Math.Max(ClientSize.Width - 156, 200), 9);

                if (adBlockerButton != null)
                    adBlockerButton.Location = new Point(Math.Max(ClientSize.Width - 118, 238), 9);

                if (bookmarkButton != null)
                    bookmarkButton.Location = new Point(Math.Max(ClientSize.Width - 80, 276), 9);

                if (settingsButton != null)
                    settingsButton.Location = new Point(Math.Max(ClientSize.Width - 42, 314), 9);
            }

            // Repaint to ensure black border is drawn
            //Invalidate();
        }

        #endregion


        // Assuming this function is responsible for creating a new tab
        private async Task<TabPage> CreateNewTab()
        {
            var newTabPage = new TabPage("New Tab");


            // Add the new tab page to the control
            chromeTabControl.TabPages.Add(newTabPage);

            // Optionally, you can set this tab as the active one
            chromeTabControl.SelectedTab = newTabPage;
            // Return the newly created tab page
            return newTabPage;
        }
    }
}
