using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.IO; // Add this line

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private List<BrowserTab> browserTabs = new();
        private List<BookmarkItem> bookmarks = new();
        private List<HistoryItem> history = new();
        private List<DownloadItem> downloads = new();
        private string homeUrl = "https://www.google.com";
        private string dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CVSBrowser");
        private bool isDarkMode = true;
        private bool isWebDarkModeEnabled = false;

        // Add Ad Blocker properties
        private bool isAdBlockerEnabled = true;

        // Ad Blocker filter lists
        private HashSet<string> adBlockDomains = new();
        private HashSet<string> adBlockKeywords = new();

        // Dark Mode CSS to inject into web pages
        private const string DarkModeCSS = @"
            (function() {
                if (document.getElementById('dark-mode-extension')) return;
                
                const style = document.createElement('style');
                style.id = 'dark-mode-extension';
                style.textContent = `
                    html { filter: invert(1) hue-rotate(180deg) !important; }
                    img, video, iframe, canvas, svg, embed, object { filter: invert(1) hue-rotate(180deg) !important; }
                    [style*=""""background-image""""] { filter: invert(1) hue-rotate(180deg) !important; }
                    input, textarea, select { background-color: #333 !important; color: #fff !important; }
                `;
                document.head.appendChild(style);
            })();
        ";

        private const string RemoveDarkModeCSS = @"
            (function() {
                const darkModeStyle = document.getElementById('dark-mode-extension');
                if (darkModeStyle) {
                    darkModeStyle.remove();
                }
            })();
        ";

        // Ad Blocker CSS and JavaScript
        private const string AdBlockerCSS = @"
            (function() {
                if (document.getElementById('ad-blocker-extension')) return;
                
                const style = document.createElement('style');
                style.id = 'ad-blocker-extension';
                style.textContent = `
                    /* Block common ad containers */
                    *[id*=""ad""], *[class*=""ad""], *[id*=""banner""], *[class*=""banner""],
                    *[id*=""popup""], *[class*=""popup""], *[id*=""sponsor""], *[class*=""sponsor""],
                    *[class*=""advertisement""], *[id*=""advertisement""], 
                    iframe[src*=""ads""], iframe[src*=""doubleclick""], iframe[src*=""googlesyndication""],
                    iframe[src*=""amazon-adsystem""], iframe[src*=""facebook.com/tr""],
                    div[class*=""AdArea""], div[id*=""AdArea""], 
                    div[class*=""adContainer""], div[id*=""adContainer""],
                    div[class*=""ad-container""], div[id*=""ad-container""],
                    div[class*=""adsystem""], div[id*=""adsystem""],
                    ins.adsbygoogle, .adsbygoogle,
                    /* Block social media widgets that track users */
                    iframe[src*=""facebook.com/plugins""], iframe[src*=""twitter.com/widgets""],
                    /* Block common ad networks */
                    *[src*=""doubleclick.net""], *[src*=""googleadservices.com""],
                    *[src*=""googlesyndication.com""], *[src*=""amazon-adsystem.com""],
                    *[src*=""adsystem.com""], *[src*=""ads.yahoo.com""],
                    *[src*=""advertising.com""], *[src*=""adsrvr.org""],
                    /* Block overlay ads and popups */
                    div[style*=""position: fixed""][style*=""z-index""],
                    div[class*=""overlay""][class*=""ad""], div[id*=""overlay""][id*=""ad""],
                    /* Block newsletter popups */
                    div[class*=""newsletter""][class*=""popup""], div[id*=""newsletter""][id*=""popup""],
                    div[class*=""subscribe""][class*=""modal""], div[id*=""subscribe""][id*=""modal""]
                    {
                        display: none !important;
                        visibility: hidden !important;
                        opacity: 0 !important;
                        width: 0 !important;
                        height: 0 !important;
                        margin: 0 !important;
                        padding: 0 !important;
                        border: none !important;
                        background: none !important;
                    }
                    
                    /* Remove ad placeholders */
                    .ad-placeholder, .advertisement-placeholder, 
                    .banner-placeholder, .sponsored-content {
                        display: none !important;
                    }
                    
                    /* Fix layout after removing ads */
                    body { 
                        overflow-x: auto !important; 
                    }
                `;
                document.head.appendChild(style);
            })();
        ";

        private const string AdBlockerJS = @"
            (function() {
                if (window.adBlockerActive) return;
                window.adBlockerActive = true;
                
                // Block common ad functions
                const noop = function() {};
                const noopStr = function() { return ''; };
                const noopArray = function() { return []; };
                const noopObj = function() { return {}; };
                
                // Override common ad networks
                window.googletag = window.googletag || { cmd: [], display: noop, enableServices: noop };
                window.pbjs = window.pbjs || { que: [], addAdUnits: noop, requestBids: noop };
                window.apntag = window.apntag || { anq: [], loadTags: noop };
                
                // Block Google AdSense
                if (typeof window.adsbygoogle !== 'undefined') {
                    window.adsbygoogle = [];
                }
                
                // Block Amazon ads
                window.amzn_assoc_ad = noop;
                window.amazon_ad_tag = noop;
                
                // Block Facebook tracking
                window.fbq = window.fbq || noop;
                window._fbq = window._fbq || noop;
                
                // Block Google Analytics and other trackers
                window.gtag = window.gtag || noop;
                window.ga = window.ga || noop;
                window._gaq = window._gaq || { push: noop };
                
                // Remove ad elements continuously
                const removeAds = function() {
                    const adSelectors = [
                        'iframe[src*=""ads""]', 'iframe[src*=""doubleclick""]',
                        'iframe[src*=""googlesyndication""]', 'iframe[src*=""amazon-adsystem""]',
                        '*[class*=""advertisement""]', '*[id*=""advertisement""]',
                        '*[class*=""ad-banner""]', '*[id*=""ad-banner""]',
                        '.adsbygoogle', 'ins.adsbygoogle',
                        '*[class*=""sponsor""]', '*[id*=""sponsor""]',
                        '*[class*=""popup""][style*=""position: fixed""]',
                        'div[class*=""overlay""][class*=""ad""]'
                    ];
                    
                    adSelectors.forEach(selector => {
                        document.querySelectorAll(selector).forEach(el => {
                            if (el && el.parentNode) {
                                el.parentNode.removeChild(el);
                            }
                        });
                    });
                    
                    // Remove elements with ad-related attributes
                    document.querySelectorAll('*').forEach(el => {
                        const className = el.className || '';
                        const id = el.id || '';
                        if (typeof className === 'string' && typeof id === 'string') {
                            if (className.toLowerCase().includes('ad') || 
                                id.toLowerCase().includes('ad') ||
                                className.toLowerCase().includes('banner') ||
                                id.toLowerCase().includes('banner') ||
                                className.toLowerCase().includes('popup') ||
                                id.toLowerCase().includes('popup')) {
                                
                                // Only remove if it's likely an ad (check size, content, etc.)
                                const rect = el.getBoundingClientRect();
                                if ((rect.width > 200 && rect.height > 100) || 
                                    el.tagName === 'IFRAME' ||
                                    el.innerHTML.toLowerCase().includes('advertisement')) {
                                    if (el.parentNode) {
                                        el.parentNode.removeChild(el);
                                    }
                                }
                            }
                        }
                    });
                };
                
                // Run ad removal on page load and periodically
                removeAds();
                setInterval(removeAds, 2000);
                
                // Block popup windows
                const originalOpen = window.open;
                window.open = function(url, name, features) {
                    // Allow opening if user initiated (check for recent click)
                    if (window.userRecentlyClicked) {
                        return originalOpen.call(this, url, name, features);
                    }
                    console.log('Blocked popup:', url);
                    return null;
                };
                
                // Track user clicks
                let clickTimer;
                document.addEventListener('click', function() {
                    window.userRecentlyClicked = true;
                    clearTimeout(clickTimer);
                    clickTimer = setTimeout(() => {
                        window.userRecentlyClicked = false;
                    }, 1000);
                }, true);
                
                // Block new window/tab creation from ads
                const originalCreateElement = document.createElement;
                document.createElement = function(tagName) {
                    const element = originalCreateElement.call(this, tagName);
                    if (tagName.toLowerCase() === 'a') {
                        element.addEventListener('click', function(e) {
                            if (this.target === '_blank' && !window.userRecentlyClicked) {
                                e.preventDefault();
                                console.log('Blocked ad link:', this.href);
                            }
                        });
                    }
                    return element;
                };
            })();
        ";

        private const string RemoveAdBlockerCSS = @"
            (function() {
                const adBlockerStyle = document.getElementById('ad-blocker-extension');
                if (adBlockerStyle) {
                    adBlockerStyle.remove();
                }
                window.adBlockerActive = false;
            })();
        ";

        public Form1()
        {
            InitializeComponent();

            // Only initialize if not in design mode
            if (!DesignMode)
            {
                InitializeDataFolder();
                LoadBrowserData();
                InitializeAdBlocker(); // Initialize ad blocker
                ThemeManager.CurrentTheme = ThemeMode.Dark;
                ThemeManager.ThemeChanged += OnThemeChanged;
                WindowState = FormWindowState.Normal;

                // Subscribe to new tab button event
                chromeTabControl.NewTabRequested += async (s, e) => await CreateNewTab(homeUrl);
            }
        }

        private void InitializeAdBlocker()
        {
            // Load ad blocker filters
            LoadAdBlockerFilters();

            // Update the ad blocker button state
            UpdateAdBlockerButton();
        }

        private void LoadAdBlockerFilters()
        {
            // Common ad domains to block
            adBlockDomains = new HashSet<string>
            {
                "doubleclick.net", "googleadservices.com", "googlesyndication.com",
                "amazon-adsystem.com", "adsystem.com", "ads.yahoo.com",
                "advertising.com", "adsrvr.org", "facebook.com/tr",
                "google-analytics.com", "googletagmanager.com",
                "scorecardresearch.com", "quantserve.com", "outbrain.com",
                "taboola.com", "addthis.com", "sharethis.com"
            };

            // Keywords to identify ad-related requests
            adBlockKeywords = new HashSet<string>
            {
                "advertisement", "banner", "popup", "sponsor", "tracking",
                "analytics", "doubleclick", "adsystem", "adservice",
                "/ads/", "/ad/", "/banner/", "/popup/"
            };
        }

        private void InitializeDataFolder()
        {
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);
        }

        private void OnThemeChanged(ThemeMode newTheme)
        {
            isDarkMode = newTheme == ThemeMode.Dark;
            UpdateChromeTheme();
            SaveBrowserData();
        }

        private void UpdateChromeTheme()
        {
            if (DesignMode) return;

            var isDark = ThemeManager.CurrentTheme == ThemeMode.Dark;

            tabStripPanel.BackColor = isDark ? Color.FromArgb(51, 51, 55) : Color.FromArgb(230, 230, 230);
            navigationPanel.BackColor = isDark ? Color.FromArgb(51, 51, 55) : Color.FromArgb(230, 230, 230);
            webContentPanel.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
            BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;

            foreach (Control control in navigationPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.ForeColor = isDark ? Color.White : Color.Black;
                }
            }

            // Update address bar colors
            if (addressBar?.Parent?.Parent is Panel addressBarBorder)
            {
                var addressBarContainer = addressBar.Parent;

                addressBar.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
                addressBar.ForeColor = isDark ? Color.White : Color.Black;
                addressBarContainer.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.White;
                addressBarBorder.BackColor = isDark ? Color.FromArgb(63, 63, 70) : Color.Gray;
            }

            chromeTabControl?.Invalidate();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                try
                {
                    // Load saved session first
                    var session = LoadBrowserSession();

                    if (session?.Tabs != null && session.Tabs.Count > 0)
                    {
                        // Restore saved tabs
                        await RestoreBrowserSession(session);
                    }
                    else
                    {
                        // No saved session, create default home tab
                        await CreateNewTab(homeUrl);
                    }

                    UpdateNavigationButtons();
                    UpdateChromeTheme();
                    UpdateDarkModeButton();
                    UpdateAdBlockerButton();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Form1_Load: {ex.Message}");
                    // If something fails, create at least one tab
                    if (browserTabs.Count == 0)
                    {
                        await CreateNewTab("https://www.google.com");
                    }
                }
            }
        }

        // Session Management Methods
        private async Task RestoreBrowserSession(BrowserSession session)
        {
            int activeTabIndex = 0;

            for (int i = 0; i < session.Tabs.Count; i++)
            {
                var tabSession = session.Tabs[i];
                await CreateNewTab(tabSession.Url, tabSession.Title);

                if (tabSession.IsActiveTab)
                {
                    activeTabIndex = i;
                }
            }

            // Set the previously active tab as current
            if (activeTabIndex < chromeTabControl.TabPages.Count)
            {
                chromeTabControl.SelectedIndex = activeTabIndex;
                ShowCurrentTabWebView();
                UpdateAddressBar();
            }
        }

        private BrowserSession? LoadBrowserSession()
        {
            try
            {
                var sessionFile = Path.Combine(dataFolder, "session.json");
                if (File.Exists(sessionFile))
                {
                    var json = File.ReadAllText(sessionFile);
                    return JsonSerializer.Deserialize<BrowserSession>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading browser session: {ex.Message}");
            }

            return null;
        }

        private void SaveBrowserSession()
        {
            try
            {
                var session = new BrowserSession
                {
                    LastSaved = DateTime.Now,
                    Tabs = new List<TabSession>()
                };

                var currentTabIndex = chromeTabControl.SelectedIndex;

                for (int i = 0; i < browserTabs.Count; i++)
                {
                    var tab = browserTabs[i];
                    if (!string.IsNullOrEmpty(tab.Url) && tab.Url != "about:blank")
                    {
                        session.Tabs.Add(new TabSession
                        {
                            Title = tab.Title,
                            Url = tab.Url,
                            LastAccess = DateTime.Now,
                            IsActiveTab = i == currentTabIndex
                        });
                    }
                }

                // Only save if we have tabs to save
                if (session.Tabs.Count > 0)
                {
                    var sessionFile = Path.Combine(dataFolder, "session.json");
                    var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(sessionFile, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving browser session: {ex.Message}");
            }
        }

        // Override ProcessCmdKey to handle keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                case Keys.Control | Keys.T:
                    // Ctrl+N or Ctrl+T - New Tab with Google.com
                    _ = CreateNewTab(homeUrl);
                    return true;

                case Keys.Control | Keys.W:
                    // Ctrl+W - Close Tab
                    var selectedIndex = chromeTabControl.SelectedIndex;
                    if (selectedIndex >= 0)
                    {
                        CloseTab(selectedIndex);
                    }
                    return true;

                case Keys.Control | Keys.R:
                case Keys.F5:
                    // Ctrl+R or F5 - Refresh
                    refreshButton_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Control | Keys.L:
                    // Ctrl+L - Focus address bar
                    addressBar.Focus();
                    addressBar.SelectAll();
                    return true;

                case Keys.F12:
                    // F12 - Developer Tools
                    GetCurrentTab()?.WebView?.CoreWebView2?.OpenDevToolsWindow();
                    return true;

                case Keys.Alt | Keys.Left:
                    // Alt+Left - Back
                    backButton_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Alt | Keys.Right:
                    // Alt+Right - Forward
                    forwardButton_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Control | Keys.D:
                    // Ctrl+D - Add Bookmark
                    bookmarkButton_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Control | Keys.Shift | Keys.T:
                    // Ctrl+Shift+T - Restore session (bonus feature)
                    _ = Task.Run(async () =>
                    {
                        var session = LoadBrowserSession();
                        if (session != null)
                        {
                            this.Invoke(async () => await RestoreBrowserSession(session));
                        }
                    });
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async Task<BrowserTab> CreateNewTab(string url = "about:blank", string title = "New Tab")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"CreateNewTab called with URL: {url}");

                var webView = new Microsoft.Web.WebView2.WinForms.WebView2
                {
                    Dock = DockStyle.Fill,
                    Visible = false
                };

                var tabPage = new TabPage(title)
                {
                    Name = $"tab_{browserTabs.Count}",
                    BackColor = Color.FromArgb(37, 37, 38),
                    UseVisualStyleBackColor = false
                };

                var browserTab = new BrowserTab
                {
                    Title = title,
                    Url = url,
                    WebView = webView
                };

                browserTabs.Add(browserTab);
                chromeTabControl.TabPages.Add(tabPage);
                webContentPanel.Controls.Add(webView);

                // Ensure WebView2 is initialized first!
                await webView.EnsureCoreWebView2Async(null);

                // Set up ad blocker for this WebView
                if (isAdBlockerEnabled)
                {
                    SetupAdBlockerForWebView(webView);
                }

                // Attach event handlers
                webView.CoreWebView2.NavigationStarting += (s, args) => OnNavigationStarting(browserTab, args);
                webView.CoreWebView2.NavigationCompleted += (s, args) => OnNavigationCompleted(browserTab, args);
                webView.CoreWebView2.DocumentTitleChanged += (s, args) => OnDocumentTitleChanged(browserTab);
                webView.CoreWebView2.DownloadStarting += (s, args) => OnDownloadStarting(args);
                webView.CoreWebView2.NewWindowRequested += (s, args) => OnNewWindowRequested(args);

                // Now select the tab and update UI
                chromeTabControl.SelectedTab = tabPage;
                ShowCurrentTabWebView();
                UpdateAddressBar();

                // Navigate if needed
                if (!string.IsNullOrEmpty(url) && url != "about:blank")
                {
                    if (!IsValidUrl(url))
                    {
                        url = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
                        browserTab.Url = url;
                        if (addressBar != null)
                            addressBar.Text = url;
                    }

                    System.Diagnostics.Debug.WriteLine($"Navigating WebView to: {url}");
                    webView.CoreWebView2.Navigate(url);
                }

                if (chromeTabControl.TabPages.Count > 1)
                    SaveBrowserSession();

                System.Diagnostics.Debug.WriteLine($"Tab created successfully. Total tabs: {browserTabs.Count}");
                return browserTab;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating tab: {ex.Message}");
                MessageBox.Show($"Error creating tab: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void SetupAdBlockerForWebView(Microsoft.Web.WebView2.WinForms.WebView2 webView)
        {
            // Add web resource request filter to block ads
            webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.WebResourceRequested += OnWebResourceRequested;
            
            // Enhanced popup blocking
            webView.CoreWebView2.WindowCloseRequested += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine("Window close requested - possible popup");
            };

            // Handle script errors through console messages instead
            //webView.CoreWebView2.ConsoleMessage += (s, args) =>
            //{
            //    if (args.Kind == CoreWebView2ConsoleMessageKind.Error)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Console error (possibly blocked ad script): {args.Message}");
            //    }
            //};
        }

        private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (!isAdBlockerEnabled) return;

            string uri = e.Request.Uri.ToLower();

            // Check if the request is for an ad
            bool isAd = adBlockDomains.Any(domain => uri.Contains(domain)) ||
                       adBlockKeywords.Any(keyword => uri.Contains(keyword));

            if (isAd)
            {
                System.Diagnostics.Debug.WriteLine($"Blocked ad request: {e.Request.Uri}");

                try
                {
                    // Replace this line:
                    // response.Headers.Add("Content-Type", "text/plain");

                    // With this line:
                    // Fix: Use CoreWebView2.CreateWebResourceResponse instead of e.Environment.CreateWebResourceResponse
                    var webView = sender as Microsoft.Web.WebView2.WinForms.WebView2;
                    if (webView?.CoreWebView2 != null)
                    {
                        var response = webView.CoreWebView2.Environment.CreateWebResourceResponse(
                            new MemoryStream(), 204, "No Content", "");
                        response.Headers.AppendHeader("Content-Type", "text/plain");
                        //response.Headers.Add("Content-Type", "text/plain");
                        e.Response = response;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error blocking ad request: {ex.Message}");
                    // Fallback: just let the request through if blocking fails
                }
            }
        }

        // Helper method to validate URLs
        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private void ShowCurrentTabWebView()
        {
            // Hide all WebView controls
            foreach (var tab in browserTabs)
            {
                if (tab.WebView != null)
                    tab.WebView.Visible = false;
            }

            // Show only the current tab's WebView
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView != null)
            {
                currentTab.WebView.Visible = true;
                currentTab.WebView.BringToFront();
            }
        }

        private void OnNavigationStarting(BrowserTab tab, CoreWebView2NavigationStartingEventArgs args)
        {
            this.Invoke(() =>
            {
                tab.IsLoading = true;
                if (progressBar != null)
                {
                    progressBar.Visible = true;
                    progressBar.Value = 0;
                }

                System.Diagnostics.Debug.WriteLine($"Navigation starting for tab: {args.Uri}");

                // Check if navigation should be blocked (malicious ads, etc.)
                if (isAdBlockerEnabled && IsAdUrl(args.Uri))
                {
                    System.Diagnostics.Debug.WriteLine($"Blocked navigation to ad URL: {args.Uri}");
                    args.Cancel = true;
                    return;
                }

                // Update the tab's URL immediately
                tab.Url = args.Uri;

                // Update address bar only for current tab
                if (GetCurrentTab() == tab && addressBar != null)
                {
                    addressBar.Text = args.Uri;
                }
            });
        }

        private bool IsAdUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            string lowerUrl = url.ToLower();
            return adBlockDomains.Any(domain => lowerUrl.Contains(domain)) ||
                   adBlockKeywords.Any(keyword => lowerUrl.Contains(keyword));
        }

        private async void OnNavigationCompleted(BrowserTab tab, CoreWebView2NavigationCompletedEventArgs args)
        {
            this.Invoke(async () =>
            {
                tab.IsLoading = false;
                tab.Url = tab.WebView?.CoreWebView2.Source ?? "";
                tab.CanGoBack = tab.WebView?.CoreWebView2.CanGoBack ?? false;
                tab.CanGoForward = tab.WebView?.CoreWebView2.CanGoForward ?? false;

                if (progressBar != null)
                    progressBar.Visible = false;

                System.Diagnostics.Debug.WriteLine($"Navigation completed: {tab.Url}, Success: {args.IsSuccess}");

                if (args.IsSuccess)
                {
                    if (!string.IsNullOrEmpty(tab.Url) && tab.Url != "about:blank")
                    {
                        AddToHistory(tab.Title, tab.Url);
                        SaveBrowserSession();

                        // Apply extensions if enabled
                        if (isWebDarkModeEnabled && tab.WebView != null)
                        {
                            try
                            {
                                await tab.WebView.CoreWebView2.ExecuteScriptAsync(DarkModeCSS);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to apply dark mode: {ex.Message}");
                            }
                        }

                        // Apply ad blocker if enabled
                        if (isAdBlockerEnabled && tab.WebView != null)
                        {
                            try
                            {
                                await tab.WebView.CoreWebView2.ExecuteScriptAsync(AdBlockerCSS);
                                await tab.WebView.CoreWebView2.ExecuteScriptAsync(AdBlockerJS);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to apply ad blocker: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation failed for: {tab.Url}");
                }

                // Update UI only for current tab
                if (GetCurrentTab() == tab)
                {
                    UpdateAddressBar();
                    UpdateNavigationButtons();
                }
            });
        }

        private void OnDocumentTitleChanged(BrowserTab tab)
        {
            this.Invoke(() =>
            {
                var title = tab.WebView?.CoreWebView2.DocumentTitle ?? "New Tab";
                tab.Title = title;

                var tabIndex = browserTabs.IndexOf(tab);
                if (tabIndex >= 0 && tabIndex < chromeTabControl.TabPages.Count)
                {
                    chromeTabControl.TabPages[tabIndex].Text = title;
                    chromeTabControl.Invalidate();
                }

                SaveBrowserSession();
            });
        }

        private async void OnNewWindowRequested(CoreWebView2NewWindowRequestedEventArgs args)
        {
            // Check if this is a popup ad
            if (isAdBlockerEnabled && IsAdUrl(args.Uri))
            {
                System.Diagnostics.Debug.WriteLine($"Blocked popup ad: {args.Uri}");
                args.Handled = true;
                return;
            }

            args.Handled = true;
            await CreateNewTab(args.Uri);
        }

        private void OnDownloadStarting(CoreWebView2DownloadStartingEventArgs args)
        {
            var downloadOperation = args.DownloadOperation;
            var downloadItem = new DownloadItem
            {
                FileName = args.ResultFilePath.Split('\\').Last(),
                Url = downloadOperation.Uri,
                FilePath = args.ResultFilePath,
                TotalBytes = (long)(downloadOperation.TotalBytesToReceive ?? 0)
            };

            downloads.Add(downloadItem);
            SaveBrowserData();
        }

        private void UpdateAddressBar()
        {
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView?.CoreWebView2 != null && addressBar != null)
            {
                if (string.IsNullOrEmpty(currentTab.Url) || currentTab.Url == "about:blank")
                {
                    addressBar.Text = "";
                }
                else
                {
                    addressBar.Text = currentTab.Url;
                }
            }
            else if (addressBar != null)
            {
                addressBar.Text = "";
            }
        }

        private void UpdateNavigationButtons()
        {
            var currentTab = GetCurrentTab();

            if (backButton != null)
            {
                backButton.Enabled = currentTab?.CanGoBack ?? false;
                backButton.ForeColor = backButton.Enabled ? Color.White : Color.Gray;
            }

            if (forwardButton != null)
            {
                forwardButton.Enabled = currentTab?.CanGoForward ?? false;
                forwardButton.ForeColor = forwardButton.Enabled ? Color.White : Color.Gray;
            }
        }

        private void UpdateDarkModeButton()
        {
            if (darkModeExtensionButton != null)
            {
                darkModeExtensionButton.Text = isWebDarkModeEnabled ? "🌞" : "🌙";
                darkModeExtensionButton.ForeColor = isWebDarkModeEnabled ? Color.Gold : Color.White;
            }
        }

        private void UpdateAdBlockerButton()
        {
            if (adBlockerButton != null)
            {
                adBlockerButton.Text = isAdBlockerEnabled ? "🛡️" : "🚫";
                adBlockerButton.ForeColor = isAdBlockerEnabled ? Color.LightGreen : Color.Gray;
                adBlockerButton.BackColor = isAdBlockerEnabled ? Color.FromArgb(30, 144, 255, 50) : Color.Transparent;
            }
        }

        private BrowserTab? GetCurrentTab()
        {
            var selectedIndex = chromeTabControl.SelectedIndex;
            return selectedIndex >= 0 && selectedIndex < browserTabs.Count ? browserTabs[selectedIndex] : null;
        }

        private void AddToHistory(string title, string url)
        {
            var existingItem = history.FirstOrDefault(h => h.Url == url);
            if (existingItem != null)
            {
                existingItem.VisitTime = DateTime.Now;
            }
            else
            {
                history.Insert(0, new HistoryItem { Title = title, Url = url, VisitTime = DateTime.Now });
            }

            if (history.Count > 1000)
            {
                history = history.Take(1000).ToList();
            }

            SaveBrowserData();
        }

        private void NavigateToUrl()
        {
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView?.CoreWebView2 == null)
            {
                System.Diagnostics.Debug.WriteLine("NavigateToUrl: No current tab or WebView not ready");
                return;
            }

            string url = addressBar.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("file://"))
            {
                if (url.Contains('.') && !url.Contains(' '))
                {
                    url = "https://" + url;
                }
                else
                {
                    url = $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                }
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"NavigateToUrl: Navigating from '{currentTab.Url}' to '{url}'");
                currentTab.Url = url;
                currentTab.WebView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"NavigateToUrl error: {ex}");
            }
        }

        // Dark Mode Extension Event Handler
        private async void darkModeExtensionButton_Click(object sender, EventArgs e)
        {
            try
            {
                isWebDarkModeEnabled = !isWebDarkModeEnabled;
                UpdateDarkModeButton();

                var currentTab = GetCurrentTab();
                if (currentTab?.WebView?.CoreWebView2 != null)
                {
                    if (isWebDarkModeEnabled)
                    {
                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(DarkModeCSS);
                    }
                    else
                    {
                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(RemoveDarkModeCSS);
                    }
                }

                SaveBrowserData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling dark mode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ad Blocker Extension Event Handler
        private async void AdBlockerButton_Click(object sender, EventArgs e)
        {
            try
            {
                isAdBlockerEnabled = !isAdBlockerEnabled;
                UpdateAdBlockerButton();

                var currentTab = GetCurrentTab();
                if (currentTab?.WebView?.CoreWebView2 != null)
                {
                    if (isAdBlockerEnabled)
                    {
                        // Enable ad blocker
                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(AdBlockerCSS);
                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(AdBlockerJS);

                        // Set up request filtering for existing tabs
                        foreach (var tab in browserTabs)
                        {
                            if (tab.WebView?.CoreWebView2 != null)
                            {
                                try
                                {
                                    SetupAdBlockerForWebView(tab.WebView);
                                }
                                catch (Exception filterEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error setting up ad blocker for existing tab: {filterEx.Message}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Disable ad blocker
                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(RemoveAdBlockerCSS);

                        // Note: Cannot remove WebResourceRequested filters dynamically
                        // They will be removed when new tabs are created
                    }
                }

                SaveBrowserData();

                // Show notification
                string message = isAdBlockerEnabled ? "Ad Blocker Enabled" : "Ad Blocker Disabled";
                MessageBox.Show($"{message}\n\nThe ad blocker will take full effect on newly loaded pages.",
                    "Ad Blocker", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling ad blocker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"AdBlocker error: {ex}");
            }
        }

        // Event Handlers
        private void addressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToUrl();
            }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView?.CoreWebView2?.CanGoBack == true)
            {
                currentTab.WebView.CoreWebView2.GoBack();
            }
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView?.CoreWebView2?.CanGoForward == true)
            {
                currentTab.WebView.CoreWebView2.GoForward();
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            var currentTab = GetCurrentTab();
            currentTab?.WebView?.CoreWebView2?.Reload();
        }

        private void homeButton_Click(object sender, EventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView?.CoreWebView2 != null)
            {
                try
                {
                    currentTab.WebView.CoreWebView2.Navigate(homeUrl);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void ChromeTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrentTabWebView();
            UpdateAddressBar();
            UpdateNavigationButtons();
            SaveBrowserSession();
        }
        private void ChromeTabControl_MouseClick(object sender, MouseEventArgs e)
        {
            var chromeTab = (ChromeTabControl)sender;

            // Check for tab close button click
            for (int i = 0; i < chromeTab.TabCount; i++)
            {
                var tabRect = chromeTab.GetTabRect(i);
                var closeRect = new Rectangle(tabRect.Right - 20, tabRect.Y + 10, 12, 12);

                if (closeRect.Contains(e.Location))
                {
                    CloseTab(i);
                    return;
                }
            }

            // Middle click to close tab
            if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < chromeTab.TabCount; i++)
                {
                    if (chromeTab.GetTabRect(i).Contains(e.Location))
                    {
                        CloseTab(i);
                        return;
                    }
                }
            }
        }

        private void ChromeTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Handled by custom ChromeTabControl paint method
        }

        private void CloseTab(int index)
        {
            if (index >= 0 && index < browserTabs.Count)
            {
                if (browserTabs.Count > 1)
                {
                    var tabToClose = browserTabs[index];

                    // Remove WebView from webContentPanel
                    if (tabToClose.WebView != null)
                    {
                        webContentPanel.Controls.Remove(tabToClose.WebView);
                        tabToClose.WebView.Dispose();
                    }

                    browserTabs.RemoveAt(index);
                    chromeTabControl.TabPages.RemoveAt(index);

                    // Show the current tab after closing
                    ShowCurrentTabWebView();
                    UpdateAddressBar();
                    UpdateNavigationButtons();
                    SaveBrowserSession();
                }
                else
                {
                    Close();
                }
            }
        }

        private void bookmarkButton_Click(object sender, EventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab?.WebView != null && !string.IsNullOrEmpty(currentTab.Url))
            {
                var bookmark = new BookmarkItem
                {
                    Title = currentTab.Title,
                    Url = currentTab.Url,
                    DateAdded = DateTime.Now
                };

                if (!bookmarks.Any(b => b.Url == bookmark.Url))
                {
                    bookmarks.Add(bookmark);
                    SaveBrowserData();

                    if (bookmarkButton != null)
                    {
                        bookmarkButton.ForeColor = Color.Gold;
                        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
                        timer.Tick += (s, args) =>
                        {
                            bookmarkButton.ForeColor = Color.White;
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                }
            }
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            try
            {
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("New Tab (Ctrl+N)", null, async (s, args) => await CreateNewTab(homeUrl));
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Bookmarks", null, (s, args) => ShowBookmarksDialog());
                contextMenu.Items.Add("History", null, (s, args) => ShowHistoryDialog());
                contextMenu.Items.Add("Downloads", null, (s, args) => ShowDownloadsDialog());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add($"Ad Blocker: {(isAdBlockerEnabled ? "ON" : "OFF")}", null, async (s, args) => AdBlockerButton_Click(this, EventArgs.Empty));
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Restore Session (Ctrl+Shift+T)", null, async (s, args) =>
                {
                    var session = LoadBrowserSession();
                    if (session != null) await RestoreBrowserSession(session);
                });
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Settings", null, (s, args) => ShowSettingsDialog());
                contextMenu.Items.Add("Developer Tools (F12)", null, (s, args) => GetCurrentTab()?.WebView?.CoreWebView2?.OpenDevToolsWindow());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Exit", null, (s, args) => Close());

                // Apply theme safely
                if (!DesignMode)
                {
                    ThemeManager.ApplyTheme(contextMenu);
                }

                if (settingsButton != null)
                    contextMenu.Show(settingsButton, new Point(0, settingsButton.Height));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing settings menu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowBookmarksDialog()
        {
            try
            {
                // Create a simple bookmarks form if BookmarksForm doesn't exist
                var bookmarksForm = CreateSimpleBookmarksForm();
                bookmarksForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing bookmarks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Form CreateSimpleBookmarksForm()
        {
            var form = new Form
            {
                Text = "Bookmarks",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill
            };

            foreach (var bookmark in bookmarks)
            {
                listBox.Items.Add($"{bookmark.Title} - {bookmark.Url}");
            }

            listBox.DoubleClick += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var bookmark = bookmarks[listBox.SelectedIndex];
                    var currentTab = GetCurrentTab();
                    if (currentTab?.WebView?.CoreWebView2 != null)
                    {
                        currentTab.WebView.CoreWebView2.Navigate(bookmark.Url);
                        form.Close();
                    }
                }
            };

            form.Controls.Add(listBox);
            return form;
        }

        private void ShowHistoryDialog()
        {
            try
            {
                var historyForm = CreateSimpleHistoryForm();
                historyForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Form CreateSimpleHistoryForm()
        {
            var form = new Form
            {
                Text = "History",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill
            };

            foreach (var item in history.Take(50)) // Show recent 50 items
            {
                listBox.Items.Add($"{item.VisitTime:yyyy-MM-dd HH:mm} - {item.Title} - {item.Url}");
            }

            listBox.DoubleClick += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    var historyItem = history[listBox.SelectedIndex];
                    var currentTab = GetCurrentTab();
                    if (currentTab?.WebView?.CoreWebView2 != null)
                    {
                        currentTab.WebView.CoreWebView2.Navigate(historyItem.Url);
                        form.Close();
                    }
                }
            };

            form.Controls.Add(listBox);
            return form;
        }

        private void ShowDownloadsDialog()
        {
            try
            {
                MessageBox.Show($"Downloads ({downloads.Count} items)\n\nThis feature will be enhanced in future updates.",
                    "Downloads", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing downloads: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ShowSettingsDialog()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Opening settings with values: homeUrl={homeUrl}, isDarkMode={isDarkMode}, isWebDarkModeEnabled={isWebDarkModeEnabled}, isAdBlockerEnabled={isAdBlockerEnabled}");

                var settingsForm = new SettingsForm(homeUrl, isDarkMode, isWebDarkModeEnabled, isAdBlockerEnabled);

                // Remove the problematic ThemeManager.ApplyTheme line for now
                // if (!DesignMode)
                // {
                //     ThemeManager.ApplyTheme(settingsForm);
                // }

                var result = settingsForm.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"Dialog result: {result}");

                if (result == DialogResult.OK)
                {
                    System.Diagnostics.Debug.WriteLine($"Applying settings: homeUrl={settingsForm.HomeUrl}, isDarkMode={settingsForm.IsDarkMode}");

                    homeUrl = settingsForm.HomeUrl;
                    if (settingsForm.IsDarkMode != isDarkMode)
                    {
                        ThemeManager.CurrentTheme = settingsForm.IsDarkMode ? ThemeMode.Dark : ThemeMode.Light;
                    }
                    if (settingsForm.IsWebDarkModeEnabled != isWebDarkModeEnabled)
                    {
                        isWebDarkModeEnabled = settingsForm.IsWebDarkModeEnabled;
                        UpdateDarkModeButton();

                        // Apply to current tab
                        var currentTab = GetCurrentTab();
                        if (currentTab?.WebView?.CoreWebView2 != null)
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    if (isWebDarkModeEnabled)
                                    {
                                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(DarkModeCSS);
                                    }
                                    else
                                    {
                                        await currentTab.WebView.CoreWebView2.ExecuteScriptAsync(RemoveDarkModeCSS);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Failed to toggle web dark mode: {ex.Message}");
                                }
                            });
                        }
                    }
                    if (settingsForm.IsAdBlockerEnabled != isAdBlockerEnabled)
                    {
                        isAdBlockerEnabled = settingsForm.IsAdBlockerEnabled;
                        UpdateAdBlockerButton();
                    }
                    SaveBrowserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"ShowSettingsDialog error: {ex}");
            }
        }

        private void LoadBrowserData()
        {
            try
            {
                var bookmarksFile = Path.Combine(dataFolder, "bookmarks.json");
                if (File.Exists(bookmarksFile))
                {
                    var json = File.ReadAllText(bookmarksFile);
                    bookmarks = JsonSerializer.Deserialize<List<BookmarkItem>>(json) ?? new();
                }

                var historyFile = Path.Combine(dataFolder, "history.json");
                if (File.Exists(historyFile))
                {
                    var json = File.ReadAllText(historyFile);
                    history = JsonSerializer.Deserialize<List<HistoryItem>>(json) ?? new();
                }

                var settingsFile = Path.Combine(dataFolder, "settings.json");
                if (File.Exists(settingsFile))
                {
                    var json = File.ReadAllText(settingsFile);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (settings?.TryGetValue("homeUrl", out var savedHomeUrl) == true)
                    {
                        homeUrl = savedHomeUrl?.ToString() ?? homeUrl;
                    }
                    if (settings?.TryGetValue("isDarkMode", out var savedDarkMode) == true)
                    {
                        if (bool.TryParse(savedDarkMode?.ToString(), out bool darkMode))
                        {
                            isDarkMode = darkMode;
                            ThemeManager.CurrentTheme = darkMode ? ThemeMode.Dark : ThemeMode.Light;
                        }
                    }
                    if (settings?.TryGetValue("isWebDarkModeEnabled", out var savedWebDarkMode) == true)
                    {
                        if (bool.TryParse(savedWebDarkMode?.ToString(), out bool webDarkMode))
                        {
                            isWebDarkModeEnabled = webDarkMode;
                        }
                    }
                    if (settings?.TryGetValue("isAdBlockerEnabled", out var savedAdBlocker) == true)
                    {
                        if (bool.TryParse(savedAdBlocker?.ToString(), out bool adBlocker))
                        {
                            isAdBlockerEnabled = adBlocker;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading browser data: {ex.Message}");
            }
        }

        private void SaveBrowserData()
        {
            try
            {
                var bookmarksFile = Path.Combine(dataFolder, "bookmarks.json");
                File.WriteAllText(bookmarksFile, JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));

                var historyFile = Path.Combine(dataFolder, "history.json");
                File.WriteAllText(historyFile, JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true }));

                var settingsFile = Path.Combine(dataFolder, "settings.json");
                var settings = new Dictionary<string, object>
                {
                    ["homeUrl"] = homeUrl,
                    ["isDarkMode"] = isDarkMode,
                    ["isWebDarkModeEnabled"] = isWebDarkModeEnabled,
                    ["isAdBlockerEnabled"] = isAdBlockerEnabled
                };
                File.WriteAllText(settingsFile, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving browser data: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveBrowserSession();
            SaveBrowserData();

            foreach (var tab in browserTabs)
            {
                tab.WebView?.Dispose();
            }
            base.OnFormClosing(e);
        }

        // Dragging functionality
        private bool isDragging = false;
        private Point dragStartPoint;

        private void chromeTabControl_DragEnter(object sender, DragEventArgs e)
        {
            // This method is for file drag-and-drop operations
        }

        private void chromeTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var chromeTab = (ChromeTabControl)sender;
                bool clickedOnTab = false;

                for (int i = 0; i < chromeTab.TabCount; i++)
                {
                    if (chromeTab.GetTabRect(i).Contains(e.Location))
                    {
                        clickedOnTab = true;
                        break;
                    }
                }

                if (!clickedOnTab && chromeTab.GetNewTabButtonRect().Contains(e.Location))
                {
                    clickedOnTab = true;
                }

                if (!clickedOnTab)
                {
                    isDragging = true;
                    dragStartPoint = new Point(e.X, e.Y);
                }
            }
        }

        private void chromeTabControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - dragStartPoint.X, currentScreenPos.Y - dragStartPoint.Y);
            }
        }

        private void chromeTabControl_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - dragStartPoint.X, currentScreenPos.Y - dragStartPoint.Y);
            }
        }

        private void tabStripPanel_DoubleClick(object sender, EventArgs e)
        {
            maximizeButton.PerformClick();
        }

        private void tabStripPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void tabStripPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - dragStartPoint.X, currentScreenPos.Y - dragStartPoint.Y);
            }
        }

        private void tabStripPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }
    }
}