namespace WinFormsApp1
{
    public partial class HistoryForm : Form
    {
        public HistoryItem? SelectedHistory { get; private set; }
        private List<HistoryItem> history;
        private ListView listView;
        public HistoryForm(List<HistoryItem> history)
        {
            this.history = history;
            InitializeComponent();
            LoadHistory();
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItems.Count > 0)
            {
                SelectedHistory = (HistoryItem)listView.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        private void okButton_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                SelectedHistory = (HistoryItem)listView.SelectedItems[0].Tag;
            }
        }
        private void InitializeComponent()
        {
            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listView.Columns.Add("Title", 300);
            listView.Columns.Add("URL", 400);
            listView.Columns.Add("Visit Time", 150);

            listView.DoubleClick += ListView_DoubleClick;

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var okButton = new Button { Text = "Go", DialogResult = DialogResult.OK, Left = 10, Top = 8 };
            var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 100, Top = 8 };

            okButton.Click += okButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { okButton, cancelButton });
            Controls.AddRange(new Control[] { listView, buttonPanel });

            Text = "History";
            Size = new Size(600, 400);
            StartPosition = FormStartPosition.CenterParent;
        }

        private void LoadHistory()
        {
            var listView = (ListView)Controls[0];
            listView.Items.Clear();

            foreach (var historyItem in history)
            {
                var item = new ListViewItem(historyItem.Title);
                item.SubItems.Add(historyItem.Url);
                item.SubItems.Add(historyItem.VisitTime.ToString("yyyy-MM-dd HH:mm:ss"));
                item.Tag = historyItem;
                listView.Items.Add(item);
            }
        }
    }
}