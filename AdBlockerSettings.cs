using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsApp1
{
    public class AdBlockerSettings
    {
        public bool IsEnabled { get; set; } = true;
        public HashSet<string> BlockedDomains { get; set; } = new();
        public HashSet<string> BlockedKeywords { get; set; } = new();
        public string CustomCSS { get; set; } = string.Empty;
        public string CustomJavaScript { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";

        [JsonIgnore]
        public string DefaultCSS => @"
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

        [JsonIgnore]
        public string DefaultJavaScript => @"
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

        public AdBlockerSettings()
        {
            LoadDefaultSettings();
        }

        public void LoadDefaultSettings()
        {
            BlockedDomains = new HashSet<string>
            {
                "doubleclick.net", "googleadservices.com", "googlesyndication.com",
                "amazon-adsystem.com", "adsystem.com", "ads.yahoo.com",
                "advertising.com", "adsrvr.org", "facebook.com/tr",
                "google-analytics.com", "googletagmanager.com",
                "scorecardresearch.com", "quantserve.com", "outbrain.com",
                "taboola.com", "addthis.com", "sharethis.com"
            };

            BlockedKeywords = new HashSet<string>
            {
                "advertisement", "banner", "popup", "sponsor", "tracking",
                "analytics", "doubleclick", "adsystem", "adservice",
                "/ads/", "/ad/", "/banner/", "/popup/"
            };

            CustomCSS = DefaultCSS;
            CustomJavaScript = DefaultJavaScript;
        }

        public string GetDynamicCSS()
        {
            var domainSelectors = string.Join(", ", BlockedDomains.Select(d => $"*[src*=\"{d}\"]"));
            var keywordSelectors = string.Join(", ", BlockedKeywords.Where(k => !k.StartsWith("/")).Select(k => $"*[class*=\"{k}\"], *[id*=\"{k}\"]"));
            
            return $@"
                (function() {{
                    if (document.getElementById('ad-blocker-extension')) return;
                    
                    const style = document.createElement('style');
                    style.id = 'ad-blocker-extension';
                    style.textContent = `
                        /* Block specific domains */
                        {domainSelectors}
                        {{
                            display: none !important;
                            visibility: hidden !important;
                            opacity: 0 !important;
                            width: 0 !important;
                            height: 0 !important;
                        }}
                        
                        /* Block keyword-based elements */
                        {keywordSelectors}
                        {{
                            display: none !important;
                            visibility: hidden !important;
                            opacity: 0 !important;
                        }}
                        
                        
                        
                        /* Remove ad placeholders */
                        .ad-placeholder, .advertisement-placeholder, 
                        .banner-placeholder, .sponsored-content {{
                            display: none !important;
                        }}
                        
                        /* Fix layout after removing ads */
                        body {{ 
                            overflow-x: auto !important; 
                        }}
                    `;
                    document.head.appendChild(style);
                }})();
            ";
        }

        public string GetDynamicJavaScript()
        {
            var domainArray = string.Join(", ", BlockedDomains.Select(d => $"\"{d}\""));
            var keywordArray = string.Join(", ", BlockedKeywords.Select(k => $"\"{k}\""));
            
            return $@"
                (function() {{
                    if (window.adBlockerActive) return;
                    window.adBlockerActive = true;
                    
                    // Dynamic blocked domains and keywords
                    const blockedDomains = [{domainArray}];
                    const blockedKeywords = [{keywordArray}];
                    
                    // Block common ad functions
                    const noop = function() {{}};
                    const noopStr = function() {{ return ''; }};
                    const noopArray = function() {{ return []; }};
                    const noopObj = function() {{ return {{}}; }};
                    
                    // Override common ad networks
                    window.googletag = window.googletag || {{ cmd: [], display: noop, enableServices: noop }};
                    window.pbjs = window.pbjs || {{ que: [], addAdUnits: noop, requestBids: noop }};
                    window.apntag = window.apntag || {{ anq: [], loadTags: noop }};
                    
                    // Block Google AdSense
                    if (typeof window.adsbygoogle !== 'undefined') {{
                        window.adsbygoogle = [];
                    }}
                    
                    // Block trackers
                    window.fbq = window.fbq || noop;
                    window._fbq = window._fbq || noop;
                    window.gtag = window.gtag || noop;
                    window.ga = window.ga || noop;
                    window._gaq = window._gaq || {{ push: noop }};
                    
                    // Remove ad elements continuously
                    const removeAds = function() {{
                        // Remove by domains
                        blockedDomains.forEach(domain => {{
                            document.querySelectorAll(`*[src*=""${{domain}}""]`).forEach(el => {{
                                if (el && el.parentNode) {{
                                    el.parentNode.removeChild(el);
                                }}
                            }});
                        }});
                        
                        // Remove by keywords
                        blockedKeywords.forEach(keyword => {{
                            document.querySelectorAll(`*[class*=""${{keyword}}""], *[id*=""${{keyword}}""]`).forEach(el => {{
                                if (el && el.parentNode) {{
                                    const rect = el.getBoundingClientRect();
                                    if ((rect.width > 200 && rect.height > 100) || 
                                        el.tagName === 'IFRAME' ||
                                        el.innerHTML.toLowerCase().includes('advertisement')) {{
                                        el.parentNode.removeChild(el);
                                    }}
                                }}
                            }});
                        }});
                        
                        // Default ad selectors
                        const adSelectors = [
                            'iframe[src*=""ads""]', 'iframe[src*=""doubleclick""]',
                            'iframe[src*=""googlesyndication""]', 'iframe[src*=""amazon-adsystem""]',
                            '*[class*=""advertisement""]', '*[id*=""advertisement""]',
                            '.adsbygoogle', 'ins.adsbygoogle'
                        ];
                        
                        adSelectors.forEach(selector => {{
                            document.querySelectorAll(selector).forEach(el => {{
                                if (el && el.parentNode) {{
                                    el.parentNode.removeChild(el);
                                }}
                            }});
                        }});
                    }};
                    
                    // Run ad removal on page load and periodically
                    removeAds();
                    setInterval(removeAds, 2000);
                    
                    // Block popup windows
                    const originalOpen = window.open;
                    window.open = function(url, name, features) {{
                        // Check if URL contains blocked domains
                        if (blockedDomains.some(domain => url.toLowerCase().includes(domain.toLowerCase()))) {{
                            console.log('Blocked popup:', url);
                            return null;
                        }}
                        
                        // Allow opening if user initiated
                        if (window.userRecentlyClicked) {{
                            return originalOpen.call(this, url, name, features);
                        }}
                        console.log('Blocked popup:', url);
                        return null;
                    }};
                    
                    // Track user clicks
                    let clickTimer;
                    document.addEventListener('click', function() {{
                        window.userRecentlyClicked = true;
                        clearTimeout(clickTimer);
                        clickTimer = setTimeout(() => {{
                            window.userRecentlyClicked = false;
                        }}, 1000);
                    }}, true);
                }})();
            ";
        }

        public string GetEffectiveCSS()
        {
            if (!string.IsNullOrEmpty(CustomCSS) && CustomCSS != DefaultCSS)
            {
                return CustomCSS; // User has custom CSS
            }
            
            // Generate dynamic CSS based on current domains and keywords
            return GetDynamicCSS();
        }

        public string GetEffectiveJavaScript()
        {
            if (!string.IsNullOrEmpty(CustomJavaScript) && CustomJavaScript != DefaultJavaScript)
            {
                return CustomJavaScript; // User has custom JS
            }
            
            // Generate dynamic JavaScript based on current domains and keywords
            return GetDynamicJavaScript();
        }

        public static AdBlockerSettings LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<AdBlockerSettings>(json);
                    if (settings != null)
                    {
                        settings.LastUpdated = DateTime.Now;
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ad blocker settings: {ex.Message}");
            }

            return new AdBlockerSettings();
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                LastUpdated = DateTime.Now;
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving ad blocker settings: {ex.Message}");
                throw;
            }
        }

        public void ExportToFile(string filePath)
        {
            SaveToFile(filePath);
        }

        public static AdBlockerSettings ImportFromFile(string filePath)
        {
            return LoadFromFile(filePath);
        }
    }
}