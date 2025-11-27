// Enhanced InputDialog with better validation and UI
public class EnhancedInputDialog : Form
{
    public string InputText { get; private set; } = "";

    public EnhancedInputDialog(string prompt, string title, string defaultValue = "")
    {
        InitializeComponent(prompt, title, defaultValue);
    }

    private void InitializeComponent(string prompt, string title, string defaultValue)
    {
        this.Text = title;
        this.Size = new Size(450, 180);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        var label = new Label
        {
            Text = prompt,
            Location = new Point(15, 20),
            Size = new Size(400, 20),
            Font = new Font("Segoe UI", 9)
        };

        var textBox = new TextBox
        {
            Location = new Point(15, 50),
            Size = new Size(400, 25),
            Text = defaultValue,
            Font = new Font("Segoe UI", 9)
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(260, 90),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(350, 90),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        okButton.Click += (object? s, EventArgs e) => {
            InputText = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(InputText))
            {
                MessageBox.Show("Please enter a valid value.", "Invalid Input", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };

        textBox.KeyDown += (object? s, KeyEventArgs e) => {
            if (e.KeyCode == Keys.Enter)
            {
                okButton.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                cancelButton.PerformClick();
            }
        };

        // Focus the text box and select all text
        this.Load += (s, e) => {
            textBox.Focus();
            textBox.SelectAll();
        };

        this.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
        this.AcceptButton = okButton;
        this.CancelButton = cancelButton;
    }
}