using Microsoft.Identity.Client;

namespace CalendarExpansionVisualizer
{
    public partial class frmMain : Form
    {
        private string? _accessToken;
        private AuthFlow _flow = AuthFlow.Delegated;
        private List<MeetingRow>? _lastCheckRows;
        private bool _checkMode;
        private bool _useMicrosoftAppId = true;

        // The date window used by the most recent Check, reused when computing in-range
        // attachment metrics for the Event Information form.
        private DateTimeOffset _lastWindowStart;
        private DateTimeOffset _lastWindowEnd;

        // Well-known Microsoft first-party public client (Microsoft Graph PowerShell).
        // Lets a user sign in interactively without registering their own application.
        private const string MicrosoftGraphPowerShellClientId = "14d82eec-204b-4c2f-b7e8-296a70dab67e";

        private sealed class MeetingRow
        {
            public string Subject { get; set; } = string.Empty;
            public DateTimeOffset SortKey { get; set; }
            public Dictionary<DateOnly, CellState> Cells { get; } = new();

            public string EventId { get; set; } = string.Empty;
            public bool IsRecurring { get; set; }
            public int InstancesInRange { get; set; }
            public int ExceptionsInRange { get; set; }
            public int DeletionsInRange { get; set; }

            // Colour for the Subject cell, based on how long the recurrence runs.
            public Color SubjectColor { get; set; } = Color.Black;

            // Whole-series totals (across the entire recurrence, not just the window).
            // Null until computed on demand when the Event Information form is opened.
            public int? TotalInstances { get; set; }
            public int? TotalExceptions { get; set; }
            public int? TotalDeletions { get; set; }
            public string? TotalScopeNote { get; set; }
            public bool TotalsComputed { get; set; }

            // Cached one-line status describing the whole event's span and change counts,
            // shown in the status bar when the row is clicked. Null until first computed.
            public string? SeriesStatusText { get; set; }

            // Attachment metrics for the instances within the selected date range.
            public int? RangeAttachmentCount { get; set; }
            public long? RangeAttachmentTotalSize { get; set; }
            public bool RangeAttachmentsComputed { get; set; }
        }

        public frmMain()
        {
            InitializeComponent();

            DateTime start = DateTime.Now.Date;
            dtpStart.Value = start;
            dtpEnd.Value = start.AddDays(7);

            txtLogPath.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CalendarExpansionVisualizer.log");

            cboCloud.SelectedIndex = 0; // Commercial

            UpdateAuthFieldStates();

            dgvEvents.CellDoubleClick += dgvEvents_CellDoubleClick;
            dgvEvents.CellClick += dgvEvents_CellClick;
        }

        private void AuthFlowChanged(object? sender, EventArgs e)
        {
            _useMicrosoftAppId = rbMicrosoftApp.Checked;
            _flow = rbApplication.Checked ? AuthFlow.Application : AuthFlow.Delegated;
            UpdateAuthFieldStates();
        }

        // Maps the cloud combo box selection to the sovereign cloud / Graph endpoint.
        private SovereignCloud CurrentCloud() => cboCloud.SelectedIndex switch
        {
            1 => SovereignCloud.Gcc,
            2 => SovereignCloud.GccHigh,
            3 => SovereignCloud.Dod,
            _ => SovereignCloud.Commercial
        };

        private string CurrentGraphRoot() => CloudEndpoints.GraphRoot(CurrentCloud());

        private void UpdateAuthFieldStates()
        {
            bool isApp = _flow == AuthFlow.Application;
            txtClientSecret.Enabled = isApp;
            txtUpn.Enabled = isApp;
            txtRedirectUri.Enabled = !isApp;

            if (_useMicrosoftAppId)
            {
                txtClientId.Text = MicrosoftGraphPowerShellClientId;
                txtClientId.Enabled = false;
            }
            else
            {
                if (txtClientId.Text == MicrosoftGraphPowerShellClientId)
                {
                    txtClientId.Text = string.Empty;
                }
                txtClientId.Enabled = true;
            }
        }

        private void StartDateChanged(object? sender, EventArgs e)
        {
            // Keep the end date within one month of the (new) start date.
            DateTime maxEnd = dtpStart.Value.AddMonths(1);
            if (dtpEnd.Value > maxEnd)
            {
                dtpEnd.Value = maxEnd;
            }
            if (dtpEnd.Value < dtpStart.Value)
            {
                dtpEnd.Value = dtpStart.Value;
            }
        }

        private async void btnGetToken_Click(object? sender, EventArgs e)
        {
            string clientId = _useMicrosoftAppId
                ? MicrosoftGraphPowerShellClientId
                : txtClientId.Text.Trim();

            // The Microsoft app-id option can sign in against the "common" authority.
            string tenantId = string.IsNullOrWhiteSpace(txtTenantId.Text)
                ? (_useMicrosoftAppId ? "common" : string.Empty)
                : txtTenantId.Text.Trim();

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId))
            {
                MessageBox.Show(this, "Tenant ID and Client ID are required.",
                    "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_flow == AuthFlow.Application &&
                string.IsNullOrWhiteSpace(txtClientSecret.Text))
            {
                MessageBox.Show(this, "Client Secret is required for the application flow.",
                    "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_flow == AuthFlow.Application &&
                string.IsNullOrWhiteSpace(txtUpn.Text))
            {
                MessageBox.Show(this, "Mailbox UPN is required for the application flow.",
                    "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true, "Acquiring token...");
            try
            {
                AuthenticationResult result = _flow == AuthFlow.Application
                    ? await AuthService.AcquireApplicationAsync(
                        tenantId, clientId,
                        txtClientSecret.Text, CurrentCloud(), CancellationToken.None)
                    : await AuthService.AcquireDelegatedAsync(
                        tenantId, clientId,
                        txtRedirectUri.Text.Trim(), CurrentCloud(), CancellationToken.None);

                _accessToken = result.AccessToken;
                lblTokenStatus.ForeColor = Color.Green;
                lblTokenStatus.Text = $"Token acquired (expires {result.ExpiresOn.LocalDateTime:HH:mm})";
                btnFetch.Enabled = true;
                btnCheck.Enabled = true;
                btnList.Enabled = true;
                btnTests.Enabled = true;
                lblStatus.Text = "Token acquired. Ready to fetch calendar.";
            }
            catch (MsalException ex)
            {
                _accessToken = null;
                btnFetch.Enabled = false;
                btnCheck.Enabled = false;
                btnList.Enabled = false;
                btnTests.Enabled = false;
                lblTokenStatus.ForeColor = Color.Firebrick;
                lblTokenStatus.Text = "Token failed";
                MessageBox.Show(this, $"Authentication failed:\n\n{ex.Message}",
                    "Auth error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _accessToken = null;
                btnFetch.Enabled = false;
                btnCheck.Enabled = false;
                btnList.Enabled = false;
                btnTests.Enabled = false;
                lblTokenStatus.ForeColor = Color.Firebrick;
                lblTokenStatus.Text = "Token failed";
                MessageBox.Show(this, $"Unexpected error:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, null);
            }
        }

        private async void btnFetch_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                MessageBox.Show(this, "Acquire a token first.",
                    "No token", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date.AddDays(1).AddTicks(-1);

            if (end <= start)
            {
                MessageBox.Show(this, "End date must be on or after the start date.",
                    "Invalid range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpEnd.Value.Date > dtpStart.Value.Date.AddMonths(1))
            {
                MessageBox.Show(this, "The date range cannot exceed one month.",
                    "Invalid range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLogPath.Text))
            {
                MessageBox.Show(this, "Please specify a log file path.",
                    "Missing log path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true, "Fetching calendar view...");
            try
            {
                using var client = new GraphCalendarClient(
                    _accessToken, txtLogPath.Text.Trim(), chkEnableLog.Checked, CurrentGraphRoot());
                List<CalendarInstance> events = await client.GetCalendarViewAsync(
                    _flow,
                    _flow == AuthFlow.Application ? txtUpn.Text.Trim() : null,
                    new DateTimeOffset(start),
                    new DateTimeOffset(end),
                    CancellationToken.None);

                BindResults(events);
                lblStatus.Text = $"Retrieved {events.Count} expanded event instance(s). " +
                                 (chkEnableLog.Checked ? $"Logged to {txtLogPath.Text.Trim()}" : "Logging disabled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to fetch calendar:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Fetch failed.";
            }
            finally
            {
                SetBusy(false, null);
            }
        }

        private void BindResults(List<CalendarInstance> events)
        {
            var rows = events
                .Select(ev => new
                {
                    ev.Subject,
                    Start = ev.Start?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                    End = ev.End?.LocalDateTime.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                    ev.Organizer,
                    ev.Location,
                    AllDay = ev.IsAllDay,
                    Recurring = !string.IsNullOrEmpty(ev.SeriesMasterId),
                    ev.Type
                })
                .ToList();

            dgvEvents.DataSource = null;
            dgvEvents.Rows.Clear();
            dgvEvents.Columns.Clear();
            dgvEvents.AutoGenerateColumns = true;
            dgvEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEvents.RowHeadersVisible = true;
            _checkMode = false;
            _lastCheckRows = null;
            dgvEvents.DataSource = rows;
        }

        private async void btnCheck_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                MessageBox.Show(this, "Acquire a token first.",
                    "No token", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date.AddDays(1).AddTicks(-1);

            if (end <= start)
            {
                MessageBox.Show(this, "End date must be on or after the start date.",
                    "Invalid range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dtpEnd.Value.Date > dtpStart.Value.Date.AddMonths(1))
            {
                MessageBox.Show(this, "The date range cannot exceed one month.",
                    "Invalid range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLogPath.Text))
            {
                MessageBox.Show(this, "Please specify a log file path.",
                    "Missing log path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_flow == AuthFlow.Application && string.IsNullOrWhiteSpace(txtUpn.Text))
            {
                MessageBox.Show(this, "Mailbox UPN is required for the application flow.",
                    "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetBusy(true, "Checking calendar...");
            lblElapsed.Text = "";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var client = new GraphCalendarClient(
                    _accessToken, txtLogPath.Text.Trim(), chkEnableLog.Checked, CurrentGraphRoot());
                string? upn = _flow == AuthFlow.Application ? txtUpn.Text.Trim() : null;

                List<CalendarInstance> instances = await client.GetCalendarViewAsync(
                    _flow, upn, new DateTimeOffset(start), new DateTimeOffset(end),
                    CancellationToken.None);

                _lastWindowStart = new DateTimeOffset(start);
                _lastWindowEnd = new DateTimeOffset(end);

                DateOnly winStart = DateOnly.FromDateTime(start.Date);
                DateOnly winEnd = DateOnly.FromDateTime(end.Date);

                List<MeetingRow> rows = await BuildMeetingRowsAsync(client, upn, instances, winStart, winEnd);
                RenderCheckGrid(rows, winStart, winEnd);

                lblStatus.Text = $"Checked {rows.Count} meeting(s)/series across " +
                                 $"{winStart:M/d}\u2013{winEnd:M/d}. " +
                                 (chkEnableLog.Checked ? $"Logged to {txtLogPath.Text.Trim()}" : "Logging disabled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to check calendar:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Check failed.";
            }
            finally
            {
                stopwatch.Stop();
                lblElapsed.Text = $"Took {FormatElapsed(stopwatch.Elapsed)}";
                SetBusy(false, null);
            }
        }

        /// <summary>
        /// Formats a duration compactly as minutes, seconds and milliseconds
        /// (e.g. "1m 05s 320ms", "12s 004ms", "742ms").
        /// </summary>
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

        private static async Task<List<MeetingRow>> BuildMeetingRowsAsync(
            GraphCalendarClient client, string? upn, List<CalendarInstance> instances,
            DateOnly winStart, DateOnly winEnd)
        {
            var rows = new List<MeetingRow>();

            // All series masters in the mailbox, so we can also show series that are active
            // over the window but have no instance inside it (they never appear in calendarView).
            // Best-effort: if this listing fails it must not break the core processing below.
            List<SeriesMaster> allMasters;
            try
            {
                allMasters = await client.GetSeriesMastersAsync(
                    AuthFlowOf(upn), upn, CancellationToken.None);
            }
            catch
            {
                allMasters = new List<SeriesMaster>();
            }

            var masterCache = allMasters
                .GroupBy(m => m.Id)
                .ToDictionary(g => g.Key, g => (SeriesMaster?)g.First());

            // Single (non-recurring) events: one row each.
            foreach (CalendarInstance ev in instances.Where(i => string.IsNullOrEmpty(i.SeriesMasterId)))
            {
                var row = new MeetingRow
                {
                    Subject = string.IsNullOrEmpty(ev.Subject) ? "(no subject)" : ev.Subject,
                    SortKey = ev.Start ?? DateTimeOffset.MaxValue,
                    EventId = ev.Id,
                    IsRecurring = false,
                    InstancesInRange = 1,
                    SubjectColor = OversizedMaster(ev.MessageSize) ? Color.Red : Color.Black
                };

                DateOnly from = ev.Start.HasValue ? DateOnly.FromDateTime(ev.Start.Value.LocalDateTime) : winStart;
                DateOnly to = ev.End.HasValue ? DateOnly.FromDateTime(ev.End.Value.LocalDateTime) : from;
                // Graph end is exclusive at midnight for all-day events; keep single-day sane.
                if (to > from && ev.IsAllDay)
                {
                    to = to.AddDays(-1);
                }

                for (DateOnly d = from; d <= to; d = d.AddDays(1))
                {
                    if (d >= winStart && d <= winEnd)
                    {
                        row.Cells[d] = CellState.Normal;
                    }
                }

                rows.Add(row);
            }

            // Recurring series: group instances by their series master.
            foreach (var group in instances
                         .Where(i => !string.IsNullOrEmpty(i.SeriesMasterId))
                         .GroupBy(i => i.SeriesMasterId))
            {
                string masterId = group.Key;
                if (!masterCache.TryGetValue(masterId, out SeriesMaster? master))
                {
                    master = await client.GetEventAsync(
                        AuthFlowOf(upn), upn, masterId, CancellationToken.None);
                    masterCache[masterId] = master;
                }

                var actual = new Dictionary<DateOnly, CellState>();
                foreach (CalendarInstance inst in group)
                {
                    if (!inst.Start.HasValue)
                    {
                        continue;
                    }
                    DateOnly d = DateOnly.FromDateTime(inst.Start.Value.LocalDateTime);
                    CellState state = string.Equals(inst.Type, "exception", StringComparison.OrdinalIgnoreCase)
                        ? CellState.Modified
                        : CellState.Normal;

                    // An exception always wins over a plain occurrence for the same day.
                    if (state == CellState.Modified || !actual.ContainsKey(d))
                    {
                        actual[d] = state;
                    }
                }

                string subject = master is not null && !string.IsNullOrEmpty(master.Subject)
                    ? master.Subject
                    : group.First().Subject;
                if (string.IsNullOrEmpty(subject))
                {
                    subject = "(no subject)";
                }

                string recurType = master?.Recurrence?.PatternType
                                   ?? group.First().Type;
                if (string.IsNullOrEmpty(recurType))
                {
                    recurType = "recurring";
                }

                if (master?.Recurrence is not null && master.ExceptionCount is null &&
                    !string.IsNullOrEmpty(masterId))
                {
                    master.ExceptionCount = await client.GetSeriesExceptionCountAsync(
                        AuthFlowOf(upn), upn, masterId, CancellationToken.None);
                }

                var row = new MeetingRow
                {
                    Subject = $"{subject} ({recurType})",
                    EventId = masterId,
                    IsRecurring = true,
                    SubjectColor = OversizedMaster(master?.MessageSize) || ManyExceptions(master?.ExceptionCount)
                        ? Color.Red
                        : SubjectColorFor(master?.Recurrence)
                };

                // Expected pattern dates -> detect deletions.
                var expected = master?.Recurrence is not null
                    ? RecurrenceExpander.Expand(master.Recurrence, winStart, winEnd)
                    : new List<DateOnly>();

                var expectedSet = new HashSet<DateOnly>(expected);

                foreach (DateOnly d in expectedSet)
                {
                    row.Cells[d] = actual.TryGetValue(d, out CellState st)
                        ? st
                        : CellState.Deleted;
                }

                // Instances that exist but aren't in the expected set (e.g. moved exceptions).
                foreach (var kvp in actual)
                {
                    if (!expectedSet.Contains(kvp.Key))
                    {
                        row.Cells[kvp.Key] = kvp.Value;
                    }
                }

                row.InstancesInRange = actual.Count;
                row.ExceptionsInRange = actual.Values.Count(v => v == CellState.Modified);
                row.DeletionsInRange = expectedSet.Count(d => !actual.ContainsKey(d));

                // Sort by the true series start (may be years before the window).
                row.SortKey = master?.Recurrence is not null
                    ? new DateTimeOffset(master.Recurrence.RangeStart.ToDateTime(TimeOnly.MinValue))
                    : group.Min(i => i.Start ?? DateTimeOffset.MaxValue);

                rows.Add(row);
            }

            // Series that are active over the window but have no instance inside it
            // (e.g. a yearly March meeting when viewing June). These never come back from
            // calendarView, so add them explicitly from the series-master list.
            var seenMasterIds = new HashSet<string>(
                instances.Where(i => !string.IsNullOrEmpty(i.SeriesMasterId))
                         .Select(i => i.SeriesMasterId));

            foreach (SeriesMaster master in allMasters)
            {
                if (master.Recurrence is null ||
                    string.IsNullOrEmpty(master.Id) ||
                    seenMasterIds.Contains(master.Id) ||
                    !RecurrenceOverlaps(master.Recurrence, winStart, winEnd))
                {
                    continue;
                }

                string subject = string.IsNullOrEmpty(master.Subject) ? "(no subject)" : master.Subject;
                string recurType = string.IsNullOrEmpty(master.Recurrence.PatternType)
                    ? "recurring"
                    : master.Recurrence.PatternType;

                if (master.ExceptionCount is null)
                {
                    master.ExceptionCount = await client.GetSeriesExceptionCountAsync(
                        AuthFlowOf(upn), upn, master.Id, CancellationToken.None);
                }

                var row = new MeetingRow
                {
                    Subject = $"{subject} ({recurType})",
                    EventId = master.Id,
                    IsRecurring = true,
                    SubjectColor = OversizedMaster(master.MessageSize) || ManyExceptions(master.ExceptionCount)
                        ? Color.Red
                        : SubjectColorFor(master.Recurrence),
                    SortKey = new DateTimeOffset(master.Recurrence.RangeStart.ToDateTime(TimeOnly.MinValue))
                };

                // No actual instances -> any pattern date in the window is a deletion; if the
                // pattern yields no date in the window the row stays entirely grey.
                var expected = RecurrenceExpander.Expand(master.Recurrence, winStart, winEnd);
                foreach (DateOnly d in expected)
                {
                    row.Cells[d] = CellState.Deleted;
                }
                row.InstancesInRange = 0;
                row.ExceptionsInRange = 0;
                row.DeletionsInRange = expected.Count;

                rows.Add(row);
            }

            return rows
                .OrderBy(r => r.SortKey)
                .ThenBy(r => r.Subject, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// True when the calendar master record is 50 MB or larger.
        /// </summary>
        private static bool OversizedMaster(long? messageSize)
            => messageSize is { } size && size >= 50L * 1024L * 1024L;

        /// <summary>
        /// True when the entire series has more than 30 exceptions.
        /// </summary>
        private static bool ManyExceptions(int? exceptionCount)
            => exceptionCount is { } count && count > 30;

        /// <summary>
        /// Colour for a recurring meeting's Subject based on how long the series runs:
        /// dark orange when it spans between one and two years, red when it spans over two
        /// years or has no end date, otherwise black.
        /// </summary>
        private static Color SubjectColorFor(RecurrenceInfo? r)
        {
            if (r is null)
            {
                return Color.Black;
            }

            // No end date -> red.
            if (r.RangeType == "noEnd")
            {
                return Color.Red;
            }

            (DateOnly? end, _) = RecurrenceExpander.Summarize(r);
            if (end is not { } effectiveEnd)
            {
                // No computable end date behaves like an open-ended series.
                return Color.Red;
            }

            int days = effectiveEnd.DayNumber - r.RangeStart.DayNumber;
            if (days > 730)
            {
                return Color.Red;          // over two years
            }
            if (days >= 365)
            {
                return Color.DarkOrange;   // between one and two years
            }
            return Color.Black;
        }

        private static bool RecurrenceOverlaps(RecurrenceInfo r, DateOnly winStart, DateOnly winEnd)
        {
            if (r.RangeStart > winEnd)
            {
                return false;
            }

            (DateOnly? end, _) = RecurrenceExpander.Summarize(r);
            if (end is { } effectiveEnd && effectiveEnd < winStart)
            {
                return false;
            }

            return true;
        }

        private static AuthFlow AuthFlowOf(string? upn)
            => string.IsNullOrEmpty(upn) ? AuthFlow.Delegated : AuthFlow.Application;

        private void RenderCheckGrid(List<MeetingRow> rows, DateOnly winStart, DateOnly winEnd)
        {
            _lastCheckRows = rows;
            _checkMode = true;

            dgvEvents.DataSource = null;
            dgvEvents.Rows.Clear();
            dgvEvents.Columns.Clear();
            dgvEvents.AutoGenerateColumns = false;
            dgvEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvEvents.RowHeadersVisible = false;

            var subjectCol = new DataGridViewTextBoxColumn
            {
                HeaderText = "Subject",
                Name = "colSubject",
                Width = 280,
                ReadOnly = true,
                Frozen = true
            };
            dgvEvents.Columns.Add(subjectCol);

            var dayColumns = new List<DateOnly>();
            for (DateOnly d = winStart; d <= winEnd; d = d.AddDays(1))
            {
                dayColumns.Add(d);
                var col = new DataGridViewTextBoxColumn
                {
                    HeaderText = d.ToString("M/d"),
                    Name = "col_" + d.ToString("yyyyMMdd"),
                    Width = 46,
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                };
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.DefaultCellStyle.BackColor = Color.Gainsboro;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvEvents.Columns.Add(col);
            }

            foreach (MeetingRow meeting in rows)
            {
                int rowIndex = dgvEvents.Rows.Add();
                DataGridViewRow gridRow = dgvEvents.Rows[rowIndex];
                gridRow.Cells[0].Value = meeting.Subject;
                gridRow.Cells[0].Style.ForeColor = meeting.SubjectColor;

                for (int i = 0; i < dayColumns.Count; i++)
                {
                    DataGridViewCell cell = gridRow.Cells[i + 1];
                    CellState state = meeting.Cells.TryGetValue(dayColumns[i], out CellState s)
                        ? s
                        : CellState.None;

                    switch (state)
                    {
                        case CellState.Normal:
                            cell.Value = "X";
                            cell.Style.BackColor = Color.LimeGreen;
                            cell.Style.ForeColor = Color.Black;
                            cell.Style.Font = new Font(dgvEvents.Font.FontFamily, 14F, FontStyle.Bold);
                            break;
                        case CellState.Modified:
                            cell.Value = "M";
                            cell.Style.BackColor = Color.Gold;
                            cell.Style.ForeColor = Color.Black;
                            break;
                        case CellState.Deleted:
                            cell.Value = "D";
                            cell.Style.BackColor = Color.Firebrick;
                            cell.Style.ForeColor = Color.White;
                            break;
                        default:
                            cell.Value = string.Empty;
                            cell.Style.BackColor = Color.Gainsboro;
                            break;
                    }
                }
            }
        }


        private void btnTests_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                MessageBox.Show(this, "Acquire a token first.",
                    "No token", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_flow == AuthFlow.Application && string.IsNullOrWhiteSpace(txtUpn.Text))
            {
                MessageBox.Show(this, "Mailbox UPN is required for the application flow.",
                    "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var context = new TestContext
            {
                AccessToken = _accessToken,
                Flow = _flow,
                GraphRoot = CurrentGraphRoot(),
                Upn = _flow == AuthFlow.Application ? txtUpn.Text.Trim() : null,
                LogPath = txtLogPath.Text.Trim(),
                LogEnabled = chkEnableLog.Checked,
                CloudName = CurrentCloud().ToString()
            };

            using var tests = new frmTests(context);
            tests.ShowDialog(this);
        }

        private async void btnList_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                MessageBox.Show(this, "Acquire a token first.",
                    "No token", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_checkMode || _lastCheckRows is null || _lastCheckRows.Count == 0)
            {
                MessageBox.Show(this, "Run \"Check\" first, then select a recurring series row to list.",
                    "List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int rowIndex = dgvEvents.CurrentCell?.RowIndex ?? -1;
            if (rowIndex < 0 || rowIndex >= _lastCheckRows.Count)
            {
                MessageBox.Show(this, "Select a recurring series row in the grid first.",
                    "List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MeetingRow row = _lastCheckRows[rowIndex];
            if (!row.IsRecurring || string.IsNullOrEmpty(row.EventId))
            {
                MessageBox.Show(this, "The selected row is not a recurring series.",
                    "List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool wantExceptions = rbListExceptions.Checked || rbListBoth.Checked;
            bool wantDeletions = rbListDeletions.Checked || rbListBoth.Checked;
            bool showKindLabel = rbListBoth.Checked;

            SetBusy(true, "Listing series changes...");
            try
            {
                string? upn = _flow == AuthFlow.Application ? txtUpn.Text.Trim() : null;
                using var client = new GraphCalendarClient(
                    _accessToken, txtLogPath.Text.Trim(), chkEnableLog.Checked, CurrentGraphRoot());
                EventDetails? details = await client.GetEventDetailsAsync(
                    _flow, upn, row.EventId, CancellationToken.None);

                if (details is null)
                {
                    MessageBox.Show(this, "Could not load event details.",
                        "List", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                (string body, string title) = RecurrenceChangeReport.Build(
                    details.Subject, details.SeriesChanges, wantExceptions, wantDeletions, showKindLabel);

                using var form = new EventInformationForm(title, body, Array.Empty<WarningLine>());
                form.ShowDialog(this);

                int shown = details.SeriesChanges.Count(c =>
                    (c.Kind == RecurChangeKind.Exception && wantExceptions) ||
                    (c.Kind == RecurChangeKind.Deletion && wantDeletions));
                lblStatus.Text = $"Listed {shown} change(s) for the entire series.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to list series changes:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, null);
            }
        }

        private async void dgvEvents_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (!_checkMode || _lastCheckRows is null || e.RowIndex < 0 ||
                e.RowIndex >= _lastCheckRows.Count)
            {
                return;
            }

            if (string.IsNullOrEmpty(_accessToken))
            {
                return;
            }

            MeetingRow row = _lastCheckRows[e.RowIndex];
            if (string.IsNullOrEmpty(row.EventId))
            {
                MessageBox.Show(this, "No event id is available for this row.",
                    "Event Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true, "Loading event information...");
            try
            {
                string? upn = _flow == AuthFlow.Application ? txtUpn.Text.Trim() : null;
                using var client = new GraphCalendarClient(
                    _accessToken, txtLogPath.Text.Trim(), chkEnableLog.Checked, CurrentGraphRoot());
                EventDetails? details = await client.GetEventDetailsAsync(
                    _flow, upn, row.EventId, CancellationToken.None);

                if (details is null)
                {
                    MessageBox.Show(this, "Could not load event details.",
                        "Event Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // The grid only knows counts within the selected window. For the Event
                // Information form, also show whole-series totals read directly from the
                // master's stored recurrence blob (no per-instance searching).
                if (details.Recurrence is not null && !row.TotalsComputed && details.HasSeriesCounts)
                {
                    row.TotalInstances = details.TotalInstances;
                    row.TotalExceptions = details.TotalExceptions;
                    row.TotalDeletions = details.TotalDeletions;
                    row.TotalScopeNote = details.TotalScopeNote;
                    row.TotalsComputed = true;
                }

                if (!row.RangeAttachmentsComputed)
                {
                    (int rc, long rt, bool known) = await client.GetRangeAttachmentSummaryAsync(
                        _flow, upn, row.EventId, row.IsRecurring,
                        _lastWindowStart, _lastWindowEnd, CancellationToken.None);
                    if (known)
                    {
                        row.RangeAttachmentCount = rc;
                        row.RangeAttachmentTotalSize = rt;
                    }
                    row.RangeAttachmentsComputed = true;
                }

                (string body, List<WarningLine> warnings) = BuildEventInformation(details, row);
                using var form = new EventInformationForm(details.Subject, body, warnings);
                form.SetRecurrenceChanges(details.Subject, details.SeriesChanges);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to load event information:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, null);
            }
        }

        private async void dgvEvents_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (!_checkMode || _lastCheckRows is null || e.RowIndex < 0 ||
                e.RowIndex >= _lastCheckRows.Count)
            {
                return;
            }

            MeetingRow row = _lastCheckRows[e.RowIndex];

            // Show a cached summary immediately if we already computed it.
            if (row.SeriesStatusText is not null)
            {
                lblStatus.Text = row.SeriesStatusText;
                return;
            }

            if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(row.EventId))
            {
                return;
            }

            lblStatus.Text = "Loading event span...";
            try
            {
                string? upn = _flow == AuthFlow.Application ? txtUpn.Text.Trim() : null;
                using var client = new GraphCalendarClient(
                    _accessToken, txtLogPath.Text.Trim(), chkEnableLog.Checked, CurrentGraphRoot());
                EventDetails? details = await client.GetEventDetailsAsync(
                    _flow, upn, row.EventId, CancellationToken.None);

                if (details is null)
                {
                    lblStatus.Text = "Could not load event span.";
                    return;
                }

                if (details.Recurrence is not null && !row.TotalsComputed && details.HasSeriesCounts)
                {
                    row.TotalInstances = details.TotalInstances;
                    row.TotalExceptions = details.TotalExceptions;
                    row.TotalDeletions = details.TotalDeletions;
                    row.TotalScopeNote = details.TotalScopeNote;
                    row.TotalsComputed = true;
                }

                row.SeriesStatusText = BuildSeriesStatus(details);
                lblStatus.Text = row.SeriesStatusText;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Failed to load event span: {ex.Message}";
            }
        }

        /// <summary>
        /// Builds the status-bar summary describing the whole event's span and the number
        /// of exceptions and deletions across the entire series (independent of the
        /// selected date range).
        /// </summary>
        private static string BuildSeriesStatus(EventDetails details)
        {
            string span;
            if (details.Recurrence is { } r)
            {
                (DateOnly? end, _) = RecurrenceExpander.Summarize(r);
                string endText = r.RangeType == "noEnd" || end is null
                    ? "no end date"
                    : end.Value.ToString("yyyy-MM-dd");
                span = $"{r.RangeStart:yyyy-MM-dd} to {endText}";
            }
            else
            {
                string start = Or(details.Start, "(unknown)");
                string end = Or(details.End, "(unknown)");
                span = $"{start} to {end}";
            }

            if (details.HasSeriesCounts)
            {
                string instances = details.TotalInstances.HasValue
                    ? details.TotalInstances.Value.ToString()
                    : "unlimited";
                int exceptions = details.TotalExceptions ?? 0;
                int deletions = details.TotalDeletions ?? 0;
                return $"Entire event span: {span}   |   Instances: {instances}   |   " +
                       $"Exceptions: {exceptions}   |   Deletions: {deletions}";
            }

            return $"Entire event span: {span}   |   Instances/Exceptions/Deletions: (not available)";
        }

        private static (string Body, List<WarningLine> Warnings) BuildEventInformation(
            EventDetails details, MeetingRow row)
        {
            var warnings = new List<WarningLine>();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("SUBJECT");
            sb.AppendLine($"  {details.Subject}");
            sb.AppendLine();
            sb.AppendLine("TIME");
            sb.AppendLine($"  Start : {Or(details.Start, "(unknown)")}");
            sb.AppendLine($"  End   : {Or(details.End, "(unknown)")}");
            sb.AppendLine();

            sb.AppendLine("IDENTIFIERS");
            sb.AppendLine($"  Event Id (iCalId)   : {Or(details.Id, "(none)")}");
            sb.AppendLine($"  iCalUId             : {Or(details.ICalUId, "(none)")}");
            sb.AppendLine($"  CleanGlobalObjectId : {Or(details.CleanGlobalObjectId, "(none)")}");
            sb.AppendLine();

            sb.AppendLine("ORGANIZER");
            sb.AppendLine($"  {Or(details.Organizer, "(none)")}");
            sb.AppendLine();

            sb.AppendLine($"ATTENDEES ({details.Attendees.Count})");
            if (details.Attendees.Count == 0)
            {
                sb.AppendLine("  (none)");
            }
            else
            {
                foreach (string a in details.Attendees)
                {
                    sb.AppendLine($"  {a}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("STORAGE");
            sb.AppendLine($"  Calendar record size : {FormatSize(details.MessageSize)}");
            sb.AppendLine($"  Attachments          : {(details.AttachmentCount?.ToString() ?? "(unknown)")}");
            sb.AppendLine($"  Attachments size     : {FormatSize(details.AttachmentsTotalSize)}");
            sb.AppendLine();

            // Calendar-master size warnings (tiered, coloured).
            if (details.MessageSize is { } masterSize)
            {
                const long mb = 1024L * 1024L;
                if (masterSize >= 50 * mb)
                {
                    warnings.Add(new WarningLine("Calendar master is over 50+ megabytes", Color.Red));
                }
                else if (masterSize >= 25 * mb)
                {
                    warnings.Add(new WarningLine("Calendar master is over 25+ megabytes", Color.Orange));
                }
                else if (masterSize >= 10 * mb)
                {
                    warnings.Add(new WarningLine("Calendar master is over 10+ megabytes", Color.Cyan));
                }
            }

            // Whole-series exception-count warnings (tiered, coloured).
            if (details.HasSeriesCounts && details.TotalExceptions is { } seriesExceptions)
            {
                if (seriesExceptions >= 30)
                {
                    warnings.Add(new WarningLine(
                        "There are over 30+ exceptions for the series.", Color.Orange));
                }
                else if (seriesExceptions >= 20)
                {
                    warnings.Add(new WarningLine(
                        "There are over 20+ exceptions for the series.", Color.Orange));
                }
                else if (seriesExceptions >= 10)
                {
                    warnings.Add(new WarningLine(
                        "There are over 10+ exceptions for the series.", Color.Cyan));
                }
            }

            // Whole-series deletion-count warnings (tiered, coloured).
            if (details.HasSeriesCounts && details.TotalDeletions is { } seriesDeletions)
            {
                if (seriesDeletions >= 30)
                {
                    warnings.Add(new WarningLine(
                        "There are over 30+ deletions for the series.", Color.Orange));
                }
                else if (seriesDeletions >= 20)
                {
                    warnings.Add(new WarningLine(
                        "There are over 20+ deletions for the series.", Color.Orange));
                }
                else if (seriesDeletions >= 10)
                {
                    warnings.Add(new WarningLine(
                        "There are over 10+ deletions for the series.", Color.Cyan));
                }
            }

            sb.AppendLine("RECURRENCE");
            if (details.Recurrence is { } r)
            {
                sb.AppendLine($"  Pattern         : {r.PatternType}");
                sb.AppendLine($"  Interval        : {r.Interval}");
                if (r.DaysOfWeek.Count > 0)
                {
                    sb.AppendLine($"  Days of week    : {string.Join(", ", r.DaysOfWeek)}");
                }
                if (r.DayOfMonth > 0)
                {
                    sb.AppendLine($"  Day of month    : {r.DayOfMonth}");
                }
                if (r.Month > 0)
                {
                    sb.AppendLine($"  Month           : {r.Month}");
                }
                if (r.PatternType is "relativeMonthly" or "relativeYearly")
                {
                    sb.AppendLine($"  Index           : {r.Index}");
                }
                sb.AppendLine($"  First day of wk : {r.FirstDayOfWeek}");
                sb.AppendLine($"  Range type      : {r.RangeType}");
                sb.AppendLine($"  Range start     : {r.RangeStart:yyyy-MM-dd}");

                (DateOnly? end, int? total) = RecurrenceExpander.Summarize(r);

                if (r.RangeType == "endDate" && r.RangeEnd is { } re)
                {
                    sb.AppendLine($"  Range end       : {re:yyyy-MM-dd}");
                }
                else if (r.RangeType == "numbered")
                {
                    sb.AppendLine($"  Occurrences     : {r.NumberOfOccurrences}");
                    if (end is { } computed)
                    {
                        sb.AppendLine($"  Computed end    : {computed:yyyy-MM-dd}");
                    }
                }
                else
                {
                    sb.AppendLine("  Range end       : (not set)");
                }

                if (total is { } t)
                {
                    sb.AppendLine($"  Total occurrences (pattern) : {t}");
                }

                // Warnings.
                if (r.RangeType == "noEnd")
                {
                    warnings.Add(new WarningLine(
                        "The recurrence end date is not set (open-ended series).", Color.Red));
                }

                if (end is { } spanEnd)
                {
                    int days = spanEnd.DayNumber - r.RangeStart.DayNumber;
                    if (days > 366)
                    {
                        warnings.Add(new WarningLine($"The recurrence pattern spans over a year " +
                                     $"({r.RangeStart:yyyy-MM-dd} to {spanEnd:yyyy-MM-dd}).", Color.Red));
                    }
                }
                else if (r.RangeType == "noEnd")
                {
                    warnings.Add(new WarningLine(
                        "The recurrence pattern has no end and therefore exceeds one year.", Color.Red));
                }
            }
            else
            {
                sb.AppendLine("  (single, non-recurring meeting)");
            }
            sb.AppendLine();

            sb.AppendLine("INSTANCE COUNTS (within the selected date range)");
            sb.AppendLine($"  Instances            : {row.InstancesInRange}");
            sb.AppendLine($"  Exceptions (modified): {row.ExceptionsInRange}");
            sb.AppendLine($"  Exception deletions  : {row.DeletionsInRange}");
            if (row.RangeAttachmentsComputed && row.RangeAttachmentCount.HasValue)
            {
                int attCount = row.RangeAttachmentCount.Value;
                long attTotal = row.RangeAttachmentTotalSize ?? 0;
                long? attAverage = attCount > 0 ? attTotal / attCount : (long?)null;
                sb.AppendLine($"  Attachments (count)  : {attCount}");
                sb.AppendLine($"  Attachments total    : {FormatSize(attTotal)}");
                sb.AppendLine($"  Attachments average  : {FormatSize(attAverage)}");
            }
            else
            {
                sb.AppendLine("  Attachments          : (not available)");
            }

            if (row.IsRecurring)
            {
                sb.AppendLine();
                string scope = string.IsNullOrEmpty(row.TotalScopeNote)
                    ? "entire series"
                    : row.TotalScopeNote;
                sb.AppendLine($"INSTANCE COUNTS (whole calendar item - {scope})");
                if (row.TotalsComputed)
                {
                    string instances = row.TotalInstances.HasValue
                        ? row.TotalInstances.Value.ToString()
                        : "unbounded (no end date)";
                    sb.AppendLine($"  Instances            : {instances}");
                    sb.AppendLine($"  Exceptions (modified): {row.TotalExceptions ?? 0}");
                    sb.AppendLine($"  Exception deletions  : {row.TotalDeletions ?? 0}");
                }
                else
                {
                    sb.AppendLine("  (not available)");
                }

                if (details.HasSeriesAttachmentInfo)
                {
                    int attCount = details.SeriesAttachmentCount;
                    long attTotal = details.SeriesAttachmentTotalSize;
                    long? attAverage = attCount > 0 ? attTotal / attCount : (long?)null;
                    sb.AppendLine($"  Attachments (count)  : {attCount}");
                    sb.AppendLine($"  Attachments total    : {FormatSize(attTotal)}");
                    sb.AppendLine($"  Attachments average  : {FormatSize(attAverage)}");
                }
                else
                {
                    sb.AppendLine("  Attachments          : (not available)");
                }
            }

            return (sb.ToString(), warnings);
        }

        private static string Or(string value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback : value;

        private static string FormatSize(long? bytes)
        {
            if (bytes is not { } b)
            {
                return "(unknown)";
            }

            string[] units = { "bytes", "KB", "MB", "GB" };
            double size = b;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            string human = unit == 0
                ? $"{b} {units[unit]}"
                : $"{size:0.##} {units[unit]} ({b:N0} bytes)";
            return human;
        }


        private void btnBrowseLog_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Select log file",
                Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = Path.GetFileName(txtLogPath.Text),
                OverwritePrompt = false
            };

            if (!string.IsNullOrWhiteSpace(txtLogPath.Text))
            {
                string? dir = Path.GetDirectoryName(txtLogPath.Text);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    dialog.InitialDirectory = dir;
                }
            }

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtLogPath.Text = dialog.FileName;
            }
        }

        private void SetBusy(bool busy, string? status)
        {
            UseWaitCursor = busy;
            grpAuth.Enabled = !busy;
            grpQuery.Enabled = !busy;
            if (status is not null)
            {
                lblStatus.Text = status;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
