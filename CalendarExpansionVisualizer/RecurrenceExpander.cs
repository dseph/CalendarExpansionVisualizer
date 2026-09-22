namespace CalendarExpansionVisualizer
{
    internal enum CellState
    {
        None,
        Normal,   // occurrence present  -> green "x"
        Modified, // exception           -> yellow "M"
        Deleted   // pattern date missing -> red "D"
    }

    internal sealed class RecurrenceInfo
    {
        public string PatternType { get; set; } = string.Empty; // daily, weekly, absoluteMonthly, ...
        public int Interval { get; set; } = 1;
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public int DayOfMonth { get; set; }
        public int Month { get; set; }
        public string Index { get; set; } = "first"; // first/second/third/fourth/last
        public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

        public string RangeType { get; set; } = "noEnd"; // endDate, noEnd, numbered
        public DateOnly RangeStart { get; set; }
        public DateOnly? RangeEnd { get; set; }
        public int NumberOfOccurrences { get; set; }
    }

    internal sealed class SeriesMaster
    {
        public string Id { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public RecurrenceInfo? Recurrence { get; set; }
    }

    /// <summary>
    /// Expands a Graph recurrence pattern into the set of expected occurrence dates
    /// that fall within a window. This lets us detect deleted occurrences: a date that
    /// the pattern says should have an instance but that Graph does not return.
    /// </summary>
    internal static class RecurrenceExpander
    {
        private const int IterationCap = 500_000;

        public static List<DateOnly> Expand(RecurrenceInfo info, DateOnly windowStart, DateOnly windowEnd)
        {
            var results = new List<DateOnly>();
            if (info is null)
            {
                return results;
            }

            DateOnly last = windowEnd;
            if (info.RangeType == "endDate" && info.RangeEnd is { } re && re < last)
            {
                last = re;
            }

            DateOnly d = info.RangeStart;
            int count = 0;
            int iterations = 0;

            while (d <= last && iterations++ < IterationCap)
            {
                if (Matches(d, info))
                {
                    count++;
                    if (info.RangeType == "numbered" && count > info.NumberOfOccurrences)
                    {
                        break;
                    }

                    if (d >= windowStart && d <= windowEnd)
                    {
                        results.Add(d);
                    }
                }

                d = d.AddDays(1);
            }

            return results;
        }

        /// <summary>
        /// Summarizes a recurrence: the effective end date (if any) and the total number
        /// of occurrences (if bounded). Returns nulls for an open-ended (noEnd) pattern.
        /// </summary>
        public static (DateOnly? End, int? Total) Summarize(RecurrenceInfo info)
        {
            if (info is null)
            {
                return (null, null);
            }

            if (info.RangeType == "endDate" && info.RangeEnd is { } end)
            {
                int total = Expand(info, info.RangeStart, end).Count;
                return (end, total);
            }

            if (info.RangeType == "numbered")
            {
                DateOnly d = info.RangeStart;
                DateOnly last = info.RangeStart;
                int count = 0;
                int iterations = 0;
                while (iterations++ < IterationCap)
                {
                    if (Matches(d, info))
                    {
                        count++;
                        last = d;
                        if (count >= info.NumberOfOccurrences)
                        {
                            break;
                        }
                    }
                    d = d.AddDays(1);
                }
                return (last, info.NumberOfOccurrences);
            }

            return (null, null);
        }

        private static bool Matches(DateOnly d, RecurrenceInfo info)
        {
            if (d < info.RangeStart)
            {
                return false;
            }

            int interval = info.Interval <= 0 ? 1 : info.Interval;

            switch (info.PatternType)
            {
                case "daily":
                    return (d.DayNumber - info.RangeStart.DayNumber) % interval == 0;

                case "weekly":
                {
                    if (!info.DaysOfWeek.Contains(d.DayOfWeek))
                    {
                        return false;
                    }
                    int startWeek = WeekStart(info.RangeStart, info.FirstDayOfWeek).DayNumber;
                    int thisWeek = WeekStart(d, info.FirstDayOfWeek).DayNumber;
                    int weeks = (thisWeek - startWeek) / 7;
                    return weeks % interval == 0;
                }

                case "absoluteMonthly":
                {
                    if (d.Day != info.DayOfMonth)
                    {
                        return false;
                    }
                    int months = MonthsBetween(info.RangeStart, d);
                    return months >= 0 && months % interval == 0;
                }

                case "relativeMonthly":
                {
                    if (!info.DaysOfWeek.Contains(d.DayOfWeek) || !NthMatch(d, info))
                    {
                        return false;
                    }
                    int months = MonthsBetween(info.RangeStart, d);
                    return months >= 0 && months % interval == 0;
                }

                case "absoluteYearly":
                {
                    if (d.Month != info.Month || d.Day != info.DayOfMonth)
                    {
                        return false;
                    }
                    int years = d.Year - info.RangeStart.Year;
                    return years >= 0 && years % interval == 0;
                }

                case "relativeYearly":
                {
                    if (d.Month != info.Month || !info.DaysOfWeek.Contains(d.DayOfWeek) || !NthMatch(d, info))
                    {
                        return false;
                    }
                    int years = d.Year - info.RangeStart.Year;
                    return years >= 0 && years % interval == 0;
                }

                default:
                    return false;
            }
        }

        private static int MonthsBetween(DateOnly from, DateOnly to)
            => (to.Year - from.Year) * 12 + (to.Month - from.Month);

        private static DateOnly WeekStart(DateOnly d, DayOfWeek firstDay)
        {
            int diff = ((int)d.DayOfWeek - (int)firstDay + 7) % 7;
            return d.AddDays(-diff);
        }

        private static bool NthMatch(DateOnly d, RecurrenceInfo info)
        {
            int daysInMonth = DateTime.DaysInMonth(d.Year, d.Month);

            int ordinalUpToD = 0;
            for (int day = 1; day <= d.Day; day++)
            {
                var candidate = new DateOnly(d.Year, d.Month, day);
                if (info.DaysOfWeek.Contains(candidate.DayOfWeek))
                {
                    ordinalUpToD++;
                }
            }

            if (!info.DaysOfWeek.Contains(d.DayOfWeek))
            {
                return false;
            }

            if (string.Equals(info.Index, "last", StringComparison.OrdinalIgnoreCase))
            {
                int totalInMonth = 0;
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var candidate = new DateOnly(d.Year, d.Month, day);
                    if (info.DaysOfWeek.Contains(candidate.DayOfWeek))
                    {
                        totalInMonth++;
                    }
                }
                return ordinalUpToD == totalInMonth;
            }

            int wanted = info.Index.ToLowerInvariant() switch
            {
                "first" => 1,
                "second" => 2,
                "third" => 3,
                "fourth" => 4,
                _ => 1
            };

            return ordinalUpToD == wanted;
        }
    }
}
