namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Read-only dialog that displays full information about a meeting or series,
    /// including recurrence, organizer/attendees, identifiers and instance counts.
    /// Warnings (e.g. recurrence over a year, no end date) are shown in red.
    /// </summary>
    public sealed class EventInformationForm : Form
    {
        public EventInformationForm(string title, string body, IReadOnlyList<string> warnings)
        {
            Text = "Event Information";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(660, 620);
            MinimumSize = new Size(500, 400);

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font(Font.FontFamily, 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };

            var txtBody = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericMonospace, 9.5F),
                BackColor = Color.White,
                Text = body
            };

            var btnClose = new Button
            {
                Text = "Close",
                Dock = DockStyle.Right,
                Width = 100,
                DialogResult = DialogResult.OK
            };
            var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            pnlButtons.Controls.Add(btnClose);

            Controls.Add(txtBody);

            if (warnings.Count > 0)
            {
                var txtWarn = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Bottom,
                    Height = 90,
                    ForeColor = Color.Red,
                    BackColor = Color.White,
                    Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold),
                    Text = string.Join(Environment.NewLine, warnings)
                };
                Controls.Add(txtWarn);
            }

            Controls.Add(pnlButtons);
            Controls.Add(lblTitle);

            AcceptButton = btnClose;
            CancelButton = btnClose;
        }
    }
}
