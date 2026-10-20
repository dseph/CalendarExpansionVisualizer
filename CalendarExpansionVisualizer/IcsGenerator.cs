using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Serializes a Microsoft Graph calendar master record as an RFC 5545 iCalendar object.
    /// </summary>
    internal static partial class IcsGenerator
    {
        private const string ProductId =
            "-//Calendar Expansion Visualizer//Generated ICS 1.0//EN";

        public static string Generate(EventDetails master)
        {
            ArgumentNullException.ThrowIfNull(master);

            if (string.Equals(master.Type, "occurrence", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(master.Type, "exception", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "ICS generation requires a calendar master record, not a recurrence instance.",
                    nameof(master));
            }

            if (string.IsNullOrWhiteSpace(master.StartDateTime) ||
                string.IsNullOrWhiteSpace(master.EndDateTime))
            {
                throw new InvalidOperationException(
                    "The calendar master does not contain a complete start and end time.");
            }

            string uid = BuildUid(master);
            DateTimeOffset generatedAt = DateTimeOffset.UtcNow;
            var writer = new IcsWriter();

            writer.Write("BEGIN:VCALENDAR");
            writer.Write("VERSION:2.0");
            writer.Write($"PRODID:{ProductId}");
            writer.Write("CALSCALE:GREGORIAN");

            foreach (string timeZoneId in RequiredTimeZones(master))
            {
                WriteTimeZone(writer, timeZoneId, master);
            }

            WriteMasterEvent(writer, master, uid, generatedAt);
            foreach (RecurChange exception in master.SeriesChanges
                         .Where(change => change.Kind == RecurChangeKind.Exception)
                         .OrderBy(change => change.OriginalStart))
            {
                WriteExceptionEvent(writer, master, exception, uid, generatedAt);
            }

            writer.Write("END:VCALENDAR");
            return writer.ToString();
        }

        private static void WriteMasterEvent(
            IcsWriter writer, EventDetails master, string uid, DateTimeOffset generatedAt)
        {
            writer.Write("BEGIN:VEVENT");
            writer.Write($"UID:{EscapeText(uid)}");
            writer.Write($"DTSTAMP:{FormatUtc(generatedAt)}");
            WriteAuditDates(writer, master);
            WriteEventTimes(writer, master);

            if (master.Recurrence is { } recurrence)
            {
                writer.Write($"RRULE:{BuildRecurrenceRule(master, recurrence)}");
                WriteExDates(writer, master);
            }

            WriteCommonProperties(writer, master, master.Subject, master.Location);
            WriteAlarm(writer, master);
            writer.Write("END:VEVENT");
        }

        private static void WriteExceptionEvent(
            IcsWriter writer, EventDetails master, RecurChange exception,
            string uid, DateTimeOffset generatedAt)
        {
            bool isAllDay = exception.IsAllDay ?? master.IsAllDay;
            writer.Write("BEGIN:VEVENT");
            writer.Write($"UID:{EscapeText(uid)}");
            writer.Write($"DTSTAMP:{FormatUtc(generatedAt)}");
            WriteAuditDates(writer, master);
            writer.Write(BuildWallTimeProperty(
                "RECURRENCE-ID", exception.OriginalStart, master.StartTimeZone, master.IsAllDay));
            writer.Write(BuildWallTimeProperty(
                "DTSTART", exception.Start, master.StartTimeZone, isAllDay));
            writer.Write(BuildWallTimeProperty(
                "DTEND", exception.End, master.EndTimeZone, isAllDay));
            WriteCommonProperties(
                writer,
                master,
                exception.Subject ?? master.Subject,
                exception.Location ?? master.Location,
                exception.BusyStatus is { } busyStatus
                    ? MapBusyStatusOverride(busyStatus)
                    : null);
            WriteAlarm(
                writer,
                master,
                exception.IsReminderOn,
                exception.ReminderMinutesBeforeStart);
            writer.Write("END:VEVENT");
        }

        private static void WriteEventTimes(IcsWriter writer, EventDetails master)
        {
            DateTime start = ParseGraphWallTime(master.StartDateTime);
            DateTime end = ParseGraphWallTime(master.EndDateTime);

            if (master.IsAllDay)
            {
                writer.Write($"DTSTART;VALUE=DATE:{start:yyyyMMdd}");
                writer.Write($"DTEND;VALUE=DATE:{end:yyyyMMdd}");
                return;
            }

            writer.Write(BuildWallTimeProperty(
                "DTSTART", start, master.StartTimeZone, isAllDay: false));
            writer.Write(BuildWallTimeProperty(
                "DTEND", end, master.EndTimeZone, isAllDay: false));
        }

        private static void WriteAuditDates(IcsWriter writer, EventDetails master)
        {
            if (master.CreatedDateTime is { } created)
            {
                writer.Write($"CREATED:{FormatUtc(created)}");
            }

            if (master.LastModifiedDateTime is { } modified)
            {
                writer.Write($"LAST-MODIFIED:{FormatUtc(modified)}");
            }
        }

        private static void WriteCommonProperties(
            IcsWriter writer, EventDetails master, string subject, string location,
            string? showAsOverride = null)
        {
            writer.Write($"SUMMARY:{EscapeText(subject)}");

            if (!string.IsNullOrEmpty(master.BodyContent))
            {
                string plainText = string.Equals(
                    master.BodyContentType, "html", StringComparison.OrdinalIgnoreCase)
                    ? HtmlToPlainText(master.BodyContent)
                    : master.BodyContent;
                writer.Write($"DESCRIPTION:{EscapeText(plainText)}");

                if (string.Equals(
                        master.BodyContentType, "html", StringComparison.OrdinalIgnoreCase))
                {
                    writer.Write(
                        $"X-ALT-DESC;FMTTYPE=text/html:{EscapeText(master.BodyContent)}");
                }
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                writer.Write($"LOCATION:{EscapeText(location)}");
            }

            WriteOrganizer(writer, master);
            WriteAttendees(writer, master);

            if (master.Categories.Count > 0)
            {
                writer.Write(
                    $"CATEGORIES:{string.Join(",", master.Categories.Select(EscapeText))}");
            }

            writer.Write(master.IsCancelled ? "STATUS:CANCELLED" : "STATUS:CONFIRMED");
            string showAs = showAsOverride ?? master.ShowAs;
            writer.Write(string.Equals(showAs, "free", StringComparison.OrdinalIgnoreCase)
                ? "TRANSP:TRANSPARENT"
                : "TRANSP:OPAQUE");
            writer.Write($"CLASS:{MapClass(master.Sensitivity)}");

            if (TryGetAbsoluteUri(master.OnlineMeetingUrl, out string? onlineUrl))
            {
                writer.Write($"URL:{onlineUrl}");
            }
            else if (TryGetAbsoluteUri(master.WebLink, out string? webUrl))
            {
                writer.Write($"URL:{webUrl}");
            }

            if (!string.IsNullOrWhiteSpace(showAs))
            {
                writer.Write(
                    $"X-MICROSOFT-CDO-BUSYSTATUS:{MapMicrosoftBusyStatus(showAs)}");
            }
        }

        private static void WriteOrganizer(IcsWriter writer, EventDetails master)
        {
            if (!TryGetMailto(master.OrganizerAddress, out string? mailto))
            {
                return;
            }

            string parameters = string.IsNullOrWhiteSpace(master.OrganizerName)
                ? string.Empty
                : $";CN={EncodeParameter(master.OrganizerName)}";
            writer.Write($"ORGANIZER{parameters}:{mailto}");
        }

        private static void WriteAttendees(IcsWriter writer, EventDetails master)
        {
            foreach (EventAttendee attendee in master.StructuredAttendees)
            {
                if (!TryGetMailto(attendee.Address, out string? mailto))
                {
                    continue;
                }

                var parameters = new List<string>();
                if (!string.IsNullOrWhiteSpace(attendee.Name))
                {
                    parameters.Add($"CN={EncodeParameter(attendee.Name)}");
                }

                switch (attendee.Type.ToLowerInvariant())
                {
                    case "optional":
                        parameters.Add("ROLE=OPT-PARTICIPANT");
                        break;
                    case "resource":
                        parameters.Add("CUTYPE=RESOURCE");
                        parameters.Add("ROLE=NON-PARTICIPANT");
                        break;
                    default:
                        parameters.Add("ROLE=REQ-PARTICIPANT");
                        break;
                }

                parameters.Add($"PARTSTAT={MapParticipationStatus(attendee.Response)}");
                parameters.Add($"RSVP={(master.ResponseRequested ? "TRUE" : "FALSE")}");
                writer.Write($"ATTENDEE;{string.Join(";", parameters)}:{mailto}");
            }
        }

        private static void WriteAlarm(
            IcsWriter writer, EventDetails master,
            bool? isReminderOnOverride = null,
            int? reminderMinutesOverride = null)
        {
            bool isReminderOn = isReminderOnOverride ?? master.IsReminderOn;
            int? reminderMinutes = reminderMinutesOverride ?? master.ReminderMinutesBeforeStart;
            if (!isReminderOn ||
                reminderMinutes is not { } minutes ||
                minutes < 0)
            {
                return;
            }

            writer.Write("BEGIN:VALARM");
            writer.Write("ACTION:DISPLAY");
            writer.Write(minutes == 0
                ? "TRIGGER:PT0M"
                : $"TRIGGER:-PT{minutes.ToString(CultureInfo.InvariantCulture)}M");
            writer.Write($"DESCRIPTION:{EscapeText(
                string.IsNullOrWhiteSpace(master.Subject)
                    ? "Calendar reminder"
                    : master.Subject)}");
            writer.Write("END:VALARM");
        }

        private static void WriteExDates(IcsWriter writer, EventDetails master)
        {
            List<RecurChange> deletions = master.SeriesChanges
                .Where(change => change.Kind == RecurChangeKind.Deletion)
                .OrderBy(change => change.OriginalStart)
                .ToList();
            if (deletions.Count == 0)
            {
                return;
            }

            if (master.IsAllDay)
            {
                writer.Write(
                    $"EXDATE;VALUE=DATE:{string.Join(",", deletions.Select(
                        change => change.OriginalStart.ToString("yyyyMMdd", CultureInfo.InvariantCulture)))}");
                return;
            }

            string timeZoneId = NormalizeTimeZoneId(master.StartTimeZone);
            if (IsUtc(timeZoneId))
            {
                TimeZoneInfo zone = ResolveTimeZone(timeZoneId);
                writer.Write(
                    $"EXDATE:{string.Join(",", deletions.Select(change =>
                        FormatUtc(ConvertWallTimeToUtc(change.OriginalStart, zone))))}");
            }
            else
            {
                writer.Write(
                    $"EXDATE;TZID={EncodeParameter(timeZoneId)}:{string.Join(",", deletions.Select(
                        change => change.OriginalStart.ToString(
                            "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture)))}");
            }
        }

        private static string BuildRecurrenceRule(
            EventDetails master, RecurrenceInfo recurrence)
        {
            int interval = Math.Max(recurrence.Interval, 1);
            var parts = new List<string>
            {
                recurrence.PatternType switch
                {
                    "daily" => "FREQ=DAILY",
                    "weekly" => "FREQ=WEEKLY",
                    "absoluteMonthly" or "relativeMonthly" => "FREQ=MONTHLY",
                    "absoluteYearly" or "relativeYearly" => "FREQ=YEARLY",
                    _ => throw new NotSupportedException(
                        $"The Graph recurrence pattern '{recurrence.PatternType}' is not supported.")
                },
                $"INTERVAL={interval.ToString(CultureInfo.InvariantCulture)}"
            };

            switch (recurrence.PatternType)
            {
                case "weekly":
                    AddDaysOfWeek(parts, recurrence.DaysOfWeek);
                    parts.Add($"WKST={DayCode(recurrence.FirstDayOfWeek)}");
                    break;
                case "absoluteMonthly":
                    parts.Add($"BYMONTHDAY={recurrence.DayOfMonth.ToString(CultureInfo.InvariantCulture)}");
                    break;
                case "relativeMonthly":
                    AddRelativeDays(parts, recurrence);
                    break;
                case "absoluteYearly":
                    parts.Add($"BYMONTH={recurrence.Month.ToString(CultureInfo.InvariantCulture)}");
                    parts.Add($"BYMONTHDAY={recurrence.DayOfMonth.ToString(CultureInfo.InvariantCulture)}");
                    break;
                case "relativeYearly":
                    parts.Add($"BYMONTH={recurrence.Month.ToString(CultureInfo.InvariantCulture)}");
                    AddRelativeDays(parts, recurrence);
                    break;
            }

            if (string.Equals(recurrence.RangeType, "numbered", StringComparison.OrdinalIgnoreCase) &&
                recurrence.NumberOfOccurrences > 0)
            {
                parts.Add(
                    $"COUNT={recurrence.NumberOfOccurrences.ToString(CultureInfo.InvariantCulture)}");
            }
            else if (string.Equals(
                         recurrence.RangeType, "endDate", StringComparison.OrdinalIgnoreCase) &&
                     recurrence.RangeEnd is { } rangeEnd)
            {
                if (master.IsAllDay)
                {
                    parts.Add($"UNTIL={rangeEnd:yyyyMMdd}");
                }
                else
                {
                    DateOnly finalDate =
                        RecurrenceExpander.Summarize(recurrence).End ?? rangeEnd;
                    DateTime start = ParseGraphWallTime(master.StartDateTime);
                    DateTime finalStart = finalDate.ToDateTime(TimeOnly.FromDateTime(start));
                    TimeZoneInfo zone = ResolveTimeZone(master.StartTimeZone);
                    parts.Add($"UNTIL={FormatUtc(ConvertWallTimeToUtc(finalStart, zone))}");
                }
            }

            return string.Join(";", parts);
        }

        private static void AddDaysOfWeek(List<string> parts, IReadOnlyList<DayOfWeek> days)
        {
            if (days.Count == 0)
            {
                throw new InvalidOperationException(
                    "A weekly recurrence must contain at least one day of the week.");
            }

            parts.Add($"BYDAY={string.Join(",", days.Select(DayCode))}");
        }

        private static void AddRelativeDays(List<string> parts, RecurrenceInfo recurrence)
        {
            if (recurrence.DaysOfWeek.Count == 0)
            {
                throw new InvalidOperationException(
                    "A relative recurrence must contain at least one day of the week.");
            }

            parts.Add($"BYDAY={string.Join(",", recurrence.DaysOfWeek.Select(DayCode))}");
            int index = recurrence.Index.ToLowerInvariant() switch
            {
                "first" => 1,
                "second" => 2,
                "third" => 3,
                "fourth" => 4,
                "last" => -1,
                _ => throw new NotSupportedException(
                    $"The Graph recurrence index '{recurrence.Index}' is not supported.")
            };
            parts.Add($"BYSETPOS={index.ToString(CultureInfo.InvariantCulture)}");
        }

        private static IEnumerable<string> RequiredTimeZones(EventDetails master)
        {
            if (master.IsAllDay)
            {
                return Array.Empty<string>();
            }

            return new[] { master.StartTimeZone, master.EndTimeZone }
                .Select(NormalizeTimeZoneId)
                .Where(id => !IsUtc(id))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static void WriteTimeZone(
            IcsWriter writer, string timeZoneId, EventDetails master)
        {
            TimeZoneInfo zone = ResolveTimeZone(timeZoneId);
            writer.Write("BEGIN:VTIMEZONE");
            writer.Write($"TZID:{EscapeText(timeZoneId)}");
            writer.Write($"X-LIC-LOCATION:{EscapeText(timeZoneId)}");

            (DateOnly coverageStart, DateOnly? coverageEnd) = TimeZoneCoverage(master);
            DateOnly paddedStart = coverageStart.Year > 1
                ? new DateOnly(coverageStart.Year - 1, 1, 1)
                : DateOnly.MinValue;
            DateOnly? paddedEnd = coverageEnd is { Year: < 9999 } end
                ? new DateOnly(end.Year + 1, 12, 31)
                : coverageEnd;
            List<TimeZoneInfo.AdjustmentRule> rules = zone.GetAdjustmentRules()
                .Where(rule =>
                {
                    DateOnly ruleStart = DateOnly.FromDateTime(rule.DateStart);
                    DateOnly ruleEnd = DateOnly.FromDateTime(rule.DateEnd);
                    return ruleEnd >= paddedStart &&
                           (paddedEnd is null || ruleStart <= paddedEnd.Value);
                })
                .ToList();

            if (rules.Count == 0)
            {
                string offset = FormatUtcOffset(zone.BaseUtcOffset);
                writer.Write("BEGIN:STANDARD");
                writer.Write("DTSTART:19700101T000000");
                writer.Write($"TZOFFSETFROM:{offset}");
                writer.Write($"TZOFFSETTO:{offset}");
                writer.Write($"TZNAME:{EscapeText(zone.StandardName)}");
                writer.Write("END:STANDARD");
            }
            else
            {
                foreach (TimeZoneInfo.AdjustmentRule rule in rules)
                {
                    TimeSpan standardOffset = zone.BaseUtcOffset + rule.BaseUtcOffsetDelta;
                    if (rule.DaylightDelta == TimeSpan.Zero)
                    {
                        writer.Write("BEGIN:STANDARD");
                        writer.Write($"DTSTART:{rule.DateStart:yyyyMMdd'T'000000}");
                        writer.Write($"TZOFFSETFROM:{FormatUtcOffset(standardOffset)}");
                        writer.Write($"TZOFFSETTO:{FormatUtcOffset(standardOffset)}");
                        writer.Write($"TZNAME:{EscapeText(zone.StandardName)}");
                        writer.Write("END:STANDARD");
                        continue;
                    }

                    TimeSpan daylightOffset = standardOffset + rule.DaylightDelta;
                    int transitionYear = Math.Max(paddedStart.Year, rule.DateStart.Year);
                    int? lastYear = rule.DateEnd.Year == 9999 && paddedEnd is null
                        ? null
                        : Math.Min(paddedEnd?.Year ?? rule.DateEnd.Year, rule.DateEnd.Year);
                    if (lastYear is { } last && last < transitionYear)
                    {
                        continue;
                    }

                    WriteTimeZoneObservance(
                        writer,
                        "DAYLIGHT",
                        rule.DaylightTransitionStart,
                        transitionYear,
                        lastYear,
                        standardOffset,
                        daylightOffset,
                        zone.DaylightName);
                    WriteTimeZoneObservance(
                        writer,
                        "STANDARD",
                        rule.DaylightTransitionEnd,
                        transitionYear,
                        lastYear,
                        daylightOffset,
                        standardOffset,
                        zone.StandardName);
                }
            }

            writer.Write("END:VTIMEZONE");
        }

        private static void WriteTimeZoneObservance(
            IcsWriter writer,
            string component,
            TimeZoneInfo.TransitionTime transition,
            int year,
            int? lastYear,
            TimeSpan offsetFrom,
            TimeSpan offsetTo,
            string name)
        {
            DateTime onset = TransitionDate(year, transition);
            writer.Write($"BEGIN:{component}");
            writer.Write($"DTSTART:{onset:yyyyMMdd'T'HHmmss}");
            writer.Write($"TZOFFSETFROM:{FormatUtcOffset(offsetFrom)}");
            writer.Write($"TZOFFSETTO:{FormatUtcOffset(offsetTo)}");
            writer.Write($"TZNAME:{EscapeText(name)}");
            if (lastYear is null || lastYear > year)
            {
                writer.Write($"RRULE:{TransitionRule(
                    transition, lastYear, offsetFrom)}");
            }
            writer.Write($"END:{component}");
        }

        private static string TransitionRule(
            TimeZoneInfo.TransitionTime transition, int? lastYear, TimeSpan offsetFrom)
        {
            string month = transition.Month.ToString(CultureInfo.InvariantCulture);
            string until = string.Empty;
            if (lastYear is { } year)
            {
                DateTime finalOnset = TransitionDate(year, transition);
                var finalOnsetWithOffset = new DateTimeOffset(finalOnset, offsetFrom);
                until = $";UNTIL={FormatUtc(finalOnsetWithOffset)}";
            }

            if (transition.IsFixedDateRule)
            {
                return $"FREQ=YEARLY;BYMONTH={month};BYMONTHDAY=" +
                       transition.Day.ToString(CultureInfo.InvariantCulture) + until;
            }

            int week = transition.Week == 5 ? -1 : transition.Week;
            return $"FREQ=YEARLY;BYMONTH={month};BYDAY=" +
                   $"{week.ToString(CultureInfo.InvariantCulture)}{DayCode(transition.DayOfWeek)}" +
                   until;
        }

        private static (DateOnly Start, DateOnly? End) TimeZoneCoverage(EventDetails master)
        {
            DateOnly start = master.Recurrence?.RangeStart ??
                             DateOnly.FromDateTime(ParseGraphWallTime(master.StartDateTime));
            if (master.Recurrence is not { } recurrence)
            {
                return (
                    start,
                    DateOnly.FromDateTime(ParseGraphWallTime(master.EndDateTime)));
            }

            return (start, RecurrenceExpander.Summarize(recurrence).End);
        }

        private static DateTime TransitionDate(
            int year, TimeZoneInfo.TransitionTime transition)
        {
            DateTime time = transition.TimeOfDay;
            if (transition.IsFixedDateRule)
            {
                int day = Math.Min(
                    transition.Day, DateTime.DaysInMonth(year, transition.Month));
                return new DateTime(
                    year, transition.Month, day,
                    time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
            }

            DateTime first = new(
                year, transition.Month, 1,
                time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
            int delta = ((int)transition.DayOfWeek - (int)first.DayOfWeek + 7) % 7;
            int dayOfMonth = 1 + delta + (transition.Week - 1) * 7;
            if (dayOfMonth > DateTime.DaysInMonth(year, transition.Month))
            {
                dayOfMonth -= 7;
            }

            return new DateTime(
                year, transition.Month, dayOfMonth,
                time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
        }

        private static string BuildWallTimeProperty(
            string property, DateTime wallTime, string graphTimeZone, bool isAllDay)
        {
            if (isAllDay)
            {
                return $"{property};VALUE=DATE:{wallTime:yyyyMMdd}";
            }

            string timeZoneId = NormalizeTimeZoneId(graphTimeZone);
            if (IsUtc(timeZoneId))
            {
                DateTimeOffset utc = ConvertWallTimeToUtc(
                    wallTime, ResolveTimeZone(timeZoneId));
                return $"{property}:{FormatUtc(utc)}";
            }

            return $"{property};TZID={EncodeParameter(timeZoneId)}:" +
                   wallTime.ToString("yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
        }

        private static DateTime ParseGraphWallTime(string value)
        {
            if (!DateTimeOffset.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out DateTimeOffset parsed))
            {
                throw new FormatException($"Graph returned an invalid calendar date/time: '{value}'.");
            }

            return DateTime.SpecifyKind(parsed.DateTime, DateTimeKind.Unspecified);
        }

        private static DateTimeOffset ConvertWallTimeToUtc(
            DateTime wallTime, TimeZoneInfo timeZone)
        {
            DateTime unspecified = DateTime.SpecifyKind(wallTime, DateTimeKind.Unspecified);
            if (timeZone.IsInvalidTime(unspecified))
            {
                throw new InvalidOperationException(
                    $"The calendar time {unspecified:O} does not exist in time zone '{timeZone.Id}'.");
            }

            DateTime utc = TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
            return new DateTimeOffset(utc, TimeSpan.Zero);
        }

        private static TimeZoneInfo ResolveTimeZone(string graphTimeZone)
        {
            string id = NormalizeTimeZoneId(graphTimeZone);
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException ex)
            {
                throw new InvalidOperationException(
                    $"The Graph time zone '{id}' is not installed on this computer.", ex);
            }
            catch (InvalidTimeZoneException ex)
            {
                throw new InvalidOperationException(
                    $"The Graph time zone '{id}' has invalid adjustment data.", ex);
            }
        }

        private static string NormalizeTimeZoneId(string value)
            => string.IsNullOrWhiteSpace(value) ? "UTC" : value.Trim();

        private static bool IsUtc(string timeZoneId)
            => string.Equals(timeZoneId, "UTC", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(timeZoneId, "Etc/UTC", StringComparison.OrdinalIgnoreCase);

        private static string FormatUtc(DateTimeOffset value)
            => value.ToUniversalTime().ToString(
                "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

        private static string FormatUtcOffset(TimeSpan offset)
        {
            char sign = offset < TimeSpan.Zero ? '-' : '+';
            TimeSpan absolute = offset.Duration();
            string seconds = absolute.Seconds == 0
                ? string.Empty
                : absolute.Seconds.ToString("00", CultureInfo.InvariantCulture);
            return $"{sign}{(int)absolute.TotalHours:00}{absolute.Minutes:00}{seconds}";
        }

        private static string BuildUid(EventDetails master)
        {
            string uid = FirstNonEmpty(
                master.ICalUId,
                master.CleanGlobalObjectId,
                string.IsNullOrWhiteSpace(master.Id)
                    ? string.Empty
                    : $"{master.Id}@calendar-expansion-visualizer");
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new InvalidOperationException(
                    "The calendar master does not contain an identifier suitable for UID.");
            }

            return uid;
        }

        private static string FirstNonEmpty(params string[] values)
            => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

        private static string MapParticipationStatus(string response)
            => response.ToLowerInvariant() switch
            {
                "accepted" or "organizer" => "ACCEPTED",
                "tentativelyaccepted" or "tentative" => "TENTATIVE",
                "declined" => "DECLINED",
                "none" or "notresponded" or "" => "NEEDS-ACTION",
                _ => "NEEDS-ACTION"
            };

        private static string MapClass(string sensitivity)
            => sensitivity.ToLowerInvariant() switch
            {
                "private" or "personal" => "PRIVATE",
                "confidential" => "CONFIDENTIAL",
                _ => "PUBLIC"
            };

        private static string MapMicrosoftBusyStatus(string showAs)
            => showAs.ToLowerInvariant() switch
            {
                "free" => "FREE",
                "tentative" => "TENTATIVE",
                "oof" => "OOF",
                "workingelsewhere" => "WORKINGELSEWHERE",
                _ => "BUSY"
            };

        private static string MapBusyStatusOverride(int busyStatus)
            => busyStatus switch
            {
                0 => "free",
                1 => "tentative",
                3 => "oof",
                4 => "workingElsewhere",
                _ => "busy"
            };

        private static string DayCode(DayOfWeek day)
            => day switch
            {
                DayOfWeek.Sunday => "SU",
                DayOfWeek.Monday => "MO",
                DayOfWeek.Tuesday => "TU",
                DayOfWeek.Wednesday => "WE",
                DayOfWeek.Thursday => "TH",
                DayOfWeek.Friday => "FR",
                DayOfWeek.Saturday => "SA",
                _ => throw new ArgumentOutOfRangeException(nameof(day))
            };

        private static string EscapeText(string? value)
            => (value ?? string.Empty)
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\r\n", "\\n", StringComparison.Ordinal)
                .Replace("\r", "\\n", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace(";", "\\;", StringComparison.Ordinal)
                .Replace(",", "\\,", StringComparison.Ordinal);

        private static string EncodeParameter(string value)
        {
            string encoded = value
                .Replace("^", "^^", StringComparison.Ordinal)
                .Replace("\r\n", "^n", StringComparison.Ordinal)
                .Replace("\r", "^n", StringComparison.Ordinal)
                .Replace("\n", "^n", StringComparison.Ordinal)
                .Replace("\"", "^'", StringComparison.Ordinal);
            bool quote = encoded.IndexOfAny([':', ';', ',']) >= 0;
            return quote ? $"\"{encoded}\"" : encoded;
        }

        private static bool TryGetMailto(string address, out string? mailto)
        {
            mailto = null;
            string value = address.Trim();
            if (value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                value = value[7..];
            }

            if (string.IsNullOrWhiteSpace(value) ||
                value.Contains('\r') ||
                value.Contains('\n') ||
                !Uri.TryCreate($"mailto:{value}", UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            mailto = uri.AbsoluteUri;
            return true;
        }

        private static bool TryGetAbsoluteUri(string value, out string? absoluteUri)
        {
            absoluteUri = null;
            if (string.IsNullOrWhiteSpace(value) ||
                value.Contains('\r') ||
                value.Contains('\n') ||
                !Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            absoluteUri = uri.AbsoluteUri;
            return true;
        }

        private static string HtmlToPlainText(string html)
        {
            string withLineBreaks = HtmlBreakRegex().Replace(html, "\n");
            string withoutTags = HtmlTagRegex().Replace(withLineBreaks, string.Empty);
            return WebUtility.HtmlDecode(withoutTags).Trim();
        }

        [GeneratedRegex(@"(?i)<\s*(br|/p|/div|/li)\s*/?\s*>")]
        private static partial Regex HtmlBreakRegex();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex HtmlTagRegex();

        private sealed class IcsWriter
        {
            private readonly StringBuilder _builder = new();

            public void Write(string contentLine)
            {
                int currentOctets = 0;
                foreach (Rune rune in contentLine.EnumerateRunes())
                {
                    int runeOctets = rune.Utf8SequenceLength;
                    int limit = currentOctets == 0 ? 75 : 75;
                    if (currentOctets > 0 && currentOctets + runeOctets > limit)
                    {
                        _builder.Append("\r\n ");
                        currentOctets = 1;
                    }

                    _builder.Append(rune);
                    currentOctets += runeOctets;
                }

                _builder.Append("\r\n");
            }

            public override string ToString() => _builder.ToString();
        }
    }
}
