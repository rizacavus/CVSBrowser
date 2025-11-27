using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class SettingsForm : Form
    {
        public string HomeUrl { get; private set; }
        public bool IsDarkMode { get; private set; }
        public bool IsWebDarkModeEnabled { get; private set; }
        public bool IsAdBlockerEnabled { get; private set; }

        private TextBox homeTextBox;
        private CheckBox darkModeCheckBox;
        private CheckBox webDarkModeCheckBox;
        private CheckBox adBlockerCheckBox;
        private AdBlockerSettings adBlockerSettings;

        public SettingsForm(string homeUrl, bool isDarkMode, bool isWebDarkModeEnabled, bool isAdBlockerEnabled, AdBlockerSettings adBlockerSettings = null)
        {
            // Store the initial values
            HomeUrl = homeUrl;
            IsDarkMode = isDarkMode;
            IsWebDarkModeEnabled = isWebDarkModeEnabled;
            IsAdBlockerEnabled = isAdBlockerEnabled;
            this.adBlockerSettings = adBlockerSettings ?? new AdBlockerSettings();
            
            System.Diagnostics.Debug.WriteLine($"SettingsForm constructor called with: homeUrl={homeUrl}, isDarkMode={isDarkMode}, isWebDarkModeEnabled={isWebDarkModeEnabled}, isAdBlockerEnabled={isAdBlockerEnabled}");
            
            InitializeComponent();
            
            // Set the values AFTER InitializeComponent
            SetInitialValues();
        }

        private void SetInitialValues()
        {
            try
            {
                if (homeTextBox != null)
                {
                    homeTextBox.Text = HomeUrl ?? "https://www.google.com";
                    System.Diagnostics.Debug.WriteLine($"Set homeTextBox.Text = {homeTextBox.Text}");
                }

                if (darkModeCheckBox != null)
                {
                    darkModeCheckBox.Checked = IsDarkMode;
                    System.Diagnostics.Debug.WriteLine($"Set darkModeCheckBox.Checked = {darkModeCheckBox.Checked}");
                }

                if (webDarkModeCheckBox != null)
                {
                    webDarkModeCheckBox.Checked = IsWebDarkModeEnabled;
                    System.Diagnostics.Debug.WriteLine($"Set webDarkModeCheckBox.Checked = {webDarkModeCheckBox.Checked}");
                }

                if (adBlockerCheckBox != null)
                {
                    adBlockerCheckBox.Checked = IsAdBlockerEnabled;
                    System.Diagnostics.Debug.WriteLine($"Set adBlockerCheckBox.Checked = {adBlockerCheckBox.Checked}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SetInitialValues: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            try
            {
                // Home URL section
                var homeLabel = new Label
                {
                    Text = "Home Page URL:",
                    Location = new Point(12, 15),
                    Size = new Size(120, 23),
                    AutoSize = false
                };

                homeTextBox = new TextBox
                {
                    Location = new Point(140, 12),
                    Size = new Size(300, 23),
                    Text = "" // Will be set in SetInitialValues
                };

                // Browser theme section
                var themeLabel = new Label
                {
                    Text = "Browser Theme:",
                    Location = new Point(12, 50),
                    Size = new Size(120, 23),
                    AutoSize = false
                };

                darkModeCheckBox = new CheckBox
                {
                    Text = "Dark Mode",
                    Location = new Point(140, 50),
                    Size = new Size(100, 23),
                    AutoSize = false
                };

                // Web page theme section
                var webDarkModeLabel = new Label
                {
                    Text = "Web Page Theme:",
                    Location = new Point(12, 85),
                    Size = new Size(120, 23),
                    AutoSize = false
                };

                webDarkModeCheckBox = new CheckBox
                {
                    Text = "Dark Mode for Web Pages",
                    Location = new Point(140, 85),
                    Size = new Size(180, 23),
                    AutoSize = false
                };

                // Security section
                var securityLabel = new Label
                {
                    Text = "Security:",
                    Location = new Point(12, 120),
                    Size = new Size(120, 23),
                    AutoSize = false
                };

                adBlockerCheckBox = new CheckBox
                {
                    Text = "Enable Ad Blocker",
                    Location = new Point(140, 120),
                    Size = new Size(150, 23),
                    AutoSize = false
                };

                // Add Advanced Ad Blocker Settings button
                var advancedAdBlockerButton = new Button
                {
                    Text = "Advanced...",
                    Location = new Point(300, 120),
                    Size = new Size(80, 23),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                advancedAdBlockerButton.Click += AdvancedAdBlockerButton_Click;

                // Note label
                var noteLabel = new Label
                {
                    Text = "Note: Web dark mode applies a filter to make light websites dark.\nClick 'Advanced...' for detailed ad blocker settings.",
                    Location = new Point(140, 150),
                    Size = new Size(300, 40),
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                    AutoSize = false
                };

                // Buttons
                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(285, 200),
                    Size = new Size(75, 28)
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(366, 200),
                    Size = new Size(75, 28)
                };

                // Handle OK button click to save values
                okButton.Click += (s, e) => 
                {
                    try
                    {
                        HomeUrl = homeTextBox.Text?.Trim() ?? "https://www.google.com";
                        IsDarkMode = darkModeCheckBox.Checked;
                        IsWebDarkModeEnabled = webDarkModeCheckBox.Checked;
                        IsAdBlockerEnabled = adBlockerCheckBox.Checked;

                        System.Diagnostics.Debug.WriteLine($"OK clicked - saving: homeUrl={HomeUrl}, isDarkMode={IsDarkMode}, isWebDarkModeEnabled={IsWebDarkModeEnabled}, isAdBlockerEnabled={IsAdBlockerEnabled}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in OK button click: {ex.Message}");
                    }
                };

                // Add all controls to the form
                Controls.AddRange(new Control[] { 
                    homeLabel, homeTextBox, 
                    themeLabel, darkModeCheckBox, 
                    webDarkModeLabel, webDarkModeCheckBox,
                    securityLabel, adBlockerCheckBox, advancedAdBlockerButton,
                    noteLabel,
                    okButton, cancelButton 
                });

                // Form properties
                Text = "Browser Settings";
                Size = new Size(470, 270);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                ShowInTaskbar = false;
                BackColor = Color.White;

                System.Diagnostics.Debug.WriteLine("SettingsForm InitializeComponent completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeComponent: {ex.Message}");
                throw;
            }
        }

        private void AdvancedAdBlockerButton_Click(object sender, EventArgs e)
        {
            try
            {
                var adBlockerManagerForm = new AdBlockerManagerForm(adBlockerSettings);
                
                if (adBlockerManagerForm.ShowDialog() == DialogResult.OK)
                {
                    adBlockerSettings = adBlockerManagerForm.Settings;
                    MessageBox.Show("Ad blocker settings updated successfully!", "Settings Updated", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening ad blocker settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Property to get updated ad blocker settings
        public AdBlockerSettings GetUpdatedAdBlockerSettings()
        {
            return adBlockerSettings;
        }
    }
}