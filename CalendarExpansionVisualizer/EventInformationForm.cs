namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// A single warning line and the colour it should be shown in.
    /// </summary>
    public readonly record struct WarningLine(string Text, System.Drawing.Color Color);

    /// <summary>
    /// Read-only dialog that displays full information about a meeting or series,
    /// including recurrence, organizer/attendees, identifiers and instance counts.
    /// Warnings (e.g. recurrence over a year, no end date) are shown in colour.
    /// The layout lives in EventInformationForm.Designer.cs so it can be edited in
    /// the Windows Forms designer.
    /// </summary>
    public sealed partial class EventInformationForm : Form
    {
        private string _changesSubject = string.Empty;
        private IReadOnlyList<RecurChange> _recurrenceChanges = Array.Empty<RecurChange>();

        public EventInformationForm()
        {
            InitializeComponent();

            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        public EventInformationForm(string title, string body, IReadOnlyList<WarningLine> warnings)
            : this()
        {
            SetContent(title, body, warnings);
        }

        /// <summary>
        /// Populates the dialog. The warnings box is only shown when there is at least
        /// one warning; each warning line is rendered in its own colour.
        /// </summary>
        public void SetContent(string title, string body, IReadOnlyList<WarningLine> warnings)
        {
            lblTitle.Text = title;
            txtBody.Text = body;

            txtWarn.Clear();
            if (warnings is { Count: > 0 })
            {
                foreach (WarningLine w in warnings)
                {
                    txtWarn.SelectionStart = txtWarn.TextLength;
                    txtWarn.SelectionLength = 0;
                    txtWarn.SelectionColor = w.Color;
                    txtWarn.AppendText(w.Text + Environment.NewLine);
                }
                txtWarn.Visible = true;
            }
            else
            {
                txtWarn.Visible = false;
            }
        }

        /// <summary>
        /// Supplies the whole-series exceptions/deletions for this meeting so the
        /// "List Recurrence Changes" button can open the listing dialog. The button is
        /// only enabled when at least one change is available.
        /// </summary>
        internal void SetRecurrenceChanges(string subject, IReadOnlyList<RecurChange>? changes)
        {
            _changesSubject = subject ?? string.Empty;
            _recurrenceChanges = changes ?? Array.Empty<RecurChange>();
            btnListChanges.Enabled = _recurrenceChanges.Count > 0;
        }

        private void btnListChanges_Click(object? sender, EventArgs e)
        {
            using var form = new RecurrenceChangesForm(_changesSubject, _recurrenceChanges);
            form.ShowDialog(this);
        }

        private void pnlButtons_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
