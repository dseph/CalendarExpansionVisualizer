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
    }
}
