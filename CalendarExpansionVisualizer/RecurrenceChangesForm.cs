namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Dialog for listing a recurring series' exceptions and/or deletions for the entire
    /// series (including occurrences outside any search window). The user picks a mode
    /// with the radio buttons and clicks "List" to render the report into the scrolling
    /// text box. Layout lives in RecurrenceChangesForm.Designer.cs.
    /// </summary>
    public sealed partial class RecurrenceChangesForm : Form
    {
        private readonly string _subject;
        private readonly IReadOnlyList<RecurChange> _changes;

        public RecurrenceChangesForm()
        {
            InitializeComponent();
            _subject = string.Empty;
            _changes = Array.Empty<RecurChange>();
        }

        internal RecurrenceChangesForm(string subject, IReadOnlyList<RecurChange> changes)
            : this()
        {
            _subject = subject ?? string.Empty;
            _changes = changes ?? Array.Empty<RecurChange>();

            // Show an initial listing for the default selection.
            RenderListing();
        }

        private void btnList_Click(object? sender, EventArgs e)
        {
            RenderListing();
        }

        private void RenderListing()
        {
            bool wantExceptions = rbExceptions.Checked || rbBoth.Checked;
            bool wantDeletions = rbDeletions.Checked || rbBoth.Checked;
            bool showKindLabel = rbBoth.Checked;

            (string body, _) = RecurrenceChangeReport.Build(
                _subject, _changes, wantExceptions, wantDeletions, showKindLabel);

            txtOutput.Text = body;
            txtOutput.SelectionStart = 0;
            txtOutput.SelectionLength = 0;
        }

        private void txtOutput_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
