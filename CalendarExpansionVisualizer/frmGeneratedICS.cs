using System.Text;

namespace CalendarExpansionVisualizer
{
    public sealed partial class frmGeneratedICS : Form
    {
        public frmGeneratedICS()
        {
            InitializeComponent();
            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        public frmGeneratedICS(string subject, string icsText)
            : this()
        {
            lblTitle.Text = string.IsNullOrWhiteSpace(subject)
                ? "Generated ICS"
                : $"Generated ICS - {subject}";
            SetIcsText(icsText);
            txtIcs.SelectionStart = 0;
            txtIcs.SelectionLength = 0;
        }

        private void SetIcsText(string icsText)
        {
            txtIcs.SuspendLayout();
            try
            {
                txtIcs.Text = icsText;
                txtIcs.SelectAll();
                txtIcs.SelectionBackColor = Color.White;

                Color eventMarkerColor = Color.LightGreen;
                Color uidMarkerColor = Color.LightYellow;
                string[] lines = txtIcs.Lines;

                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
                {
                    string line = lines[lineIndex];
                    Color? background = line is "BEGIN:VEVENT" or "END:VEVENT"
                        ? eventMarkerColor
                        : line.StartsWith("UID:", StringComparison.Ordinal)
                            ? uidMarkerColor
                            : null;
                    if (background is not { } color)
                    {
                        continue;
                    }

                    int start = txtIcs.GetFirstCharIndexFromLine(lineIndex);
                    if (start < 0)
                    {
                        continue;
                    }

                    txtIcs.Select(start, line.Length);
                    txtIcs.SelectionBackColor = color;
                }

                txtIcs.Select(0, 0);
            }
            finally
            {
                txtIcs.ResumeLayout();
            }
        }

        private void btnCopy_Click(object? sender, EventArgs e)
        {
            UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Clipboard.SetText(txtIcs.Text);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                MessageBox.Show(this, $"The ICS text could not be copied:\n\n{ex.Message}",
                    "Clipboard error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            UseWaitCursor = true;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using var dialog = new SaveFileDialog
                {
                    Title = "Save generated ICS",
                    Filter = "iCalendar files (*.ics)|*.ics|All files (*.*)|*.*",
                    DefaultExt = "ics",
                    AddExtension = true,
                    FileName = "meeting.ics",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                File.WriteAllText(dialog.FileName, txtIcs.Text, new UTF8Encoding(false));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"The ICS file could not be saved:\n\n{ex.Message}",
                    "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void txtIcs_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
