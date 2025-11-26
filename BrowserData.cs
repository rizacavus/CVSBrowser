using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsApp1
{
    public class BookmarkItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }

    public class HistoryItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; } = DateTime.Now;
    }

    public class DownloadItem
    {
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public long ReceivedBytes { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; }
        public string Status { get; set; } = "Downloading";
    }

    public class BrowserTab
    {
        public string Title { get; set; } = "New Tab";
        public string Url { get; set; } = "about:blank";
        [JsonIgnore] // Don't serialize WebView control
        public Microsoft.Web.WebView2.WinForms.WebView2? WebView { get; set; }
        public bool CanGoBack { get; set; }
        public bool CanGoForward { get; set; }
        public bool IsLoading { get; set; }
    }

    // Serializable class for saving tab sessions
    public class TabSession
    {
        public string Title { get; set; } = "New Tab";
        public string Url { get; set; } = "about:blank";
        public DateTime LastAccess { get; set; } = DateTime.Now;
        public bool IsActiveTab { get; set; } = false;
    }

    // Container for all session tabs
    public class BrowserSession
    {
        public List<TabSession> Tabs { get; set; } = new();
        public DateTime LastSaved { get; set; } = DateTime.Now;
    }
}