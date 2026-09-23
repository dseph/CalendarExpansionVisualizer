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

                    AppendAttachmentChanges(sb, c);
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

        private static void AppendAttachmentChanges(StringBuilder sb, RecurChange c)
        {
            if (!c.AttachmentInfoKnown)
            {
                sb.AppendLine("    Attachment changes  : (not available)");
                return;
            }

            if (c.AttachmentChanges.Count == 0)
            {
                sb.AppendLine("    Attachment changes  : none");
                return;
            }

            sb.AppendLine("    Attachment changes  :");
            foreach (AttachmentChange a in c.AttachmentChanges
                         .OrderBy(a => a.Kind)
                         .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
            {
                string sign = a.Kind == AttachmentChangeKind.Added ? "+" : "-";
                string name = string.IsNullOrEmpty(a.Name) ? "(unnamed)" : a.Name;
                sb.AppendLine($"      {sign} {name} ({FormatSize(a.Size)})");
            }
        }

        private static string FormatSize(long? bytes)
        {
            if (bytes is not { } b)
            {
                return "unknown size";
            }

            string[] units = { "bytes", "KB", "MB", "GB" };
            double size = b;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return unit == 0 ? $"{b} {units[unit]}" : $"{size:0.##} {units[unit]}";
        }
    }
}
