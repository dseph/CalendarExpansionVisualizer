namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Read-only dialog that displays full information about a meeting or series,
    /// including recurrence, organizer/attendees, identifiers and instance counts.
    /// Warnings (e.g. recurrence over a year, no end date) are shown in red.
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

        public EventInformationForm(string title, string body, IReadOnlyList<string> warnings)
            : this()
        {
            SetContent(title, body, warnings);
        }

        /// <summary>
        /// Populates the dialog. The warnings box is only shown when there is at least
        /// one warning.
        /// </summary>
        public void SetContent(string title, string body, IReadOnlyList<string> warnings)
        {
            lblTitle.Text = title;
            txtBody.Text = body;

            if (warnings is { Count: > 0 })
            {
                txtWarn.Text = string.Join(Environment.NewLine, warnings);
                txtWarn.Visible = true;
            }
            else
            {
                txtWarn.Text = string.Empty;
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
