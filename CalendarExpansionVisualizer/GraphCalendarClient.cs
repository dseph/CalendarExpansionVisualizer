using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CalendarExpansionVisualizer
{
    internal sealed record CalendarInstance(
        string Subject,
        DateTimeOffset? Start,
        DateTimeOffset? End,
        string Organizer,
        string Location,
        bool IsAllDay,
        string SeriesMasterId,
        string Type,
        string Id,
        long? MessageSize = null);

    internal sealed class EventDetails
    {
        public string Id { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string Organizer { get; set; } = string.Empty;
        public List<string> Attendees { get; } = new();
        public string ICalUId { get; set; } = string.Empty;
        public string CleanGlobalObjectId { get; set; } = string.Empty;
        public RecurrenceInfo? Recurrence { get; set; }

        // Whole-series counts read directly from the master's PidLidAppointmentRecur
        // blob (no per-instance searching). Null when unavailable/not applicable.
        public int? TotalInstances { get; set; }
        public int? TotalExceptions { get; set; }
        public int? TotalDeletions { get; set; }
        public string? TotalScopeNote { get; set; }
        public bool HasSeriesCounts { get; set; }

        // Every changed occurrence (all exceptions and all deletions) across the entire
        // series, parsed from the master's recurrence blob. Independent of any date window.
        public List<RecurChange> SeriesChanges { get; } = new();

        // Size of the calendar (master) record on the server, from PidTagMessageSize.
        public long? MessageSize { get; set; }

        // Attachment summary for the calendar record.
        public int? AttachmentCount { get; set; }
        public long? AttachmentsTotalSize { get; set; }

        // The series master's own attachments, used to diff against exception occurrences.
        public List<AttachmentInfo> MasterAttachments { get; } = new();

        // Whole-series attachment aggregates: the master's stored attachments plus each
        // exception occurrence's own stored attachments. Populated when the exception
        // attachment scan runs; the master's own attachments always count.
        public int SeriesAttachmentCount { get; set; }
        public long SeriesAttachmentTotalSize { get; set; }
        public bool HasSeriesAttachmentInfo { get; set; }
    }


    /// <summary>
    /// Queries the Microsoft Graph calendarView endpoint, which expands recurring
    /// series into individual event instances for the requested time window.
    /// </summary>
    internal sealed class GraphCalendarClient : IDisposable
    {
        private readonly string GraphBase;
        private readonly HttpClient _http;

        // PidTagMessageSize (PtypInteger32 0x0E08): size in bytes of the item on the server.
        private const string MessageSizeFilter = "id eq 'Long 0x0E08'";
        private static readonly string MessageSizeExpand =
            "singleValueExtendedProperties($filter=" + Uri.EscapeDataString(MessageSizeFilter) + ")";

        public GraphCalendarClient(string accessToken, string logPath, bool logEnabled,
            string graphRoot = "https://graph.microsoft.com")
        {
            GraphBase = $"{graphRoot.TrimEnd('/')}/v1.0";
            var handler = new GraphLoggingHandler(logPath, logEnabled, new HttpClientHandler());
            _http = new HttpClient(handler);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<CalendarInstance>> GetCalendarViewAsync(
            AuthFlow flow, string? upn, DateTimeOffset start, DateTimeOffset end,
            CancellationToken cancellationToken)
        {
            string basePath = flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}/calendarView"
                : $"{GraphBase}/me/calendarView";

            string startUtc = start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            string endUtc = end.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            string? url =
                $"{basePath}?startDateTime={Uri.EscapeDataString(startUtc)}" +
                $"&endDateTime={Uri.EscapeDataString(endUtc)}" +
                "&$select=id,subject,start,end,organizer,location,isAllDay,seriesMasterId,type" +
                "&$orderby=start/dateTime&$top=100";

            var results = new List<CalendarInstance>();

            while (!string.IsNullOrEmpty(url))
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Request instances in the local time zone so wall-clock dates (including
                // all-day events at midnight) line up with the recurrence pattern dates.
                // Forcing UTC shifts all-day/near-midnight occurrences to the wrong calendar
                // day on machines with a negative UTC offset, which made the last day of the
                // window falsely appear "deleted" (red).
                request.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");

                using var response = await _http.SendAsync(request, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Graph returned {(int)response.StatusCode} {response.ReasonPhrase}: {content}");
                }

                using var doc = JsonDocument.Parse(content);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("value", out JsonElement value) &&
                    value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        results.Add(Parse(item));
                    }
                }

                url = root.TryGetProperty("@odata.nextLink", out JsonElement next)
                    ? next.GetString()
                    : null;
            }

            return results;
        }

        /// <summary>
        /// Fetches a single event (typically a recurring series master) including its
        /// recurrence pattern, so deleted occurrences can be inferred.
        /// </summary>
        public async Task<SeriesMaster?> GetEventAsync(
            AuthFlow flow, string? upn, string eventId, CancellationToken cancellationToken)
        {
            string basePath = flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}/events/{Uri.EscapeDataString(eventId)}"
                : $"{GraphBase}/me/events/{Uri.EscapeDataString(eventId)}";

            string url = $"{basePath}?$select=id,subject,type,recurrence&$expand={MessageSizeExpand}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            var master = new SeriesMaster
            {
                Id = GetString(root, "id"),
                Subject = GetString(root, "subject"),
                Type = GetString(root, "type"),
                MessageSize = ParseMessageSize(root)
            };

            if (root.TryGetProperty("recurrence", out var recurrence) &&
                recurrence.ValueKind == JsonValueKind.Object)
            {
                master.Recurrence = ParseRecurrence(recurrence);
            }

            return master;
        }

        /// <summary>
        /// Lists every recurring series master in the mailbox (regardless of date), so
        /// that series which are active over the selected window but have no instance
        /// inside it (e.g. a yearly March meeting viewed in June) can still be shown.
        /// </summary>
        public async Task<List<SeriesMaster>> GetSeriesMastersAsync(
            AuthFlow flow, string? upn, CancellationToken cancellationToken)
        {
            string basePath = flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}/events"
                : $"{GraphBase}/me/events";

            string? url = $"{basePath}?$select=id,subject,type,recurrence&$expand={MessageSizeExpand}&$top=100";

            var results = new List<SeriesMaster>();
            int pageGuard = 0;

            while (!string.IsNullOrEmpty(url) && pageGuard++ < 200)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _http.SendAsync(request, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Graph returned {(int)response.StatusCode} {response.ReasonPhrase}: {content}");
                }

                using var doc = JsonDocument.Parse(content);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("value", out JsonElement value) &&
                    value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in value.EnumerateArray())
                    {
                        // The /events collection returns seriesMasters and singleInstances;
                        // filter to masters client-side ($filter on 'type' is not supported).
                        if (!string.Equals(GetString(item, "type"), "seriesMaster",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var master = new SeriesMaster
                        {
                            Id = GetString(item, "id"),
                            Subject = GetString(item, "subject"),
                            Type = GetString(item, "type"),
                            MessageSize = ParseMessageSize(item)
                        };
                        if (item.TryGetProperty("recurrence", out var rec) &&
                            rec.ValueKind == JsonValueKind.Object)
                        {
                            master.Recurrence = ParseRecurrence(rec);
                        }
                        results.Add(master);
                    }
                }

                url = root.TryGetProperty("@odata.nextLink", out JsonElement next)
                    ? next.GetString()
                    : null;
            }

            return results;
        }

        /// <summary>
        /// Fetches full details for an event (or series master), including organizer,
        /// attendees, iCalUId, recurrence and the CleanGlobalObjectId extended property.
        /// </summary>
        public async Task<EventDetails?> GetEventDetailsAsync(
            AuthFlow flow, string? upn, string eventId, CancellationToken cancellationToken)
        {
            string basePath = flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}/events/{Uri.EscapeDataString(eventId)}"
                : $"{GraphBase}/me/events/{Uri.EscapeDataString(eventId)}";

            // CleanGlobalObjectId: PSETID_Meeting {6ED8DA90-450B-101B-98DA-00AA003F1305}, id 0x23.
            const string cleanGlobalObjectIdProp = "Binary {6ED8DA90-450B-101B-98DA-00AA003F1305} Id 0x23";
            string filter = $"id eq '{cleanGlobalObjectIdProp}'";
            string expand = "singleValueExtendedProperties($filter=" + Uri.EscapeDataString(filter) + ")";

            string url = $"{basePath}?$select=id,subject,start,end,organizer,attendees,iCalUId,type,recurrence,hasAttachments" +
                         $"&$expand={expand},attachments($select=id,name,size)";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Graph returned {(int)response.StatusCode} {response.ReasonPhrase}: {content}");
            }

            using var doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            var details = new EventDetails
            {
                Id = GetString(root, "id"),
                Subject = GetString(root, "subject"),
                Type = GetString(root, "type"),
                ICalUId = GetString(root, "iCalUId"),
                Start = GetDateTimeText(root, "start"),
                End = GetDateTimeText(root, "end")
            };

            if (root.TryGetProperty("organizer", out var org) &&
                org.TryGetProperty("emailAddress", out var orgEmail))
            {
                string name = GetString(orgEmail, "name");
                string addr = GetString(orgEmail, "address");
                details.Organizer = string.IsNullOrEmpty(name) ? addr : $"{name} <{addr}>";
            }

            if (root.TryGetProperty("attendees", out var attendees) &&
                attendees.ValueKind == JsonValueKind.Array)
            {
                foreach (var att in attendees.EnumerateArray())
                {
                    string name = string.Empty;
                    string addr = string.Empty;
                    if (att.TryGetProperty("emailAddress", out var ae))
                    {
                        name = GetString(ae, "name");
                        addr = GetString(ae, "address");
                    }
                    string attType = GetString(att, "type");
                    string respStatus = string.Empty;
                    if (att.TryGetProperty("status", out var st))
                    {
                        respStatus = GetString(st, "response");
                    }

                    string display = string.IsNullOrEmpty(name) ? addr : $"{name} <{addr}>";
                    details.Attendees.Add($"{display} [{attType}, {respStatus}]");
                }
            }

            if (root.TryGetProperty("recurrence", out var recurrence) &&
                recurrence.ValueKind == JsonValueKind.Object)
            {
                details.Recurrence = ParseRecurrence(recurrence);
            }

            if (root.TryGetProperty("attachments", out var atts) &&
                atts.ValueKind == JsonValueKind.Array)
            {
                int count = 0;
                long totalSize = 0;
                foreach (var att in atts.EnumerateArray())
                {
                    count++;
                    long? attSize = att.TryGetProperty("size", out var sz) && sz.TryGetInt64(out long s)
                        ? s
                        : null;
                    totalSize += attSize ?? 0;
                    details.MasterAttachments.Add(new AttachmentInfo(GetString(att, "name"), attSize));
                }
                details.AttachmentCount = count;
                details.AttachmentsTotalSize = totalSize;

                // Seed the whole-series aggregates with the master's own attachments. The
                // exception scan (if it runs) adds each exception occurrence's attachments.
                details.SeriesAttachmentCount = count;
                details.SeriesAttachmentTotalSize = totalSize;
                details.HasSeriesAttachmentInfo = true;
            }

            if (root.TryGetProperty("singleValueExtendedProperties", out var svep) &&
                svep.ValueKind == JsonValueKind.Array)
            {
                foreach (var prop in svep.EnumerateArray())
                {
                    string value = GetString(prop, "value");
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    // CleanGlobalObjectId (binary -> hex).
                    try
                    {
                        details.CleanGlobalObjectId = Convert.ToHexString(Convert.FromBase64String(value));
                    }
                    catch (FormatException)
                    {
                        details.CleanGlobalObjectId = value;
                    }
                    break;
                }
            }

            // For a recurring series, read the whole-series occurrence/exception/deletion
            // counts directly from the master's PidLidAppointmentRecur blob. This is a
            // single targeted request (not a per-instance search).
            if (details.Recurrence is not null)
            {
                await PopulateSeriesCountsAsync(basePath, details, cancellationToken);
                await PopulateExceptionAttachmentChangesAsync(basePath, details, cancellationToken);
            }

            // Size of the calendar record itself (PidTagMessageSize). Single targeted request.
            await PopulateMessageSizeAsync(basePath, details, cancellationToken);

            return details;
        }

        /// <summary>
        /// Fetches the PidLidAppointmentRecur (PSETID_Appointment id 0x8216) binary blob
        /// for a series master and parses the whole-series counts from it. Uses its own
        /// single-property request so the extended-property expansion is reliable.
        /// </summary>
        private async Task PopulateSeriesCountsAsync(
            string basePath, EventDetails details, CancellationToken cancellationToken)
        {
            const string apptRecurProp = "Binary {00062002-0000-0000-C000-000000000046} Id 0x8216";
            string filter = $"id eq '{apptRecurProp}'";
            string expand = "singleValueExtendedProperties($filter=" + Uri.EscapeDataString(filter) + ")";
            string url = $"{basePath}?$select=id&$expand={expand}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            using var doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("singleValueExtendedProperties", out var svep) ||
                svep.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var prop in svep.EnumerateArray())
            {
                string value = GetString(prop, "value");
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                try
                {
                    byte[] blob = Convert.FromBase64String(value);
                    if (AppointmentRecurParser.TryParse(blob, out SeriesCounts counts))
                    {
                        details.TotalInstances = counts.Instances;
                        details.TotalExceptions = counts.Exceptions;
                        details.TotalDeletions = counts.Deletions;
                        details.TotalScopeNote = counts.Endless
                            ? "entire series (no end date)"
                            : "entire series";
                        details.HasSeriesCounts = true;
                    }

                    if (AppointmentRecurParser.TryParseDetails(blob, out List<RecurChange> blobChanges))
                    {
                        details.SeriesChanges.Clear();
                        details.SeriesChanges.AddRange(blobChanges);
                    }
                }
                catch (FormatException)
                {
                    // Leave counts unset if the blob can't be decoded.
                }
                break;
            }
        }

        /// <summary>
        /// Reads the whole-series exception count for a series master directly from its
        /// PidLidAppointmentRecur blob. Returns null if it can't be determined. Uses a
        /// single targeted extended-property request so expansion is reliable.
        /// </summary>
        public async Task<int?> GetSeriesExceptionCountAsync(
            AuthFlow flow, string? upn, string eventId, CancellationToken cancellationToken)
        {
            string basePath = flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}/events/{Uri.EscapeDataString(eventId)}"
                : $"{GraphBase}/me/events/{Uri.EscapeDataString(eventId)}";

            const string apptRecurProp = "Binary {00062002-0000-0000-C000-000000000046} Id 0x8216";
            string filter = $"id eq '{apptRecurProp}'";
            string expand = "singleValueExtendedProperties($filter=" + Uri.EscapeDataString(filter) + ")";
            string url = $"{basePath}?$select=id&$expand={expand}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _http.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("singleValueExtendedProperties", out var svep) ||
                    svep.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                foreach (var prop in svep.EnumerateArray())
                {
                    string value = GetString(prop, "value");
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    byte[] blob = Convert.FromBase64String(value);
                    if (AppointmentRecurParser.TryParse(blob, out SeriesCounts counts))
                    {
                        return counts.Exceptions;
                    }
                    break;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or FormatException or JsonException)
            {
                // Best-effort: leave the count unknown.
            }

            return null;
        }

        /// <summary>
        /// Reads PidTagMessageSize (PtypInteger32 0x0E08), the size in bytes of the
        /// calendar record on the server, using its own single-property request.
        /// </summary>
        private async Task PopulateMessageSizeAsync(
            string basePath, EventDetails details, CancellationToken cancellationToken)
        {
            const string messageSizeProp = "Long 0x0E08";
            string filter = $"id eq '{messageSizeProp}'";
            string expand = "singleValueExtendedProperties($filter=" + Uri.EscapeDataString(filter) + ")";
            string url = $"{basePath}?$select=id&$expand={expand}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(request, cancellationToken);
            string content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            using var doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("singleValueExtendedProperties", out var svep) ||
                svep.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var prop in svep.EnumerateArray())
            {
                string value = GetString(prop, "value");
                if (long.TryParse(value, out long size))
                {
                    details.MessageSize = size;
                }
                break;
            }
        }

        /// <summary>
        /// For each exception occurrence of the series, retrieves its attachments from
        /// Graph and diffs them against the series master's attachments so added/removed
        /// files (name and size) can be listed. Uses the series master's /instances
        /// expansion over the span covered by the parsed exceptions (a single bounded
        /// query with paging) rather than one request per exception.
        /// </summary>
        private async Task PopulateExceptionAttachmentChangesAsync(
            string basePath, EventDetails details, CancellationToken cancellationToken)
        {
            var exceptions = details.SeriesChanges
                .Where(c => c.Kind == RecurChangeKind.Exception)
                .ToList();
            if (exceptions.Count == 0)
            {
                return;
            }

            // Bound the instances window to the span covered by the parsed exceptions,
            // padded by a day so occurrences at the edges are included.
            DateTime min = exceptions.Min(c => c.Start < c.OriginalStart ? c.Start : c.OriginalStart);
            DateTime max = exceptions.Max(c => c.End > c.OriginalEnd ? c.End : c.OriginalEnd);
            string startText = min.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss");
            string endText = max.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss");

            var masterSet = new HashSet<string>(
                details.MasterAttachments.Select(AttachmentKey), StringComparer.OrdinalIgnoreCase);

            string? url =
                $"{basePath}/instances?startDateTime={Uri.EscapeDataString(startText)}" +
                $"&endDateTime={Uri.EscapeDataString(endText)}" +
                "&$select=id,type,start,end&$expand=attachments($select=name,size)&$top=100";

            // Graph occurrences returned for the window, keyed to the closest exception.
            var occurrences = new List<(DateTimeOffset Start, List<AttachmentInfo> Attachments)>();
            int pageGuard = 0;

            try
            {
                while (!string.IsNullOrEmpty(url) && pageGuard++ < 100)
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");

                    using var response = await _http.SendAsync(request, cancellationToken);
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    using var doc = JsonDocument.Parse(content);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("value", out var value) &&
                        value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in value.EnumerateArray())
                        {
                            if (!string.Equals(GetString(item, "type"), "exception",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            DateTimeOffset? occStart = GetDateTime(item, "start");
                            if (occStart is null)
                            {
                                continue;
                            }

                            var atts = new List<AttachmentInfo>();
                            if (item.TryGetProperty("attachments", out var arr) &&
                                arr.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var att in arr.EnumerateArray())
                                {
                                    long? attSize = att.TryGetProperty("size", out var sz) &&
                                                    sz.TryGetInt64(out long s) ? s : null;
                                    atts.Add(new AttachmentInfo(GetString(att, "name"), attSize));
                                }
                            }

                            occurrences.Add((occStart.Value, atts));
                        }
                    }

                    url = root.TryGetProperty("@odata.nextLink", out var next)
                        ? next.GetString()
                        : null;
                }
            }
            catch (HttpRequestException)
            {
                return;
            }

            // Match each Graph exception occurrence to the closest parsed exception by
            // start time (both are local wall-clock), then diff attachments.
            var used = new bool[occurrences.Count];
            foreach (RecurChange change in exceptions)
            {
                int best = -1;
                double bestDiff = double.MaxValue;
                for (int i = 0; i < occurrences.Count; i++)
                {
                    if (used[i])
                    {
                        continue;
                    }
                    double diff = Math.Abs((occurrences[i].Start.DateTime - change.Start).TotalMinutes);
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        best = i;
                    }
                }

                // Only accept a match within two days to avoid mispairing occurrences.
                if (best < 0 || bestDiff > 2 * 24 * 60)
                {
                    continue;
                }

                used[best] = true;
                change.AttachmentInfoKnown = true;

                var occAttachments = occurrences[best].Attachments;
                var occKeys = new HashSet<string>(
                    occAttachments.Select(AttachmentKey), StringComparer.OrdinalIgnoreCase);

                // This exception is a separately stored item; add its attachments to the
                // whole-series aggregates (the master's were seeded already).
                details.SeriesAttachmentCount += occAttachments.Count;
                details.SeriesAttachmentTotalSize += occAttachments.Sum(a => a.Size ?? 0);

                foreach (AttachmentInfo a in occAttachments)
                {
                    if (!masterSet.Contains(AttachmentKey(a)))
                    {
                        change.AttachmentChanges.Add(new AttachmentChange
                        {
                            Kind = AttachmentChangeKind.Added,
                            Name = a.Name,
                            Size = a.Size
                        });
                    }
                }

                foreach (AttachmentInfo a in details.MasterAttachments)
                {
                    if (!occKeys.Contains(AttachmentKey(a)))
                    {
                        change.AttachmentChanges.Add(new AttachmentChange
                        {
                            Kind = AttachmentChangeKind.Removed,
                            Name = a.Name,
                            Size = a.Size
                        });
                    }
                }
            }
        }

        private static string AttachmentKey(AttachmentInfo a) => $"{a.Name}|{a.Size}";

        /// <summary>
        /// Sums the attachments across every instance of an event that falls within the
        /// given window. For a recurring series this expands the master's occurrences over
        /// the window (each occurrence carries its own attachments); for a single meeting
        /// it uses the event's own attachments. Returns whether the data was retrievable.
        /// </summary>
        public async Task<(int Count, long TotalSize, bool Known)> GetRangeAttachmentSummaryAsync(
            AuthFlow flow, string? upn, string eventId, bool isRecurring,
            DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
        {
            string basePath = flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}/events/{Uri.EscapeDataString(eventId)}"
                : $"{GraphBase}/me/events/{Uri.EscapeDataString(eventId)}";

            // Non-recurring meeting: its own attachments are the only ones in range.
            if (!isRecurring)
            {
                string singleUrl = $"{basePath}?$select=id&$expand=attachments($select=size)";
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, singleUrl);
                    using var resp = await _http.SendAsync(req, cancellationToken);
                    string body = await resp.Content.ReadAsStringAsync(cancellationToken);
                    if (!resp.IsSuccessStatusCode)
                    {
                        return (0, 0, false);
                    }

                    using var d = JsonDocument.Parse(body);
                    return (SumAttachments(d.RootElement), SumAttachmentSizes(d.RootElement), true);
                }
                catch (HttpRequestException)
                {
                    return (0, 0, false);
                }
            }

            string startText = start.ToString("yyyy-MM-ddTHH:mm:ss");
            string endText = end.ToString("yyyy-MM-ddTHH:mm:ss");
            string? url =
                $"{basePath}/instances?startDateTime={Uri.EscapeDataString(startText)}" +
                $"&endDateTime={Uri.EscapeDataString(endText)}" +
                "&$select=id,type&$expand=attachments($select=size)&$top=100";

            int count = 0;
            long totalSize = 0;
            int pageGuard = 0;

            try
            {
                while (!string.IsNullOrEmpty(url) && pageGuard++ < 100)
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Prefer", $"outlook.timezone=\"{TimeZoneInfo.Local.Id}\"");

                    using var response = await _http.SendAsync(request, cancellationToken);
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        return (0, 0, false);
                    }

                    using var doc = JsonDocument.Parse(content);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("value", out var value) &&
                        value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in value.EnumerateArray())
                        {
                            count += SumAttachments(item);
                            totalSize += SumAttachmentSizes(item);
                        }
                    }

                    url = root.TryGetProperty("@odata.nextLink", out var next)
                        ? next.GetString()
                        : null;
                }
            }
            catch (HttpRequestException)
            {
                return (0, 0, false);
            }

            return (count, totalSize, true);
        }

        private static int SumAttachments(JsonElement item)
        {
            if (item.TryGetProperty("attachments", out var arr) &&
                arr.ValueKind == JsonValueKind.Array)
            {
                return arr.GetArrayLength();
            }
            return 0;
        }

        private static long SumAttachmentSizes(JsonElement item)
        {
            long total = 0;
            if (item.TryGetProperty("attachments", out var arr) &&
                arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var att in arr.EnumerateArray())
                {
                    if (att.TryGetProperty("size", out var sz) && sz.TryGetInt64(out long s))
                    {
                        total += s;
                    }
                }
            }
            return total;
        }

        private static string GetDateTimeText(JsonElement element, string name)
        {
            if (element.TryGetProperty(name, out var dateObj) &&
                dateObj.ValueKind == JsonValueKind.Object)
            {
                string dt = GetString(dateObj, "dateTime");
                string tz = GetString(dateObj, "timeZone");
                if (!string.IsNullOrEmpty(dt))
                {
                    return string.IsNullOrEmpty(tz) ? dt : $"{dt} ({tz})";
                }
            }
            return string.Empty;
        }

        private static RecurrenceInfo? ParseRecurrence(JsonElement recurrence)
        {
            if (!recurrence.TryGetProperty("pattern", out var pattern) ||
                !recurrence.TryGetProperty("range", out var range))
            {
                return null;
            }

            var info = new RecurrenceInfo
            {
                PatternType = GetString(pattern, "type"),
                Interval = pattern.TryGetProperty("interval", out var iv) && iv.TryGetInt32(out int ivv) ? ivv : 1,
                DayOfMonth = pattern.TryGetProperty("dayOfMonth", out var dom) && dom.TryGetInt32(out int domv) ? domv : 0,
                Month = pattern.TryGetProperty("month", out var mo) && mo.TryGetInt32(out int mov) ? mov : 0,
                Index = GetStringOrDefault(pattern, "index", "first"),
                FirstDayOfWeek = ParseDay(GetStringOrDefault(pattern, "firstDayOfWeek", "sunday")) ?? DayOfWeek.Sunday,
                RangeType = GetStringOrDefault(range, "type", "noEnd"),
                NumberOfOccurrences = range.TryGetProperty("numberOfOccurrences", out var no) && no.TryGetInt32(out int nov) ? nov : 0
            };

            if (pattern.TryGetProperty("daysOfWeek", out var dow) && dow.ValueKind == JsonValueKind.Array)
            {
                foreach (var day in dow.EnumerateArray())
                {
                    if (day.ValueKind == JsonValueKind.String && ParseDay(day.GetString()) is { } parsedDay)
                    {
                        info.DaysOfWeek.Add(parsedDay);
                    }
                }
            }

            if (DateOnly.TryParse(GetString(range, "startDate"), out DateOnly start))
            {
                info.RangeStart = start;
            }

            if (info.RangeType == "endDate" &&
                DateOnly.TryParse(GetString(range, "endDate"), out DateOnly end))
            {
                info.RangeEnd = end;
            }

            return info;
        }

        private static DayOfWeek? ParseDay(string? value) => value?.ToLowerInvariant() switch
        {
            "sunday" => DayOfWeek.Sunday,
            "monday" => DayOfWeek.Monday,
            "tuesday" => DayOfWeek.Tuesday,
            "wednesday" => DayOfWeek.Wednesday,
            "thursday" => DayOfWeek.Thursday,
            "friday" => DayOfWeek.Friday,
            "saturday" => DayOfWeek.Saturday,
            _ => null
        };

        private static string GetStringOrDefault(JsonElement element, string name, string fallback)
        {
            string value = GetString(element, name);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        private static CalendarInstance Parse(JsonElement item)
        {
            string subject = GetString(item, "subject");
            bool isAllDay = item.TryGetProperty("isAllDay", out var allDay) &&
                            allDay.ValueKind == JsonValueKind.True;
            string seriesMasterId = GetString(item, "seriesMasterId");
            string type = GetString(item, "type");

            string organizer = string.Empty;
            if (item.TryGetProperty("organizer", out var org) &&
                org.TryGetProperty("emailAddress", out var email))
            {
                organizer = GetString(email, "name");
                if (string.IsNullOrEmpty(organizer))
                {
                    organizer = GetString(email, "address");
                }
            }

            string location = string.Empty;
            if (item.TryGetProperty("location", out var loc))
            {
                location = GetString(loc, "displayName");
            }

            return new CalendarInstance(
                subject,
                GetDateTime(item, "start"),
                GetDateTime(item, "end"),
                organizer,
                location,
                isAllDay,
                seriesMasterId,
                type,
                GetString(item, "id"),
                ParseMessageSize(item));
        }

        /// <summary>
        /// Reads the PidTagMessageSize value (in bytes) from an item's expanded
        /// singleValueExtendedProperties, or null when it's not present.
        /// </summary>
        private static long? ParseMessageSize(JsonElement element)
        {
            if (!element.TryGetProperty("singleValueExtendedProperties", out var svep) ||
                svep.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var prop in svep.EnumerateArray())
            {
                if (long.TryParse(GetString(prop, "value"), out long size))
                {
                    return size;
                }
            }

            return null;
        }

        private static string GetString(JsonElement element, string name)
            => element.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;

        private static DateTimeOffset? GetDateTime(JsonElement element, string name)
        {
            if (element.TryGetProperty(name, out var dateObj) &&
                dateObj.TryGetProperty("dateTime", out var dt) &&
                dt.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(dt.GetString(), out DateTime parsed))
            {
                // Graph returns the dateTime as wall-clock in the requested time zone
                // (see the Prefer header on calendarView). Treat an offset-less value as
                // local wall-clock so the calendar date matches the recurrence pattern.
                if (parsed.Kind == DateTimeKind.Unspecified)
                {
                    parsed = DateTime.SpecifyKind(parsed, DateTimeKind.Local);
                }

                return new DateTimeOffset(parsed);
            }

            return null;
        }

        // ---------------------------------------------------------------------
        // Ad-hoc Graph calls used by the Tests window (frmTests). Each returns the
        // raw response body; requests/responses are logged by GraphLoggingHandler
        // when logging is enabled, so no extra logging is needed here.
        // ---------------------------------------------------------------------

        /// <summary>Mailbox root: /me for delegated, /users/{upn} for application flow.</summary>
        private string MailboxBase(AuthFlow flow, string? upn)
            => flow == AuthFlow.Application
                ? $"{GraphBase}/users/{Uri.EscapeDataString(upn ?? string.Empty)}"
                : $"{GraphBase}/me";

        private static string Utc(DateTimeOffset value)
            => value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

        private static string EscapeFilterLiteral(string value)
            => value.Replace("'", "''");

        private async Task<string> SendGetRawAsync(
            string url, CancellationToken cancellationToken, string? preferTimeZone = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(preferTimeZone))
            {
                request.Headers.Add("Prefer", $"outlook.timezone=\"{preferTimeZone}\"");
            }

            using var response = await _http.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            return $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\r\n{body}";
        }

        /// <summary>POST /calendar/getSchedule for the given schedules and window.</summary>
        public async Task<string> GetScheduleAsync(
            AuthFlow flow, string? upn, IEnumerable<string> schedules,
            DateTimeOffset start, DateTimeOffset end, int availabilityViewInterval,
            CancellationToken cancellationToken)
        {
            string url = $"{MailboxBase(flow, upn)}/calendar/getSchedule";

            var payload = new
            {
                schedules = schedules.ToArray(),
                startTime = new { dateTime = start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
                endTime = new { dateTime = end.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), timeZone = "UTC" },
                availabilityViewInterval
            };

            string json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var response = await _http.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            return $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\r\n{body}";
        }

        /// <summary>GET /calendarView over a window (recurrences expanded).</summary>
        public Task<string> CalendarViewRawAsync(
            AuthFlow flow, string? upn, DateTimeOffset start, DateTimeOffset end,
            CancellationToken cancellationToken)
        {
            string url =
                $"{MailboxBase(flow, upn)}/calendarView" +
                $"?startDateTime={Uri.EscapeDataString(Utc(start))}" +
                $"&endDateTime={Uri.EscapeDataString(Utc(end))}" +
                "&$select=id,subject,start,end,organizer,isAllDay,type,seriesMasterId" +
                "&$orderby=start/dateTime&$top=50";

            return SendGetRawAsync(url, cancellationToken, TimeZoneInfo.Local.Id);
        }

        /// <summary>GET /events filtered by subject within a start/end range.</summary>
        public Task<string> SearchEventsBySubjectInRangeAsync(
            AuthFlow flow, string? upn, string subject,
            DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
        {
            string filter =
                $"subject eq '{EscapeFilterLiteral(subject)}'" +
                $" and start/dateTime ge '{Utc(start)}'" +
                $" and end/dateTime le '{Utc(end)}'";

            string url =
                $"{MailboxBase(flow, upn)}/events" +
                $"?$filter={Uri.EscapeDataString(filter)}" +
                "&$select=id,subject,start,end,type&$top=50";

            return SendGetRawAsync(url, cancellationToken);
        }

        /// <summary>GET /events filtered by an exact subject across the whole mailbox.</summary>
        public Task<string> SearchAllEventsBySubjectAsync(
            AuthFlow flow, string? upn, string subject, CancellationToken cancellationToken)
        {
            string filter = $"subject eq '{EscapeFilterLiteral(subject)}'";

            string url =
                $"{MailboxBase(flow, upn)}/events" +
                $"?$filter={Uri.EscapeDataString(filter)}" +
                "&$select=id,subject,start,end,type&$top=50";

            return SendGetRawAsync(url, cancellationToken);
        }

        /// <summary>GET /events/{id}/instances over a date/time window.</summary>
        public Task<string> GetEventInstancesAsync(
            AuthFlow flow, string? upn, string eventId,
            DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
        {
            string url =
                $"{MailboxBase(flow, upn)}/events/{Uri.EscapeDataString(eventId)}/instances" +
                $"?startDateTime={Uri.EscapeDataString(Utc(start))}" +
                $"&endDateTime={Uri.EscapeDataString(Utc(end))}" +
                "&$select=id,subject,start,end,type&$top=50";

            return SendGetRawAsync(url, cancellationToken, TimeZoneInfo.Local.Id);
        }

        /// <summary>GET /calendarView/delta over a window (delta token in the response).</summary>
        public Task<string> CalendarViewDeltaAsync(
            AuthFlow flow, string? upn, DateTimeOffset start, DateTimeOffset end,
            CancellationToken cancellationToken)
        {
            string url =
                $"{MailboxBase(flow, upn)}/calendarView/delta" +
                $"?startDateTime={Uri.EscapeDataString(Utc(start))}" +
                $"&endDateTime={Uri.EscapeDataString(Utc(end))}";

            return SendGetRawAsync(url, cancellationToken, TimeZoneInfo.Local.Id);
        }

        public void Dispose() => _http.Dispose();
    }
}