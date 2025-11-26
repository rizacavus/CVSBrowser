namespace WinFormsApp1
{
    public partial class DownloadsForm : Form
    {
        private List<DownloadItem> downloads;

        public DownloadsForm(List<DownloadItem> downloads)
        {
            this.downloads = downloads;
            InitializeComponent();
            LoadDownloads();
        }

        private void InitializeComponent()
        {
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listView.Columns.Add("File Name", 200);
            listView.Columns.Add("Status", 100);
            listView.Columns.Add("Size", 100);
            listView.Columns.Add("Start Time", 150);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var closeButton = new Button { Text = "Close", DialogResult = DialogResult.OK, Left = 10, Top = 8 };

            buttonPanel.Controls.Add(closeButton);
            Controls.AddRange(new Control[] { listView, buttonPanel });

            Text = "Downloads";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
        }

        private void LoadDownloads()
        {
            var listView = (ListView)Controls[0];
            listView.Items.Clear();

            foreach (var download in downloads)
            {
                var item = new ListViewItem(download.FileName);
                item.SubItems.Add(download.Status);
                item.SubItems.Add(FormatFileSize(download.TotalBytes));
                item.SubItems.Add(download.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                listView.Items.Add(item);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}