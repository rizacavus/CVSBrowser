using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace WinFormsApp1
{
    public partial class AdBlockerManagerForm : Form
    {
        public AdBlockerSettings Settings { get; private set; }
        private bool hasChanges = false;
        private TabControl tabControl = null!;
        private TabPage generalTab;
        private TabPage domainsTab;
        private TabPage keywordsTab;
        private TabPage cssTab;
        private TabPage jsTab;
        private TabPage importExportTab;
        private Panel buttonPanel;
        private Button okButton;
        private Button cancelButton;
        private Button applyButton;
        private Label statusLabel;
        private static readonly HttpClient httpClient = new HttpClient();
        public AdBlockerManagerForm(AdBlockerSettings settings)
        {
            Settings = new AdBlockerSettings
            {
                IsEnabled = settings.IsEnabled,
                BlockedDomains = new HashSet<string>(settings.BlockedDomains),
                BlockedKeywords = new HashSet<string>(settings.BlockedKeywords),
                CustomCSS = settings.CustomCSS,
                CustomJavaScript = settings.CustomJavaScript,
                Version = settings.Version
            };

            InitializeComponent();
            LoadSettings();
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Ad Blocker Settings Manager";
            this.Size = new Size(900, 700);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(700, 500);

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "tabControl"
            };

            // Create tabs
            generalTab = new TabPage("General");
            domainsTab = new TabPage("Blocked Domains");
            keywordsTab = new TabPage("Blocked Keywords");
            cssTab = new TabPage("Custom CSS");
            jsTab = new TabPage("Custom JavaScript");
            importExportTab = new TabPage("Import/Export");

            // Create tab content
            CreateGeneralTab(generalTab);
            CreateDomainsTab(domainsTab);
            CreateKeywordsTab(keywordsTab);
            CreateCssTab(cssTab);
            CreateJsTab(jsTab);
            CreateImportExportTab(importExportTab);

            // Add tabs to control
            tabControl.TabPages.AddRange(new TabPage[] { generalTab, domainsTab, keywordsTab, cssTab, jsTab, importExportTab });

            // Button panel
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Size = new Size(80, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            okButton.Location = new Point(buttonPanel.Width - 180, 15);

            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(80, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cancelButton.Location = new Point(buttonPanel.Width - 90, 15);

            applyButton = new Button
            {
                Text = "Apply",
                Size = new Size(80, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            applyButton.Location = new Point(buttonPanel.Width - 270, 15);
            applyButton.Click += ApplyButton_Click;

            statusLabel = new Label
            {
                Name = "statusLabel",
                Text = hasChanges ? "Settings modified" : "No changes",
                Location = new Point(10, 20),
                Size = new Size(200, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = hasChanges ? Color.Orange : Color.Green
            };

            buttonPanel.Controls.AddRange(new Control[] {
        okButton, cancelButton, applyButton, statusLabel
    });

            this.Controls.AddRange(new Control[] { tabControl, buttonPanel });
            this.ResumeLayout(false);

            // Update status when changes occur
            this.Load += (s, e) => UpdateStatusLabel(statusLabel);
        }

        private void CreateGeneralTab(TabPage generalTab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            var enabledCheckBox = new CheckBox
            {
                Text = "Enable Ad Blocker",
                Location = new Point(10, 20),
                Size = new Size(200, 25),
                Checked = Settings.IsEnabled
            };
            enabledCheckBox.CheckedChanged += (s, e) => {
                Settings.IsEnabled = enabledCheckBox.Checked;
                hasChanges = true;
                UpdateStatusDisplay();
            };

            var versionLabel = new Label
            {
                Text = $"Version: {Settings.Version}",
                Location = new Point(10, 60),
                Size = new Size(200, 20)
            };

            var statsLabel = new Label
            {
                Text = $"Blocked Domains: {Settings.BlockedDomains.Count}\nBlocked Keywords: {Settings.BlockedKeywords.Count}",
                Location = new Point(10, 100),
                Size = new Size(300, 40)
            };

            panel.Controls.AddRange(new Control[] { enabledCheckBox, versionLabel, statsLabel });
            generalTab.Controls.Add(panel);
        }
        private void CreateDomainsTab(TabPage domainsTab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Adjust listBox width to leave more room for buttons
            var listBox = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(350, 280), // Reduced width from 400 to 350
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                SelectionMode = SelectionMode.MultiExtended
            };

            // Add search functionality
            var searchLabel = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                Size = new Size(50, 20)
            };

            var searchTextBox = new TextBox
            {
                Location = new Point(70, 12),
                Size = new Size(200, 25),
                PlaceholderText = "Search domains..."
            };
            searchTextBox.TextChanged += (object? s, EventArgs e) => FilterDomainsList(listBox, searchTextBox.Text);

            var clearSearchButton = new Button
            {
                Text = "×",
                Location = new Point(275, 12),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat
            };
            clearSearchButton.Click += (object? s, EventArgs e) => {
                searchTextBox.Text = "";
                RefreshDomainsList(listBox);
            };

            // Adjust button positions to be more to the left
            var addButton = new Button
            {
                Text = "Add Domain",
                Location = new Point(370, 40), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addButton.Click += (object? s, EventArgs e) => ShowAddDomainDialog(listBox);

            var removeButton = new Button
            {
                Text = "Remove Selected",
                Location = new Point(370, 80), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            removeButton.Click += (object? s, EventArgs e) => RemoveSelectedDomains(listBox);

            var bulkImportButton = new Button
            {
                Text = "Bulk Import",
                Location = new Point(370, 120), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            bulkImportButton.Click += (object? s, EventArgs e) => BulkImportDomains(listBox);

            var clearAllButton = new Button
            {
                Text = "Clear All",
                Location = new Point(370, 160), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearAllButton.Click += (object? s, EventArgs e) => ClearAllDomains(listBox);

            var countLabel = new Label
            {
                Text = $"Total: {Settings.BlockedDomains.Count} domains",
                Location = new Point(370, 200), // Changed from 420 to 370
                Size = new Size(120, 20),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Name = "domainCountLabel"
            };

            // Double-click to edit
            listBox.DoubleClick += (object? s, EventArgs e) => EditSelectedDomain(listBox);

            // Context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Add", null, (s, e) => ShowAddDomainDialog(listBox));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Edit", null, (s, e) => EditSelectedDomain(listBox));
            contextMenu.Items.Add("Remove", null, (s, e) => RemoveSelectedDomains(listBox));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Copy to Clipboard", null, (s, e) => CopySelectedDomainsToClipboard(listBox));
            listBox.ContextMenuStrip = contextMenu;

            RefreshDomainsList(listBox);

            panel.Controls.AddRange(new Control[] {
        searchLabel, searchTextBox, clearSearchButton,
        listBox, addButton, removeButton, bulkImportButton, clearAllButton, countLabel
    });
            domainsTab.Controls.Add(panel);
        }
        private void CreateKeywordsTab(TabPage keywordsTab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var listBox = new ListBox
            {
                Location = new Point(10, 40),
                Size = new Size(350, 280), // Reduced width from 400 to 350
                Anchor = AnchorStyles.Top | AnchorStyles.Left |  AnchorStyles.Bottom,
                SelectionMode = SelectionMode.MultiExtended
            };

            // Add search functionality
            var searchLabel = new Label
            {
                Text = "Search:",
                Location = new Point(10, 15),
                Size = new Size(50, 20)
            };

            var searchTextBox = new TextBox
            {
                Location = new Point(70, 12),
                Size = new Size(200, 25),
                PlaceholderText = "Search keywords..."
            };
            searchTextBox.TextChanged += (object? s, EventArgs e) => FilterKeywordsList(listBox, searchTextBox.Text);

            var clearSearchButton = new Button
            {
                Text = "×",
                Location = new Point(275, 12),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat
            };
            clearSearchButton.Click += (object? s, EventArgs e) => {
                searchTextBox.Text = "";
                RefreshKeywordsList(listBox);
            };

            var addButton = new Button
            {
                Text = "Add Keyword",
                Location = new Point(370, 40), // Changed from 420 to 370
                Size = new Size(100, 30),
               // Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addButton.Click += (object? s, EventArgs e) => ShowAddKeywordDialog(listBox);

            var removeButton = new Button
            {
                Text = "Remove Selected",
                Location = new Point(370, 80), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            removeButton.Click += (object? s, EventArgs e) => RemoveSelectedKeywords(listBox);

            var commonKeywordsButton = new Button
            {
                Text = "Add Common",
                Location = new Point(370, 120), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            commonKeywordsButton.Click += (object? s, EventArgs e) => AddCommonKeywords(listBox);

            var clearAllButton = new Button
            {
                Text = "Clear All",
                Location = new Point(370, 160), // Changed from 420 to 370
                Size = new Size(100, 30),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearAllButton.Click += (object? s, EventArgs e) => ClearAllKeywords(listBox);

            var countLabel = new Label
            {
                Text = $"Total: {Settings.BlockedKeywords.Count} keywords",
                Location = new Point(370, 200), // Changed from 420 to 370
                Size = new Size(120, 20),
                //Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Name = "keywordCountLabel"
            };

            // Double-click to edit
            listBox.DoubleClick += (object? s, EventArgs e) => EditSelectedKeyword(listBox);

            // Context menu
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Add", null, (s, e) => AddCommonKeywords(listBox));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Edit", null, (s, e) => EditSelectedKeyword(listBox));
            contextMenu.Items.Add("Remove", null, (s, e) => RemoveSelectedKeywords(listBox));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Copy to Clipboard", null, (s, e) => CopySelectedKeywordsToClipboard(listBox));
            listBox.ContextMenuStrip = contextMenu;

            RefreshKeywordsList(listBox);

            panel.Controls.AddRange(new Control[] {
        searchLabel, searchTextBox, clearSearchButton,
        listBox, addButton, removeButton, commonKeywordsButton, clearAllButton, countLabel
    });
            keywordsTab.Controls.Add(panel);
        }

        private void CreateCssTab(TabPage cssTab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var textBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 10),
                Size = new Size(500, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Text = Settings.CustomCSS
            };
            textBox.TextChanged += (object? s, EventArgs e) => {
                Settings.CustomCSS = textBox.Text;
                hasChanges = true;
                UpdateStatusDisplay();
            };

            var loadButton = new Button
            {
                Text = "Load File",
                Location = new Point(520, 10),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            loadButton.Click += (object? s, EventArgs e) => LoadCSSFromFile(textBox);

            var saveButton = new Button
            {
                Text = "Save File",
                Location = new Point(520, 50),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            saveButton.Click += (object? s, EventArgs e) => SaveCSSToFile(textBox.Text);

            var validateButton = new Button
            {
                Text = "Validate",
                Location = new Point(520, 90),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            validateButton.Click += (object? s, EventArgs e) => ValidateCSS(textBox.Text);

            panel.Controls.AddRange(new Control[] { textBox, loadButton, saveButton, validateButton });
            cssTab.Controls.Add(panel);
        }

        private void CreateJsTab(TabPage jsTab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var textBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 10),
                Size = new Size(500, 350),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Text = Settings.CustomJavaScript
            };
            textBox.TextChanged += (object? s, EventArgs e) => {
                Settings.CustomJavaScript = textBox.Text;
                hasChanges = true;
                UpdateStatusDisplay();
            };

            var loadButton = new Button
            {
                Text = "Load File",
                Location = new Point(520, 10),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            loadButton.Click += (object? s, EventArgs e) => LoadJSFromFile(textBox);

            var saveButton = new Button
            {
                Text = "Save File",
                Location = new Point(520, 50),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            saveButton.Click += (object? s, EventArgs e) => SaveJSToFile(textBox.Text);

            var validateButton = new Button
            {
                Text = "Validate",
                Location = new Point(520, 90),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            validateButton.Click += (object? s, EventArgs e) => ValidateJavaScript(textBox.Text);

            panel.Controls.AddRange(new Control[] { textBox, loadButton, saveButton, validateButton });
            jsTab.Controls.Add(panel);
        }

        private void CreateImportExportTab(TabPage importExportTab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            var exportLabel = new Label
            {
                Text = "Export Options:",
                Location = new Point(10, 20),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var exportDomainsButton = new Button
            {
                Text = "Export Domains",
                Location = new Point(10, 50),
                Size = new Size(120, 30)
            };
            exportDomainsButton.Click += (object? s, EventArgs e) => ExportDomainsOnly();

            var exportKeywordsButton = new Button
            {
                Text = "Export Keywords",
                Location = new Point(140, 50),
                Size = new Size(120, 30)
            };
            exportKeywordsButton.Click += (object? s, EventArgs e) => ExportKeywordsOnly();

            var exportScriptsButton = new Button
            {
                Text = "Export Scripts",
                Location = new Point(270, 50),
                Size = new Size(120, 30)
            };
            exportScriptsButton.Click += (object? s, EventArgs e) => ExportScripts();

            var importLabel = new Label
            {
                Text = "Import Options:",
                Location = new Point(10, 100),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var importDomainsButton = new Button
            {
                Text = "Import Domains",
                Location = new Point(10, 130),
                Size = new Size(120, 30)
            };
            importDomainsButton.Click += (object? s, EventArgs e) => ImportDomainsOnly();

            var importKeywordsButton = new Button
            {
                Text = "Import Keywords",
                Location = new Point(140, 130),
                Size = new Size(120, 30)
            };
            importKeywordsButton.Click += (object? s, EventArgs e) => ImportKeywordsOnly();

            var importUrlButton = new Button
            {
                Text = "Import from URL",
                Location = new Point(270, 130),
                Size = new Size(120, 30)
            };
            importUrlButton.Click += async (object? s, EventArgs e) => await ImportFromUrlAsync();

            // Filter Lists Section
            var filterListLabel = new Label
            {
                Text = "Popular Filter Lists:",
                Location = new Point(10, 180),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            var filterListBox = new ListBox
            {
                Location = new Point(10, 210),
                Size = new Size(300, 120),
                Items = {
                    "EasyList - General ads blocking",
                    "EasyPrivacy - Privacy protection",
                    "Fanboy's Annoyances - Remove annoying elements",
                    "uBlock Origin Filters - Advanced blocking",
                    "AdGuard Base Filter - Comprehensive ad blocking"
                }
            };

            var downloadFilterButton = new Button
            {
                Text = "Download Selected",
                Location = new Point(320, 210),
                Size = new Size(120, 30)
            };
            downloadFilterButton.Click += async (object? s, EventArgs e) => await DownloadFilterListAsync(filterListBox);

            var updateAllButton = new Button
            {
                Text = "Update All Filters",
                Location = new Point(320, 250),
                Size = new Size(120, 30)
            };
            updateAllButton.Click += async (object? s, EventArgs e) => await UpdateAllFiltersAsync();

            var statusTextBox = new TextBox
            {
                Location = new Point(10, 340),
                Size = new Size(430, 60),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "Ready to import/export filter lists...",
                Name = "statusTextBox"
            };

            panel.Controls.AddRange(new Control[] {
                exportLabel, exportDomainsButton, exportKeywordsButton, exportScriptsButton,
                importLabel, importDomainsButton, importKeywordsButton, importUrlButton,
                filterListLabel, filterListBox, downloadFilterButton, updateAllButton, statusTextBox
            });
            importExportTab.Controls.Add(panel);
        }

        // Helper methods for list management
        private void RefreshDomainsList(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var domain in Settings.BlockedDomains.OrderBy(d => d))
            {
                listBox.Items.Add(domain);
            }
        }

        private void RefreshKeywordsList(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var keyword in Settings.BlockedKeywords.OrderBy(k => k))
            {
                listBox.Items.Add(keyword);
            }
        }

        private void RefreshAllLists()
        {
            hasChanges = true;
            UpdateStatusDisplay();
        }

        // Implementation methods
        private void BulkImportDomains(ListBox listBox)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Import Domain List",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    Multiselect = false
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var lines = File.ReadAllLines(openDialog.FileName);
                    int addedCount = 0;

                    foreach (var line in lines)
                    {
                        var domain = line.Trim().ToLower();
                        if (!string.IsNullOrEmpty(domain) && IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                        {
                            Settings.BlockedDomains.Add(domain);
                            addedCount++;
                        }
                    }

                    RefreshDomainsList(listBox);
                    UpdateDomainCount();
                    hasChanges = true;
                    UpdateStatusDisplay();

                    MessageBox.Show($"Successfully imported {addedCount} domains.", "Import Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing domains: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportDomainsList()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Export Domain List",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"BlockedDomains_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var domains = Settings.BlockedDomains.OrderBy(d => d).ToList();
                    File.WriteAllLines(saveDialog.FileName, domains);

                    MessageBox.Show($"Successfully exported {domains.Count} domains.", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting domains: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddCommonKeywords(ListBox listBox)
        {
            var commonKeywords = new[]
            {
                "advertisement", "banner", "popup", "sponsor", "tracking",
                "/ads/", "/ad/", "/banner/", "/popup/", "adservice",
                "adsystem", "adnxs", "adsense", "doubleclick",
                "googlesyndication", "amazon-adsystem", "outbrain",
                "taboola", "addthis", "sharethis", "scorecardresearch"
            };

            int addedCount = 0;
            foreach (var keyword in commonKeywords)
            {
                if (!Settings.BlockedKeywords.Contains(keyword))
                {
                    Settings.BlockedKeywords.Add(keyword);
                    addedCount++;
                }
            }

            RefreshKeywordsList(listBox);
            UpdateKeywordCount();
            hasChanges = true;
            UpdateStatusDisplay();

            MessageBox.Show($"Added {addedCount} common keywords.", "Keywords Added", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadCSSFromFile(TextBox textBox)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Load CSS File",
                    Filter = "CSS Files (*.css)|*.css|JavaScript Files (*.js)|*.js|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    Multiselect = false
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var content = File.ReadAllText(openDialog.FileName);
                    
                    // Wrap CSS content in JavaScript if it's a pure CSS file
                    if (openDialog.FileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    {
                        content = $@"
(function() {{
    if (document.getElementById('custom-ad-blocker-css')) return;
    
    const style = document.createElement('style');
    style.id = 'custom-ad-blocker-css';
    style.textContent = `{content.Replace("`", "\\`")}`;
    document.head.appendChild(style);
}})();";
                    }

                    Settings.CustomCSS = content;
                    textBox.Text = content;
                    hasChanges = true;
                    UpdateStatusDisplay();

                    MessageBox.Show("CSS loaded successfully!", "Load Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSS file: {ex.Message}", "Load Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveCSSToFile(string css)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Save CSS File",
                    Filter = "JavaScript Files (*.js)|*.js|CSS Files (*.css)|*.css|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "js",
                    FileName = $"AdBlockerCSS_{DateTime.Now:yyyyMMdd_HHmmss}.js"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, css);
                    MessageBox.Show("CSS saved successfully!", "Save Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving CSS file: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateCSS(string css)
        {
            try
            {
                var issues = new List<string>();

                if (string.IsNullOrWhiteSpace(css))
                {
                    MessageBox.Show("CSS content is empty.", "Validation", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Basic CSS validation checks
                var openBraces = css.Count(c => c == '{');
                var closeBraces = css.Count(c => c == '}');
                if (openBraces != closeBraces)
                {
                    issues.Add($"Mismatched braces: {openBraces} opening, {closeBraces} closing");
                }

                var openParens = css.Count(c => c == '(');
                var closeParens = css.Count(c => c == ')');
                if (openParens != closeParens)
                {
                    issues.Add($"Mismatched parentheses: {openParens} opening, {closeParens} closing");
                }

                // Check for common JavaScript functions (indicates it's wrapped JS)
                if (css.Contains("document.") || css.Contains("function") || css.Contains("const ") || css.Contains("let "))
                {
                    issues.Add("Content appears to be JavaScript (this is normal for ad blocker CSS)");
                }

                if (issues.Count == 0)
                {
                    MessageBox.Show("CSS validation passed!", "Validation Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Validation issues found:\n\n{string.Join("\n", issues)}", 
                        "Validation Results", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating CSS: {ex.Message}", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadJSFromFile(TextBox textBox)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Load JavaScript File",
                    Filter = "JavaScript Files (*.js)|*.js|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    Multiselect = false
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var content = File.ReadAllText(openDialog.FileName);
                    Settings.CustomJavaScript = content;
                    textBox.Text = content;
                    hasChanges = true;
                    UpdateStatusDisplay();

                    MessageBox.Show("JavaScript loaded successfully!", "Load Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JavaScript file: {ex.Message}", "Load Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveJSToFile(string js)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Save JavaScript File",
                    Filter = "JavaScript Files (*.js)|*.js|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "js",
                    FileName = $"AdBlockerJS_{DateTime.Now:yyyyMMdd_HHmmss}.js"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, js);
                    MessageBox.Show("JavaScript saved successfully!", "Save Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving JavaScript file: {ex.Message}", "Save Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateJavaScript(string js)
        {
            try
            {
                var issues = new List<string>();

                if (string.IsNullOrWhiteSpace(js))
                {
                    MessageBox.Show("JavaScript content is empty.", "Validation", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Basic JavaScript validation checks
                var openBraces = js.Count(c => c == '{');
                var closeBraces = js.Count(c => c == '}');
                if (openBraces != closeBraces)
                {
                    issues.Add($"Mismatched braces: {openBraces} opening, {closeBraces} closing");
                }

                var openParens = js.Count(c => c == '(');
                var closeParens = js.Count(c => c == ')');
                if (openParens != closeParens)
                {
                    issues.Add($"Mismatched parentheses: {openParens} opening, {closeParens} closing");
                }

                var openBrackets = js.Count(c => c == '[');
                var closeBrackets = js.Count(c => c == ']');
                if (openBrackets != closeBrackets)
                {
                    issues.Add($"Mismatched brackets: {openBrackets} opening, {closeBrackets} closing");
                }

                // Check for potentially dangerous functions
                var dangerousFunctions = new[] { "eval(", "setTimeout(", "setInterval(" };
                foreach (var func in dangerousFunctions)
                {
                    if (js.Contains(func))
                    {
                        issues.Add($"Contains potentially unsafe function: {func}");
                    }
                }

                if (issues.Count == 0)
                {
                    MessageBox.Show("JavaScript validation passed!", "Validation Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Validation issues found:\n\n{string.Join("\n", issues)}", 
                        "Validation Results", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating JavaScript: {ex.Message}", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportDomainsOnly()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Export Domains Only",
                    Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"BlockedDomains_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var domainsData = new { BlockedDomains = Settings.BlockedDomains.OrderBy(d => d).ToList() };
                    var json = JsonSerializer.Serialize(domainsData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show($"Successfully exported {Settings.BlockedDomains.Count} domains.", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting domains: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportKeywordsOnly()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Export Keywords Only",
                    Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"BlockedKeywords_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var keywordsData = new { BlockedKeywords = Settings.BlockedKeywords.OrderBy(k => k).ToList() };
                    var json = JsonSerializer.Serialize(keywordsData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show($"Successfully exported {Settings.BlockedKeywords.Count} keywords.", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting keywords: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportScripts()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Export CSS & JavaScript",
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"AdBlockerScripts_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var scriptsData = new 
                    { 
                        CustomCSS = Settings.CustomCSS,
                        CustomJavaScript = Settings.CustomJavaScript,
                        ExportDate = DateTime.Now
                    };
                    var json = JsonSerializer.Serialize(scriptsData, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show("CSS & JavaScript exported successfully!", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting scripts: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportDomainsOnly()
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Import Domains",
                    Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    Multiselect = false
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var content = File.ReadAllText(openDialog.FileName);
                    List<string>? domains;

                    if (openDialog.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        domains = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(d => d.Trim().ToLower())
                                        .Where(d => !string.IsNullOrEmpty(d) && IsValidDomain(d))
                                        .ToList();
                    }
                    else
                    {
                        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                        if (jsonData?.TryGetValue("BlockedDomains", out var domainsObj) == true && domainsObj != null)
                        {
                            domains = JsonSerializer.Deserialize<List<string>>(domainsObj.ToString()!);
                        }
                        else
                        {
                            throw new Exception("Invalid format: BlockedDomains not found");
                        }
                    }

                    int addedCount = 0;
                    if (domains != null)
                    {
                        foreach (var domain in domains)
                        {
                            if (!Settings.BlockedDomains.Contains(domain))
                            {
                                Settings.BlockedDomains.Add(domain);
                                addedCount++;
                            }
                        }
                    }

                    RefreshAllLists();
                    hasChanges = true;
                    UpdateStatusDisplay();

                    MessageBox.Show($"Successfully imported {addedCount} new domains.", "Import Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing domains: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportKeywordsOnly()
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Import Keywords",
                    Filter = "JSON Files (*.json)|*.json|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    Multiselect = false
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var content = File.ReadAllText(openDialog.FileName);
                    List<string>? keywords;

                    if (openDialog.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        keywords = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(k => k.Trim().ToLower())
                                         .Where(k => !string.IsNullOrWhiteSpace(k))
                                         .ToList();
                    }
                    else
                    {
                        var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                        if (jsonData?.TryGetValue("BlockedKeywords", out var keywordsObj) == true && keywordsObj != null)
                        {
                            keywords = JsonSerializer.Deserialize<List<string>>(keywordsObj.ToString()!);
                        }
                        else
                        {
                            throw new Exception("Invalid format: BlockedKeywords not found");
                        }
                    }

                    int addedCount = 0;
                    if (keywords != null)
                    {
                        foreach (var keyword in keywords)
                        {
                            if (!Settings.BlockedKeywords.Contains(keyword))
                            {
                                Settings.BlockedKeywords.Add(keyword);
                                addedCount++;
                            }
                        }
                    }

                    RefreshAllLists();
                    hasChanges = true;
                    UpdateStatusDisplay();

                    MessageBox.Show($"Successfully imported {addedCount} new keywords.", "Import Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing keywords: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ImportFromUrlAsync()
        {
            try
            {
                var urlDialog = new EnhancedInputDialog("Enter URL to import filter list from:", "Import from URL");
                if (urlDialog.ShowDialog() == DialogResult.OK)
                {
                    var url = urlDialog.InputText;
                    if (string.IsNullOrWhiteSpace(url))
                        return;

                    var progressDialog = new ProgressDialog("Downloading filter list...");
                    progressDialog.Show();

                    try
                    {
                        var content = await httpClient.GetStringAsync(url);
                        progressDialog.Close();

                        // Try to parse as different formats
                        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        int addedDomains = 0, addedKeywords = 0;

                        foreach (var line in lines)
                        {
                            var trimmed = line.Trim();
                            
                            // Skip comments and empty lines
                            if (trimmed.StartsWith("#") || trimmed.StartsWith("!") || string.IsNullOrEmpty(trimmed))
                                continue;

                            // Parse AdBlock format (||domain.com^)
                            if (trimmed.StartsWith("||") && trimmed.EndsWith("^"))
                            {
                                var domain = trimmed.Substring(2, trimmed.Length - 3).ToLower();
                                if (IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                                {
                                    Settings.BlockedDomains.Add(domain);
                                    addedDomains++;
                                }
                            }
                            // Parse hosts format (0.0.0.0 domain.com)
                            else if (trimmed.StartsWith("0.0.0.0 ") || trimmed.StartsWith("127.0.0.1 "))
                            {
                                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 2)
                                {
                                    var domain = parts[1].ToLower();
                                    if (IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                                    {
                                        Settings.BlockedDomains.Add(domain);
                                        addedDomains++;
                                    }
                                }
                            }
                        }

                        RefreshAllLists();
                        hasChanges = true;
                        UpdateStatusDisplay();

                        MessageBox.Show($"Successfully imported {addedDomains} domains and {addedKeywords} keywords from URL.", 
                            "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        progressDialog.Close();
                        MessageBox.Show($"Error downloading from URL: {ex.Message}", "Download Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing from URL: {ex.Message}", "Import Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadFilterListAsync(ListBox filterListBox)
        {
            if (filterListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a filter list to download.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var filterUrls = new Dictionary<int, string>
            {
                [0] = "https://easylist.to/easylist/easylist.txt", // EasyList
                [1] = "https://easylist.to/easylist/easyprivacy.txt", // EasyPrivacy
                [2] = "https://easylist.to/easylist/fanboy-annoyance.txt", // Fanboy's Annoyances
                [3] = "https://raw.githubusercontent.com/uBlockOrigin/uAssets/master/filters/filters.txt", // uBlock Origin
                [4] = "https://filters.adtidy.org/extension/chromium/filters/2.txt" // AdGuard Base
            };

            if (!filterUrls.TryGetValue(filterListBox.SelectedIndex, out var url))
            {
                MessageBox.Show("Invalid filter list selection.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var statusText = GetStatusTextBox();
            if (statusText != null)
            {
                statusText.Text = $"Downloading {filterListBox.SelectedItem}...\r\n";
            }

            var progressDialog = new ProgressDialog($"Downloading {filterListBox.SelectedItem}...");
            progressDialog.Show();

            try
            {
                var content = await httpClient.GetStringAsync(url);
                progressDialog.Close();

                // Parse the filter list
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                int addedDomains = 0, addedKeywords = 0;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    
                    // Skip comments and empty lines
                    if (trimmed.StartsWith("#") || trimmed.StartsWith("!") || string.IsNullOrEmpty(trimmed))
                        continue;

                    // Parse AdBlock format (||domain.com^)
                    if (trimmed.StartsWith("||") && trimmed.EndsWith("^"))
                    {
                        var domain = trimmed.Substring(2, trimmed.Length - 3).ToLower();
                        if (IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                        {
                            Settings.BlockedDomains.Add(domain);
                            addedDomains++;
                        }
                    }
                    // Parse hosts format (0.0.0.0 domain.com)
                    else if (trimmed.StartsWith("0.0.0.0 ") || trimmed.StartsWith("127.0.0.1 "))
                    {
                        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var domain = parts[1].ToLower();
                            if (IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                            {
                                Settings.BlockedDomains.Add(domain);
                                addedDomains++;
                            }
                        }
                    }
                    // Parse element hiding rules for keywords
                    else if (trimmed.Contains("##") && !trimmed.StartsWith("@@"))
                    {
                        var selectorPart = trimmed.Split("##", 2)[1];
                        if (selectorPart.Contains("[") && selectorPart.Contains("*="))
                        {
                            // Extract keyword from attribute selectors like [id*="ad"]
                            var match = System.Text.RegularExpressions.Regex.Match(selectorPart, @"\*=""([^""]+)""");
                            if (match.Success)
                            {
                                var keyword = match.Groups[1].Value.ToLower();
                                if (!string.IsNullOrEmpty(keyword) && !Settings.BlockedKeywords.Contains(keyword))
                                {
                                    Settings.BlockedKeywords.Add(keyword);
                                    addedKeywords++;
                                }
                            }
                        }
                    }
                }

                RefreshAllLists();
                hasChanges = true;
                UpdateStatusDisplay();

                var message = $"Successfully downloaded {filterListBox.SelectedItem}!\n\n" +
                              $"Added {addedDomains} domains and {addedKeywords} keywords.";
                
                if (statusText != null)
                {
                    statusText.Text += $"✓ {message}\r\n";
                }

                MessageBox.Show(message, "Download Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                progressDialog.Close();
                var errorMsg = $"Error downloading filter list: {ex.Message}";
                
                statusText = GetStatusTextBox();
                if (statusText != null)
                {
                    statusText.Text += $"✗ {errorMsg}\r\n";
                }

                MessageBox.Show(errorMsg, "Download Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateAllFiltersAsync()
        {
            var statusText = GetStatusTextBox();
            if (statusText != null)
            {
                statusText.Text = "Starting filter updates...\r\n";
            }

            var progressDialog = new ProgressDialog("Updating all filter lists...");
            progressDialog.Show();

            try
            {
                var filterUrls = new Dictionary<string, string>
                {
                    ["EasyList"] = "https://easylist.to/easylist/easylist.txt",
                    ["EasyPrivacy"] = "https://easylist.to/easylist/easyprivacy.txt",
                    ["Fanboy's Annoyances"] = "https://easylist.to/easylist/fanboy-annoyance.txt",
                    ["uBlock Origin"] = "https://raw.githubusercontent.com/uBlockOrigin/uAssets/master/filters/filters.txt",
                    ["AdGuard Base"] = "https://filters.adtidy.org/extension/chromium/filters/2.txt"
                };

                int totalAdded = 0;
                int successCount = 0;

                foreach (var filter in filterUrls)
                {
                    try
                    {
                        if (statusText != null)
                        {
                            statusText.Text += $"Downloading {filter.Key}...\r\n";
                            statusText.Refresh();
                        }

                        var content = await httpClient.GetStringAsync(filter.Value);
                        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        int addedFromThisList = 0;

                        foreach (var line in lines)
                        {
                            var trimmed = line.Trim();
                            
                            if (trimmed.StartsWith("#") || trimmed.StartsWith("!") || string.IsNullOrEmpty(trimmed))
                                continue;

                            if (trimmed.StartsWith("||") && trimmed.EndsWith("^"))
                            {
                                var domain = trimmed.Substring(2, trimmed.Length - 3).ToLower();
                                if (IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                                {
                                    Settings.BlockedDomains.Add(domain);
                                    addedFromThisList++;
                                }
                            }
                            else if (trimmed.StartsWith("0.0.0.0 ") || trimmed.StartsWith("127.0.0.1 "))
                            {
                                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length >= 2)
                                {
                                    var domain = parts[1].ToLower();
                                    if (IsValidDomain(domain) && !Settings.BlockedDomains.Contains(domain))
                                    {
                                        Settings.BlockedDomains.Add(domain);
                                        addedFromThisList++;
                                    }
                                }
                            }
                        }

                        totalAdded += addedFromThisList;
                        successCount++;

                        if (statusText != null)
                        {
                            statusText.Text += $"✓ {filter.Key}: Added {addedFromThisList} new entries\r\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        if (statusText != null)
                        {
                            statusText.Text += $"✗ {filter.Key}: Failed - {ex.Message}\r\n";
                        }
                    }
                }

                progressDialog.Close();

                RefreshAllLists();
                hasChanges = true;
                UpdateStatusDisplay();
                Settings.LastUpdated = DateTime.Now;

                var message = $"Filter update complete!\n\n" +
                              $"Successfully updated {successCount} out of {filterUrls.Count} filter lists.\n" +
                              $"Total new entries added: {totalAdded}";

                if (statusText != null)
                {
                    statusText.Text += $"\r\n{message}\r\n";
                }

                MessageBox.Show(message, "Update Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                progressDialog.Close();
                var errorMsg = $"Error updating filters: {ex.Message}";
                
                if (statusText != null)
                {
                    statusText.Text += $"✗ {errorMsg}\r\n";
                }

                MessageBox.Show(errorMsg, "Update Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TextBox? GetStatusTextBox()
        {
            return tabControl?.TabPages.Cast<TabPage>()
                .FirstOrDefault(tab => tab.Text == "Import/Export")?
                .Controls.OfType<Panel>().FirstOrDefault()?
                .Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Name == "statusTextBox");
        }

        private void ShowAddDomainDialog(ListBox listBox)
        {
            var dialog = new EnhancedInputDialog("Enter domain to block:", "Add Domain", "example.com");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var input = dialog.InputText.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (!IsValidDomain(input))
                    {
                        MessageBox.Show("Please enter a valid domain name.", "Invalid Domain", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (Settings.BlockedDomains.Contains(input))
                    {
                        MessageBox.Show("This domain is already blocked.", "Duplicate Domain", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Settings.BlockedDomains.Add(input);
                    RefreshDomainsList(listBox);
                    UpdateDomainCount();
                    hasChanges = true;
                    UpdateStatusDisplay();
                }
            }
        }

        private void ShowAddKeywordDialog(ListBox listBox)
        {
            var dialog = new EnhancedInputDialog("Enter keyword to block:", "Add Keyword", "advertisement");
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var input = dialog.InputText.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (Settings.BlockedKeywords.Contains(input))
                    {
                        MessageBox.Show("This keyword is already blocked.", "Duplicate Keyword", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Settings.BlockedKeywords.Add(input);
                    RefreshKeywordsList(listBox);
                    UpdateKeywordCount();
                    hasChanges = true;
                    UpdateStatusDisplay();
                }
            }
        }

        private void RemoveSelectedDomains(ListBox listBox)
        {
            if (listBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select domains to remove.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove {listBox.SelectedItems.Count} selected domain(s)?", 
                "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var itemsToRemove = listBox.SelectedItems.Cast<string>().ToList();
                foreach (var item in itemsToRemove)
                {
                    Settings.BlockedDomains.Remove(item);
                }

                RefreshDomainsList(listBox);
                UpdateDomainCount();
                hasChanges = true;
                UpdateStatusDisplay();
            }
        }

        private void RemoveSelectedKeywords(ListBox listBox)
        {
            if (listBox.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select keywords to remove.", "No Selection", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove {listBox.SelectedItems.Count} selected keyword(s)?", 
                "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var itemsToRemove = listBox.SelectedItems.Cast<string>().ToList();
                foreach (var item in itemsToRemove)
                {
                    Settings.BlockedKeywords.Remove(item);
                }

                RefreshKeywordsList(listBox);
                UpdateKeywordCount();
                hasChanges = true;
                UpdateStatusDisplay();
            }
        }

        private void ClearAllDomains(ListBox listBox)
        {
            if (Settings.BlockedDomains.Count == 0)
            {
                MessageBox.Show("No domains to clear.", "Empty List", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove ALL {Settings.BlockedDomains.Count} domains?\n\nThis action cannot be undone.", 
                "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Settings.BlockedDomains.Clear();
                RefreshDomainsList(listBox);
                UpdateDomainCount();
                hasChanges = true;
                UpdateStatusDisplay();
            }
        }

        private void ClearAllKeywords(ListBox listBox)
        {
            if (Settings.BlockedKeywords.Count == 0)
            {
                MessageBox.Show("No keywords to clear.", "Empty List", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove ALL {Settings.BlockedKeywords.Count} keywords?\n\nThis action cannot be undone.", 
                "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Settings.BlockedKeywords.Clear();
                RefreshKeywordsList(listBox);
                UpdateKeywordCount();
                hasChanges = true;
                UpdateStatusDisplay();
            }
        }

        private void FilterDomainsList(ListBox listBox, string searchText)
        {
            listBox.Items.Clear();
            var filteredDomains = string.IsNullOrWhiteSpace(searchText) 
                ? Settings.BlockedDomains.OrderBy(d => d)
                : Settings.BlockedDomains.Where(d => d.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                         .OrderBy(d => d);

            foreach (var domain in filteredDomains)
            {
                listBox.Items.Add(domain);
            }
        }

        private void FilterKeywordsList(ListBox listBox, string searchText)
        {
            listBox.Items.Clear();
            var filteredKeywords = string.IsNullOrWhiteSpace(searchText) 
                ? Settings.BlockedKeywords.OrderBy(k => k)
                : Settings.BlockedKeywords.Where(k => k.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                          .OrderBy(k => k);

            foreach (var keyword in filteredKeywords)
            {
                listBox.Items.Add(keyword);
            }
        }

        private void EditSelectedDomain(ListBox listBox)
        {
            if (listBox.SelectedItem == null) return;

            var selectedDomain = listBox.SelectedItem.ToString();
            if (selectedDomain == null) return;

            var dialog = new EnhancedInputDialog("Edit domain:", "Edit Domain", selectedDomain);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var newDomain = dialog.InputText.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(newDomain) && newDomain != selectedDomain)
                {
                    if (!IsValidDomain(newDomain))
                    {
                        MessageBox.Show("Please enter a valid domain name.", "Invalid Domain", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (Settings.BlockedDomains.Contains(newDomain))
                    {
                        MessageBox.Show("This domain is already blocked.", "Duplicate Domain", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Settings.BlockedDomains.Remove(selectedDomain);
                    Settings.BlockedDomains.Add(newDomain);
                    RefreshDomainsList(listBox);
                    hasChanges = true;
                    UpdateStatusDisplay();
                }
            }
        }

        private void EditSelectedKeyword(ListBox listBox)
        {
            if (listBox.SelectedItem == null) return;

            var selectedKeyword = listBox.SelectedItem.ToString();
            if (selectedKeyword == null) return;

            var dialog = new EnhancedInputDialog("Edit keyword:", "Edit Keyword", selectedKeyword);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var newKeyword = dialog.InputText.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(newKeyword) && newKeyword != selectedKeyword)
                {
                    if (Settings.BlockedKeywords.Contains(newKeyword))
                    {
                        MessageBox.Show("This keyword is already blocked.", "Duplicate Keyword", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Settings.BlockedKeywords.Remove(selectedKeyword);
                    Settings.BlockedKeywords.Add(newKeyword);
                    RefreshKeywordsList(listBox);
                    hasChanges = true;
                    UpdateStatusDisplay();
                }
            }
        }

        private void CopySelectedDomainsToClipboard(ListBox listBox)
        {
            if (listBox.SelectedItems.Count > 0)
            {
                var selectedDomains = listBox.SelectedItems.Cast<string>();
                var text = string.Join(Environment.NewLine, selectedDomains);
                Clipboard.SetText(text);
                MessageBox.Show($"Copied {listBox.SelectedItems.Count} domains to clipboard.", "Copied", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CopySelectedKeywordsToClipboard(ListBox listBox)
        {
            if (listBox.SelectedItems.Count > 0)
            {
                var selectedKeywords = listBox.SelectedItems.Cast<string>();
                var text = string.Join(Environment.NewLine, selectedKeywords);
                Clipboard.SetText(text);
                MessageBox.Show($"Copied {listBox.SelectedItems.Count} keywords to clipboard.", "Copied", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateDomainCount()
        {
            var countLabel = tabControl?.TabPages.Cast<TabPage>()
                .FirstOrDefault(tab => tab.Text == "Blocked Domains")?
                .Controls.OfType<Panel>().FirstOrDefault()?
                .Controls.OfType<Label>().FirstOrDefault(l => l.Name == "domainCountLabel");
            
            if (countLabel != null)
            {
                countLabel.Text = $"Total: {Settings.BlockedDomains.Count} domains";
            }
        }

        private void UpdateKeywordCount()
        {
            var countLabel = tabControl?.TabPages.Cast<TabPage>()
                .FirstOrDefault(tab => tab.Text == "Blocked Keywords")?
                .Controls.OfType<Panel>().FirstOrDefault()?
                .Controls.OfType<Label>().FirstOrDefault(l => l.Name == "keywordCountLabel");
            
            if (countLabel != null)
            {
                countLabel.Text = $"Total: {Settings.BlockedKeywords.Count} keywords";
            }
        }

        private bool IsValidDomain(string domain)
        {
            return !string.IsNullOrWhiteSpace(domain) && 
                   domain.Contains('.') && 
                   !domain.Contains(' ') && 
                   domain.Length > 3;
        }

        private void UpdateStatusDisplay()
        {
            this.Text = hasChanges ? "Ad Blocker Settings Manager*" : "Ad Blocker Settings Manager";
            
            var statusLabel = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Bottom)?
                                 .Controls.OfType<Label>().FirstOrDefault(l => l.Name == "statusLabel");
            if (statusLabel != null)
            {
                UpdateStatusLabel(statusLabel);
            }
        }

        private void UpdateStatusLabel(Label statusLabel)
        {
            statusLabel.Text = hasChanges ? "Settings modified" : "No changes";
            statusLabel.ForeColor = hasChanges ? Color.Orange : Color.Green;
        }

        private void LoadSettings()
        {
            // Settings are already loaded in constructor
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            hasChanges = false;
            UpdateStatusDisplay();
            MessageBox.Show("Settings applied successfully!", "Apply Settings", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Helper classes for dialogs
    public class InputDialog : Form
    {
        public string InputText { get; private set; } = "";

        public InputDialog(string prompt, string title)
        {
            InitializeComponent(prompt, title);
        }

        private void InitializeComponent(string prompt, string title)
        {
            this.Text = title;
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var label = new Label
            {
                Text = prompt,
                Location = new Point(10, 15),
                Size = new Size(360, 20)
            };

            var textBox = new TextBox
            {
                Location = new Point(10, 40),
                Size = new Size(360, 25)
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(215, 80),
                Size = new Size(75, 25)
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(295, 80),
                Size = new Size(75, 25)
            };

            okButton.Click += (object? s, EventArgs e) => InputText = textBox.Text;

            this.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
        }
    }

    public class ProgressDialog : Form
    {
        public ProgressDialog(string message)
        {
            InitializeComponent(message);
        }

        private void InitializeComponent(string message)
        {
            this.Text = "Please Wait";
            this.Size = new Size(300, 100);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;

            var label = new Label
            {
                Text = message,
                Location = new Point(10, 20),
                Size = new Size(260, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var progressBar = new ProgressBar
            {
                Location = new Point(10, 45),
                Size = new Size(260, 20),
                Style = ProgressBarStyle.Marquee
            };

            this.Controls.AddRange(new Control[] { label, progressBar });
        }
    }
}