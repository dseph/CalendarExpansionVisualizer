using System.Text;

namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Builds a human-readable listing of a recurring series' exceptions and/or
    /// deletions from the changes parsed out of the master's recurrence blob. Shared by
    /// the main window and the Event Information dialog so the output is identical.
    /// </summary>
    internal static class RecurrenceChangeReport
    {
        public static (string Body, string Title) Build(
            string subject, IReadOnlyList<RecurChange> allChanges,
            bool wantExceptions, bool wantDeletions, bool showKindLabel)
        {
            string what = (wantExceptions, wantDeletions) switch
            {
                (true, true) => "Exceptions and Deletions",
                (true, false) => "Exceptions",
                (false, true) => "Deletions",
                _ => "Changes"
            };

            string title = $"{subject} \u2014 {what} (entire series)";

            var selected = allChanges
                .Where(c => (c.Kind == RecurChangeKind.Exception && wantExceptions) ||
                            (c.Kind == RecurChangeKind.Deletion && wantDeletions))
                // "Date range" is the new range for an exception and the original
                // occurrence range for a deletion; sort newest to oldest by it.
                .OrderByDescending(c => c.Kind == RecurChangeKind.Exception ? c.Start : c.OriginalStart)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine($"Total: {selected.Count}   (newest to oldest, whole series including outside the search range)");
            sb.AppendLine();

            if (selected.Count == 0)
            {
                sb.AppendLine($"(no {what.ToLowerInvariant()} found for this series)");
                return (sb.ToString(), title);
            }

            foreach (RecurChange c in selected)
            {
                // Date range first.
                if (c.Kind == RecurChangeKind.Exception)
                {
                    sb.AppendLine(FormatRange(c.Start, c.End));
                }
                else
                {
                    sb.AppendLine(FormatRange(c.OriginalStart, c.OriginalEnd));
                }

                // Then the type label (required when listing both kinds) and details.
                if (showKindLabel)
                {
                    sb.AppendLine(c.Kind == RecurChangeKind.Exception ? "  Exception" : "  Deletion");
                }

                if (c.Kind == RecurChangeKind.Exception)
                {
                    sb.AppendLine($"    Original occurrence : {FormatRange(c.OriginalStart, c.OriginalEnd)}");
                    if (!string.IsNullOrEmpty(c.Subject))
                    {
                        sb.AppendLine($"    Subject override    : {c.Subject}");
                    }
                    if (!string.IsNullOrEmpty(c.Location))
                    {
                        sb.AppendLine($"    Location override   : {c.Location}");
                    }
                    if (string.IsNullOrEmpty(c.Subject) && string.IsNullOrEmpty(c.Location))
                    {
                        sb.AppendLine("    (time changed; no subject/location override)");
                    }
                }
                else
                {
                    sb.AppendLine($"    Removed occurrence originally on {c.OriginalStart:yyyy-MM-dd}");
                }

                sb.AppendLine();
            }

            return (sb.ToString(), title);
        }

        public static string FormatRange(DateTime start, DateTime end)
        {
            return start.Date == end.Date
                ? $"{start:yyyy-MM-dd HH:mm} \u2013 {end:HH:mm}"
                : $"{start:yyyy-MM-dd HH:mm} \u2013 {end:yyyy-MM-dd HH:mm}";
        }
    }
}
