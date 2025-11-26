namespace WinFormsApp1
{
    public partial class SettingsForm : Form
    {
        public string HomeUrl { get; private set; }
        public bool IsDarkMode { get; private set; }
        public bool IsWebDarkModeEnabled { get; private set; }

        public SettingsForm(string currentHomeUrl, bool isDarkMode, bool isWebDarkModeEnabled = false)
        {
            HomeUrl = currentHomeUrl;
            IsDarkMode = isDarkMode;
            IsWebDarkModeEnabled = isWebDarkModeEnabled;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var homeLabel = new Label
            {
                Text = "Home Page URL:",
                Location = new Point(12, 15),
                Size = new Size(120, 23)
            };

            var homeTextBox = new TextBox
            {
                Text = HomeUrl,
                Location = new Point(140, 12),
                Size = new Size(300, 23)
            };

            var themeLabel = new Label
            {
                Text = "Browser Theme:",
                Location = new Point(12, 50),
                Size = new Size(120, 23)
            };

            var darkModeCheckBox = new CheckBox
            {
                Text = "Dark Mode",
                Checked = IsDarkMode,
                Location = new Point(140, 50),
                Size = new Size(100, 23)
            };

            var webDarkModeLabel = new Label
            {
                Text = "Web Page Theme:",
                Location = new Point(12, 85),
                Size = new Size(120, 23)
            };

            var webDarkModeCheckBox = new CheckBox
            {
                Text = "Dark Mode for Web Pages",
                Checked = IsWebDarkModeEnabled,
                Location = new Point(140, 85),
                Size = new Size(180, 23)
            };

            var noteLabel = new Label
            {
                Text = "Note: Web dark mode applies a filter to make light websites dark.",
                Location = new Point(140, 110),
                Size = new Size(300, 30),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(285, 150),
                Size = new Size(75, 28)
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(366, 150),
                Size = new Size(75, 28)
            };

            okButton.Click += (s, e) => 
            {
                HomeUrl = homeTextBox.Text.Trim();
                IsDarkMode = darkModeCheckBox.Checked;
                IsWebDarkModeEnabled = webDarkModeCheckBox.Checked;
            };

            Controls.AddRange(new Control[] { 
                homeLabel, homeTextBox, 
                themeLabel, darkModeCheckBox, 
                webDarkModeLabel, webDarkModeCheckBox, noteLabel,
                okButton, cancelButton 
            });

            Text = "Settings";
            Size = new Size(470, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
        }
    }
}