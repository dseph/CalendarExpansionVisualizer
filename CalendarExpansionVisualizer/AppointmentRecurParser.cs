namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Whole-series counts read from a series master's PidLidAppointmentRecur blob.
    /// Exceptions is the ExceptionCount field, Deletions is the DeletedInstanceCount
    /// field, and Instances is the pattern OccurrenceCount (null for an open-ended
    /// series where no bounded total is stored).
    /// </summary>
    internal readonly record struct SeriesCounts(int? Instances, int Exceptions, int Deletions, bool Endless);

    /// <summary>
    /// Distinguishes a modified occurrence (Exception) from a removed one (Deletion).
    /// </summary>
    internal enum RecurChangeKind
    {
        Exception,
        Deletion
    }

    /// <summary>
    /// One changed occurrence of a recurring series, read from the master's
    /// PidLidAppointmentRecur blob. <see cref="OriginalStart"/>/<see cref="OriginalEnd"/>
    /// give the occurrence's original date range; for an exception <see cref="Start"/>/
    /// <see cref="End"/> give the new (modified) date range, and <see cref="Subject"/>/
    /// <see cref="Location"/> carry any overridden values.
    /// </summary>
    internal sealed class RecurChange
    {
        public RecurChangeKind Kind { get; set; }
        public DateTime OriginalStart { get; set; }
        public DateTime OriginalEnd { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Subject { get; set; }
        public string? Location { get; set; }
    }

    /// <summary>
    /// Parses a PidLidAppointmentRecur binary blob (MS-OXOCAL 2.2.1.44) to obtain the
    /// OccurrenceCount and DeletedInstanceCount fields from the RecurrencePattern and the
    /// ExceptionCount field from the AppointmentRecurrencePattern, giving whole-series
    /// counts without paging individual instances.
    /// </summary>
    internal static class AppointmentRecurParser
    {
        private const uint EndAfterDate = 0x00002021;
        private const uint EndAfterNOccurrences = 0x00002022;
        private const uint NeverEnd = 0x00002023;
        private const uint NeverEndAlt = 0xFFFFFFFF;

        public static bool TryParse(byte[] b, out SeriesCounts counts)
        {
            counts = default;
            if (b is null || b.Length < 22)
            {
                return false;
            }

            try
            {
                int o = 0;

                ushort ReadU16()
                {
                    ushort v = (ushort)(b[o] | (b[o + 1] << 8));
                    o += 2;
                    return v;
                }

                uint ReadU32()
                {
                    uint v = (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));
                    o += 4;
                    return v;
                }

                ushort readerVersion = ReadU16(); // ReaderVersion (expected 0x3004)
                ushort writerVersion = ReadU16(); // WriterVersion (expected 0x3004)
                _ = ReadU16();                    // RecurFrequency
                ushort patternType = ReadU16();
                _ = ReadU16();                    // CalendarType
                _ = ReadU32();                    // FirstDateTime
                _ = ReadU32();                    // Period
                _ = ReadU32();                    // SlidingFlag

                // Sanity check: reject anything that is not a RecurrencePattern blob.
                if (readerVersion != 0x3004 || writerVersion != 0x3004)
                {
                    return false;
                }

                // PatternTypeSpecific length depends on the pattern type.
                int patternSpecific = patternType switch
                {
                    0x0000 => 0, // Day
                    0x0001 => 4, // Week
                    0x0002 => 4, // Month
                    0x0003 => 8, // MonthNth
                    0x0004 => 4, // MonthEnd
                    0x000A => 4, // HijriMonth
                    0x000B => 8, // HijriMonthNth
                    0x000C => 4, // HijriMonthEnd
                    _ => 4
                };
                o += patternSpecific;

                if (o + 16 > b.Length)
                {
                    return false;
                }

                uint endType = ReadU32();
                uint occurrenceCount = ReadU32();
                _ = ReadU32();                 // FirstDOW

                // DeletedInstanceCount + its date array.
                uint deletedCount = ReadU32();
                long afterDeleted = (long)o + (long)deletedCount * 4;
                if (afterDeleted + 4 > b.Length)
                {
                    return false;
                }
                o = (int)afterDeleted;

                // ModifiedInstanceCount + its date array.
                uint modifiedCount = ReadU32();
                long afterModified = (long)o + (long)modifiedCount * 4;

                // ExceptionCount lives in the AppointmentRecurrencePattern that follows the
                // RecurrencePattern: after ModifiedInstanceDates come StartDate (4) and
                // EndDate (4), then ReaderVersion2 (4), WriterVersion2 (4), StartTimeOffset
                // (4), EndTimeOffset (4), then ExceptionCount (2). If the blob is truncated,
                // fall back to ModifiedInstanceCount (which equals ExceptionCount).
                int exceptionCount = (int)modifiedCount;
                long exceptionCountOffset = afterModified + 4 /*StartDate*/ + 4 /*EndDate*/ + 16 /*version2 + offsets*/;
                if (exceptionCountOffset >= 0 && exceptionCountOffset + 2 <= b.Length)
                {
                    o = (int)exceptionCountOffset;
                    exceptionCount = ReadU16();
                }

                bool endless = endType == NeverEnd || endType == NeverEndAlt;

                // Report the raw fields requested: ExceptionCount and DeletedInstanceCount.
                int exceptions = exceptionCount;
                int deletions = (int)deletedCount;

                // Total occurrences from the pattern (placeholder/meaningless for a
                // never-ending series, so report it as unbounded in that case).
                int? instances = endless ? (int?)null : (int)occurrenceCount;

                counts = new SeriesCounts(instances, exceptions, deletions, endless);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Override flags in the ExceptionInfo structure (MS-OXOCAL 2.2.1.44.2).
        private const ushort ARO_SUBJECT = 0x0001;
        private const ushort ARO_MEETINGTYPE = 0x0002;
        private const ushort ARO_REMINDERDELTA = 0x0004;
        private const ushort ARO_REMINDER = 0x0008;
        private const ushort ARO_LOCATION = 0x0010;
        private const ushort ARO_BUSYSTATUS = 0x0020;
        private const ushort ARO_ATTACHMENT = 0x0040;
        private const ushort ARO_SUBTYPE = 0x0080;
        private const ushort ARO_APPTCOLOR = 0x0100;

        // Dates/times in the blob are minutes since midnight (UTC-agnostic wall clock)
        // 1 January 1601 in the recurrence's own time zone.
        private static readonly DateTime OutlookEpoch =
            new(1601, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        private static DateTime FromMinutes(uint minutes) => OutlookEpoch.AddMinutes(minutes);

        /// <summary>
        /// Parses every changed occurrence (all exceptions and all deletions) for the
        /// entire series from a PidLidAppointmentRecur blob, regardless of any date
        /// window. Deletions are the removed occurrences (DeletedInstanceDates that are
        /// not also modified); exceptions are the modified occurrences, enriched with the
        /// new times and overridden subject/location from the ExceptionInfo array when
        /// available.
        /// </summary>
        public static bool TryParseDetails(byte[] b, out List<RecurChange> changes)
        {
            changes = new List<RecurChange>();
            if (b is null || b.Length < 22)
            {
                return false;
            }

            try
            {
                int o = 0;

                void Need(int n)
                {
                    if (o + n > b.Length)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }

                ushort ReadU16()
                {
                    Need(2);
                    ushort v = (ushort)(b[o] | (b[o + 1] << 8));
                    o += 2;
                    return v;
                }

                uint ReadU32()
                {
                    Need(4);
                    uint v = (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));
                    o += 4;
                    return v;
                }

                string ReadAnsi(int count)
                {
                    Need(count);
                    string s = System.Text.Encoding.Latin1.GetString(b, o, count);
                    o += count;
                    return s;
                }

                ushort readerVersion = ReadU16();
                ushort writerVersion = ReadU16();
                _ = ReadU16();                    // RecurFrequency
                ushort patternType = ReadU16();
                _ = ReadU16();                    // CalendarType
                _ = ReadU32();                    // FirstDateTime
                _ = ReadU32();                    // Period
                _ = ReadU32();                    // SlidingFlag

                if (readerVersion != 0x3004 || writerVersion != 0x3004)
                {
                    return false;
                }

                int patternSpecific = patternType switch
                {
                    0x0000 => 0,
                    0x0001 => 4,
                    0x0002 => 4,
                    0x0003 => 8,
                    0x0004 => 4,
                    0x000A => 4,
                    0x000B => 8,
                    0x000C => 4,
                    _ => 4
                };
                o += patternSpecific;

                _ = ReadU32();                    // EndType
                _ = ReadU32();                    // OccurrenceCount
                _ = ReadU32();                    // FirstDOW

                uint deletedCount = ReadU32();
                var deletedDates = new uint[deletedCount];
                for (int i = 0; i < deletedCount; i++)
                {
                    deletedDates[i] = ReadU32();
                }

                uint modifiedCount = ReadU32();
                var modifiedDates = new uint[modifiedCount];
                for (int i = 0; i < modifiedCount; i++)
                {
                    modifiedDates[i] = ReadU32();
                }

                _ = ReadU32();                    // StartDate (series)
                _ = ReadU32();                    // EndDate (series)

                _ = ReadU32();                    // ReaderVersion2
                _ = ReadU32();                    // WriterVersion2
                uint startTimeOffset = ReadU32(); // minutes into the day of an occurrence's start
                uint endTimeOffset = ReadU32();   // minutes into the day of an occurrence's end

                var modifiedSet = new HashSet<uint>(modifiedDates);

                // Deletions: removed occurrences (present in DeletedInstanceDates but not
                // modified). Their times come from the pattern's default offsets.
                foreach (uint d in deletedDates)
                {
                    if (modifiedSet.Contains(d))
                    {
                        continue;
                    }

                    DateTime origStart = FromMinutes(d).AddMinutes(startTimeOffset);
                    DateTime origEnd = FromMinutes(d).AddMinutes(endTimeOffset);
                    changes.Add(new RecurChange
                    {
                        Kind = RecurChangeKind.Deletion,
                        OriginalStart = origStart,
                        OriginalEnd = origEnd,
                        Start = origStart,
                        End = origEnd
                    });
                }

                // Exceptions: modified occurrences. Try to read the ExceptionInfo array for
                // the new times and overridden subject/location; fall back to the modified
                // dates (with default times) if the variable-length data can't be parsed.
                var exceptions = new List<RecurChange>();
                bool exceptionsParsed = false;
                try
                {
                    ushort exceptionCount = ReadU16();
                    for (int i = 0; i < exceptionCount; i++)
                    {
                        uint startDateTime = ReadU32();
                        uint endDateTime = ReadU32();
                        uint originalStartDate = ReadU32();
                        ushort flags = ReadU16();

                        string? subject = null;
                        string? location = null;

                        if ((flags & ARO_SUBJECT) != 0)
                        {
                            _ = ReadU16();               // SubjectLength (chars + 1)
                            ushort len2 = ReadU16();     // SubjectLength2 (chars)
                            subject = ReadAnsi(len2);
                        }
                        if ((flags & ARO_MEETINGTYPE) != 0) _ = ReadU32();
                        if ((flags & ARO_REMINDERDELTA) != 0) _ = ReadU32();
                        if ((flags & ARO_REMINDER) != 0) _ = ReadU32();
                        if ((flags & ARO_LOCATION) != 0)
                        {
                            _ = ReadU16();               // LocationLength (chars + 1)
                            ushort len2 = ReadU16();     // LocationLength2 (chars)
                            location = ReadAnsi(len2);
                        }
                        if ((flags & ARO_BUSYSTATUS) != 0) _ = ReadU32();
                        if ((flags & ARO_ATTACHMENT) != 0) _ = ReadU32();
                        if ((flags & ARO_SUBTYPE) != 0) _ = ReadU32();
                        if ((flags & ARO_APPTCOLOR) != 0) _ = ReadU32();

                        DateTime origStart = FromMinutes(originalStartDate).AddMinutes(startTimeOffset);
                        DateTime origEnd = FromMinutes(originalStartDate).AddMinutes(endTimeOffset);
                        exceptions.Add(new RecurChange
                        {
                            Kind = RecurChangeKind.Exception,
                            OriginalStart = origStart,
                            OriginalEnd = origEnd,
                            Start = FromMinutes(startDateTime),
                            End = FromMinutes(endDateTime),
                            Subject = string.IsNullOrEmpty(subject) ? null : subject,
                            Location = string.IsNullOrEmpty(location) ? null : location
                        });
                    }

                    exceptionsParsed = true;
                }
                catch
                {
                    // Fall through to the modified-dates fallback below.
                }

                if (!exceptionsParsed)
                {
                    exceptions.Clear();
                    foreach (uint m in modifiedDates)
                    {
                        DateTime origStart = FromMinutes(m).AddMinutes(startTimeOffset);
                        DateTime origEnd = FromMinutes(m).AddMinutes(endTimeOffset);
                        exceptions.Add(new RecurChange
                        {
                            Kind = RecurChangeKind.Exception,
                            OriginalStart = origStart,
                            OriginalEnd = origEnd,
                            Start = origStart,
                            End = origEnd
                        });
                    }
                }

                changes.AddRange(exceptions);
                return true;
            }
            catch
            {
                return changes.Count > 0;
            }
        }
    }
}
