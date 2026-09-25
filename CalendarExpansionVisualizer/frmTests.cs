using System.Text;

namespace CalendarExpansionVisualizer
{
    public partial class frmTests : Form
    {
        private readonly TestContext _context = null!;

        /// <summary>
        /// Parameterless constructor required by the Windows Forms designer. Not used at
        /// runtime; use <see cref="frmTests(TestContext)"/> to supply the auth context.
        /// </summary>
        public frmTests()
        {
            InitializeComponent();
        }

        internal frmTests(TestContext context) : this()
        {
            _context = context;

            DateTime start = DateTime.Now.Date;
            dtpStart.Value = start;
            dtpEnd.Value = start.AddDays(7);

            string mailbox = _context.Flow == AuthFlow.Application
                ? (string.IsNullOrWhiteSpace(_context.Upn) ? "(no mailbox)" : _context.Upn!)
                : "(signed-in user /me)";
            lblContext.Text =
                $"Flow: {_context.Flow}   Mailbox: {mailbox}   Cloud: {_context.CloudName}   " +
                $"Logging: {(_context.LogEnabled ? "on" : "off")}";

            if (_context.Flow == AuthFlow.Application && !string.IsNullOrWhiteSpace(_context.Upn))
            {
                txtSchedules.Text = _context.Upn;
            }

            foreach (RadioButton rb in new[]
                     { rbGetSchedule, rbCalendarView, rbSubjectInRange, rbSubjectAll, rbInstances, rbDelta })
            {
                rb.CheckedChanged += TestSelectionChanged;
            }

            UpdateInputStates();
        }

        private void TestSelectionChanged(object? sender, EventArgs e) => UpdateInputStates();

        /// <summary>
        /// Enables only the input fields relevant to the currently selected test.
        /// </summary>
        private void UpdateInputStates()
        {
            bool needsRange = rbGetSchedule.Checked || rbCalendarView.Checked ||
                              rbSubjectInRange.Checked || rbInstances.Checked || rbDelta.Checked;
            bool needsSubject = rbSubjectInRange.Checked || rbSubjectAll.Checked;
            bool needsInterval = rbGetSchedule.Checked;
            bool needsEventId = rbInstances.Checked;
            bool needsSchedules = rbGetSchedule.Checked;

            lblStart.Enabled = dtpStart.Enabled = needsRange;
            lblEnd.Enabled = dtpEnd.Enabled = needsRange;
            lblSubject.Enabled = txtSubject.Enabled = needsSubject;
            lblInterval.Enabled = numInterval.Enabled = needsInterval;
            lblEventId.Enabled = txtEventId.Enabled = needsEventId;
            lblSchedules.Enabled = txtSchedules.Enabled = needsSchedules;
        }

        private async void btnRun_Click(object? sender, EventArgs e)
        {
            DateTimeOffset start = new(dtpStart.Value);
            DateTimeOffset end = new(dtpEnd.Value);
            if (end <= start)
            {
                MessageBox.Show(this, "End must be after start.",
                    "Invalid range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool showResults = chkShowResults.Checked;
            var output = new List<(DateTime Time, string Title, string Body)>();

            SetBusy(true);
            txtResults.Clear();
            lblStarted.Text = string.Empty;
            lblEnded.Text = string.Empty;
            lblElapsed.Text = string.Empty;

            DateTime startedAt = DateTime.Now;
            lblStarted.Text = startedAt.ToString("HH:mm:ss.fff");

            try
            {
                using var client = new GraphCalendarClient(
                    _context.AccessToken, _context.LogPath, _context.LogEnabled, _context.GraphRoot);

                AuthFlow flow = _context.Flow;
                string? upn = _context.Upn;

                if (rbGetSchedule.Checked)
                {
                    var schedules = txtSchedules.Text
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();

                    if (schedules.Count == 0)
                    {
                        AppendResult(output, "GetSchedule", "SKIPPED: no schedules entered.", showResults);
                    }
                    else
                    {
                        string res = await client.GetScheduleAsync(
                            flow, upn, schedules, start, end, (int)numInterval.Value, CancellationToken.None);
                        AppendResult(output, "GetSchedule", res, showResults);
                    }
                }
                else if (rbCalendarView.Checked)
                {
                    string res = await client.CalendarViewRawAsync(flow, upn, start, end, CancellationToken.None);
                    AppendResult(output, "CalendarView", res, showResults);
                }
                else if (rbSubjectInRange.Checked)
                {
                    if (string.IsNullOrWhiteSpace(txtSubject.Text))
                    {
                        AppendResult(output, "Search subject in time range", "SKIPPED: no subject entered.", showResults);
                    }
                    else
                    {
                        string res = await client.SearchEventsBySubjectInRangeAsync(
                            flow, upn, txtSubject.Text.Trim(), start, end, CancellationToken.None);
                        AppendResult(output, "Search subject in time range", res, showResults);
                    }
                }
                else if (rbSubjectAll.Checked)
                {
                    if (string.IsNullOrWhiteSpace(txtSubject.Text))
                    {
                        AppendResult(output, "Search all events for subject", "SKIPPED: no subject entered.", showResults);
                    }
                    else
                    {
                        string res = await client.SearchAllEventsBySubjectAsync(
                            flow, upn, txtSubject.Text.Trim(), CancellationToken.None);
                        AppendResult(output, "Search all events for subject", res, showResults);
                    }
                }
                else if (rbInstances.Checked)
                {
                    if (string.IsNullOrWhiteSpace(txtEventId.Text))
                    {
                        AppendResult(output, "Event instances in range", "SKIPPED: no event id entered.", showResults);
                    }
                    else
                    {
                        string res = await client.GetEventInstancesAsync(
                            flow, upn, txtEventId.Text.Trim(), start, end, CancellationToken.None);
                        AppendResult(output, "Event instances in range", res, showResults);
                    }
                }
                else if (rbDelta.Checked)
                {
                    string res = await client.CalendarViewDeltaAsync(flow, upn, start, end, CancellationToken.None);
                    AppendResult(output, "CalendarView delta", res, showResults);
                }
            }
            catch (Exception ex)
            {
                AppendResult(output, "ERROR", ex.ToString(), showResults);
                MessageBox.Show(this, $"A test failed:\n\n{ex.Message}",
                    "Test error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DateTime endedAt = DateTime.Now;
                lblEnded.Text = endedAt.ToString("HH:mm:ss.fff");
                lblElapsed.Text = FormatElapsed(endedAt - startedAt);

                if (showResults)
                {
                    RenderSections(output);
                }
                else
                {
                    txtResults.Clear();
                    txtResults.Text = "(result display disabled — see the log file for full request/response detail.)";
                }

                SetBusy(false);
            }
        }

        private static void AppendResult(
            List<(DateTime Time, string Title, string Body)> output, string title, string body, bool showResults)
        {
            if (!showResults)
            {
                return;
            }

            output.Add((DateTime.Now, title, body));
        }

        /// <summary>
        /// Renders each captured section into the results box, pretty-printing and
        /// syntax-colouring any JSON response body.
        /// </summary>
        private void RenderSections(List<(DateTime Time, string Title, string Body)> sections)
        {
            txtResults.Clear();

            foreach (var (time, title, body) in sections)
            {
                AppendColored(txtResults,
                    "================================================================\n", Color.Gray);
                AppendColored(txtResults, $"[{time:HH:mm:ss.fff}] {title}\n", Color.DarkSlateBlue, bold: true);
                AppendColored(txtResults,
                    "----------------------------------------------------------------\n", Color.Gray);

                RenderBody(body);
                AppendColored(txtResults, "\n\n", Color.Black);
            }

            txtResults.SelectionStart = 0;
            txtResults.SelectionLength = 0;
            txtResults.ScrollToCaret();
        }

        /// <summary>
        /// Splits an HTTP result into its status line and body, then colourises the body
        /// as JSON when possible (pretty-printed), or shows it verbatim otherwise.
        /// </summary>
        private void RenderBody(string body)
        {
            string statusLine = body;
            string remainder = string.Empty;

            int newline = body.IndexOf('\n');
            if (newline >= 0 && body.StartsWith("HTTP ", StringComparison.Ordinal))
            {
                statusLine = body[..newline].TrimEnd('\r');
                remainder = body[(newline + 1)..];
                AppendColored(txtResults, statusLine + "\n", Color.Teal, bold: true);
            }
            else
            {
                // Non-HTTP text (SKIPPED / ERROR messages): show as-is.
                AppendColored(txtResults, body + "\n", Color.DimGray);
                return;
            }

            string trimmed = remainder.TrimStart();
            if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(remainder);
                    string pretty = System.Text.Json.JsonSerializer.Serialize(
                        doc.RootElement,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    AppendJsonColored(txtResults, pretty);
                    return;
                }
                catch (System.Text.Json.JsonException)
                {
                    // Fall through and show the raw body.
                }
            }

            AppendColored(txtResults, remainder + "\n", Color.Black);
        }

        /// <summary>Appends a run of text in the given colour (and optional bold) to a RichTextBox.</summary>
        private static void AppendColored(RichTextBox rtb, string text, Color color, bool bold = false)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = color;
            rtb.SelectionFont = new Font(rtb.Font, bold ? FontStyle.Bold : FontStyle.Regular);
            rtb.AppendText(text);
            rtb.SelectionColor = rtb.ForeColor;
        }

        // JSON token colours (index order matches the RTF colour table below).
        private static readonly Color JsonPunctuationColor = Color.FromArgb(105, 105, 105);
        private static readonly Color JsonKeyColor = Color.FromArgb(0, 0, 180);
        private static readonly Color JsonStringColor = Color.FromArgb(163, 21, 21);
        private static readonly Color JsonNumberColor = Color.FromArgb(0, 128, 0);
        private static readonly Color JsonKeywordColor = Color.FromArgb(0, 0, 255);
        private static readonly Color JsonNullColor = Color.FromArgb(128, 128, 128);
        private static readonly Color JsonDefaultColor = Color.FromArgb(0, 0, 0);

        /// <summary>
        /// Builds a single RTF document for the pretty-printed JSON with basic syntax
        /// colouring (keys, string values, numbers, booleans/null, punctuation) and
        /// inserts it into the results box in one operation. Newlines are emitted as a
        /// single RTF line break so no extra blank lines appear.
        /// </summary>
        private static void AppendJsonColored(RichTextBox rtb, string json)
        {
            json = json.Replace("\r\n", "\n").Replace("\r", "\n");

            var rtf = new StringBuilder();
            rtf.Append(@"{\rtf1\ansi\deff0{\fonttbl{\f0\fmodern Consolas;}}");
            rtf.Append(@"{\colortbl ;");
            AppendColorTableEntry(rtf, JsonPunctuationColor); // \cf1
            AppendColorTableEntry(rtf, JsonKeyColor);         // \cf2
            AppendColorTableEntry(rtf, JsonStringColor);      // \cf3
            AppendColorTableEntry(rtf, JsonNumberColor);      // \cf4
            AppendColorTableEntry(rtf, JsonKeywordColor);     // \cf5
            AppendColorTableEntry(rtf, JsonNullColor);        // \cf6
            AppendColorTableEntry(rtf, JsonDefaultColor);     // \cf7
            rtf.Append('}');
            rtf.Append(@"\f0\fs18 ");

            int i = 0;
            int n = json.Length;

            void Emit(int colorIndex, string text)
            {
                rtf.Append(@"\cf").Append(colorIndex).Append(' ');
                AppendRtfEscaped(rtf, text);
            }

            while (i < n)
            {
                char c = json[i];

                if (c == '"')
                {
                    int start = i++;
                    while (i < n)
                    {
                        if (json[i] == '\\') { i += 2; continue; }
                        if (json[i] == '"') { i++; break; }
                        i++;
                    }
                    string token = json[start..Math.Min(i, n)];

                    int j = i;
                    while (j < n && char.IsWhiteSpace(json[j])) j++;
                    bool isKey = j < n && json[j] == ':';

                    Emit(isKey ? 2 : 3, token);
                }
                else if (c == '-' || char.IsDigit(c))
                {
                    int start = i;
                    while (i < n && (char.IsDigit(json[i]) || json[i] is '-' or '+' or '.' or 'e' or 'E'))
                    {
                        i++;
                    }
                    Emit(4, json[start..i]);
                }
                else if (char.IsLetter(c))
                {
                    int start = i;
                    while (i < n && char.IsLetter(json[i])) i++;
                    string word = json[start..i];
                    int colorIndex = word is "true" or "false" ? 5 : word == "null" ? 6 : 7;
                    Emit(colorIndex, word);
                }
                else
                {
                    // Punctuation and whitespace (including newlines).
                    int start = i;
                    while (i < n && !(json[i] == '"' || json[i] == '-' || char.IsLetterOrDigit(json[i])))
                    {
                        i++;
                    }
                    Emit(1, json[start..i]);
                }
            }

            rtf.Append('}');

            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectedRtf = rtf.ToString();
        }

        private static void AppendColorTableEntry(StringBuilder rtf, Color color)
            => rtf.Append(@"\red").Append(color.R)
                  .Append(@"\green").Append(color.G)
                  .Append(@"\blue").Append(color.B)
                  .Append(';');

        /// <summary>Escapes text for embedding in an RTF body.</summary>
        private static void AppendRtfEscaped(StringBuilder rtf, string text)
        {
            foreach (char c in text)
            {
                switch (c)
                {
                    case '\\': rtf.Append(@"\\"); break;
                    case '{': rtf.Append(@"\{"); break;
                    case '}': rtf.Append(@"\}"); break;
                    case '\n': rtf.Append(@"\line "); break;
                    case '\t': rtf.Append(@"\tab "); break;
                    default:
                        if (c > 127)
                        {
                            rtf.Append(@"\u").Append((int)c).Append('?');
                        }
                        else
                        {
                            rtf.Append(c);
                        }
                        break;
                }
            }
        }

        /// <summary>Compact minutes/seconds/milliseconds formatting.</summary>
        private static string FormatElapsed(TimeSpan elapsed)
        {
            int minutes = (int)elapsed.TotalMinutes;
            int seconds = elapsed.Seconds;
            int millis = elapsed.Milliseconds;

            if (minutes > 0)
            {
                return $"{minutes}m {seconds:00}s {millis:000}ms";
            }
            if (seconds > 0)
            {
                return $"{seconds}s {millis:000}ms";
            }
            return $"{millis}ms";
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            grpInputs.Enabled = !busy;
            grpTests.Enabled = !busy;
            chkShowResults.Enabled = !busy;
            btnRun.Enabled = !busy;

            if (!busy)
            {
                UpdateInputStates();
            }
        }
    }
}
