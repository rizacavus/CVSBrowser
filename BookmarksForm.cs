namespace WinFormsApp1
{
    public partial class BookmarksForm : Form
    {
        public BookmarkItem? SelectedBookmark { get; private set; }
        private List<BookmarkItem> bookmarks;

        public BookmarksForm(List<BookmarkItem> bookmarks)
        {
            this.bookmarks = bookmarks;
            InitializeComponent();
            LoadBookmarks();
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

            listView.Columns.Add("Title", 300);
            listView.Columns.Add("URL", 400);
            listView.Columns.Add("Date Added", 150);

            listView.DoubleClick += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    SelectedBookmark = (BookmarkItem)listView.SelectedItems[0].Tag;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var okButton = new Button { Text = "Go", DialogResult = DialogResult.OK, Left = 10, Top = 8 };
            var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 100, Top = 8 };

            okButton.Click += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    SelectedBookmark = (BookmarkItem)listView.SelectedItems[0].Tag;
                }
            };

            buttonPanel.Controls.AddRange(new Control[] { okButton, cancelButton });
            Controls.AddRange(new Control[] { listView, buttonPanel });

            Text = "Bookmarks";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
        }

        private void LoadBookmarks()
        {
            var listView = (ListView)Controls[0];
            listView.Items.Clear();

            foreach (var bookmark in bookmarks)
            {
                var item = new ListViewItem(bookmark.Title);
                item.SubItems.Add(bookmark.Url);
                item.SubItems.Add(bookmark.DateAdded.ToString("yyyy-MM-dd HH:mm"));
                item.Tag = bookmark;
                listView.Items.Add(item);
            }
        }
    }
}