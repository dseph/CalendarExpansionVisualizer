# Calendar Expansion Visualizer User Guide

## Purpose

Calendar Expansion Visualizer is a Windows desktop application for inspecting Microsoft Graph calendar data. It can:

- Fetch expanded calendar instances for a date range.
- Check recurring series against the instances returned by Graph.
- Identify modified and deleted recurrence occurrences.
- Inspect calendar-master metadata, recurrence ranges, storage size, attachments, attendees, and identifiers.
- Run focused Microsoft Graph API tests.
- Generate an RFC 5545 iCalendar (`.ics`) representation from the calendar master record.

The application targets .NET 10 for Windows and uses Microsoft Graph through MSAL authentication.

## Security requirements

### Microsoft Graph permissions

The delegated sign-in flow requests the `Calendars.Read` scope. The application flow uses the tenant application's `.default` permissions and therefore requires the appropriate application permission and administrator consent in Entra ID.

Use the least privilege necessary:

- Delegated flow: grant only `Calendars.Read` unless the tenant's policy requires otherwise.
- Application flow: grant only the equivalent read-only calendar application permission and restrict who may use the client credentials.
- Do not share client secrets, access tokens, mailbox UPNs, or exported calendar data.

### Authentication modes

The Authentication group supports:

- **Delegated (user)**: interactive sign-in as the current user. The signed-in user is queried through `/me`.
- **Application (app-only)**: client credentials using tenant ID, client ID, client secret, and mailbox UPN. The mailbox is queried through `/users/{UPN}`.
- **Microsoft App ID**: uses Microsoft's public Graph PowerShell client ID for interactive delegated sign-in. This is an application identifier, not a secret. Disable this option when organizational policy requires a separately registered application.

Select the correct cloud before signing in:

- **Commercial**: `graph.microsoft.com`.
- **GCC**: commercial Graph endpoint and public-cloud authority.
- **GCC High**: `graph.microsoft.us` and US Government authority.
- **DoD**: `dod-graph.microsoft.us` and US Government authority.

Do not use a commercial endpoint for a GCC High or DoD mailbox.

### Logging and privacy

Logging is optional and is controlled by **Enable logging**. When enabled, the application writes Graph request and response headers and bodies to the configured log file. The default path is under the current user's Documents folder.

Logs can contain sensitive mailbox data, including:

- Meeting subjects, bodies, attendee addresses, locations, and private links.
- Recurrence data and attachment metadata.
- Mailbox identifiers and request URLs.
- Error details.

The OAuth `Authorization` header is redacted, but the log is not a sanitized audit log. Treat it as confidential, protect its file permissions, avoid enabling it on shared machines, and delete it when troubleshooting is complete. Do not send logs outside the organization without reviewing and redacting them.

### Operational security

- Use HTTPS Graph endpoints only.
- Protect the client secret using the organization's secret-management process; the application does not persist it in source code.
- Avoid exporting or copying generated ICS files containing private meeting information to uncontrolled locations.
- Review the selected mailbox and date range before running a query.
- The application reads calendar data but does not intentionally modify calendar items.

## Main window

### Authentication group

1. Choose **Delegated (user)** or **Application (app-only)**.
2. Choose the Microsoft Graph cloud.
3. Enter the tenant and client information required by the selected flow.
4. For application flow, enter the mailbox UPN.
5. Click **Get Token**.

After successful authentication, **Fetch**, **Check**, and **Tests** become available. If authentication fails, the token is cleared and those operations are disabled.

### Calendar Query group

- **Start Date** and **End Date** define the query window.
- The date range cannot exceed one month.
- **Fetch** displays the expanded instances returned by Graph.
- **Check** analyzes recurring series and compares expected recurrence dates with returned instances.
- **Log File** selects the optional diagnostic log path.
- **Browse...** selects a log file path.
- **Enable logging** enables the sensitive request/response logging described above.
- **Tests** opens the Graph API test window.

The application uses local wall-clock values for the selected date range so recurrence dates remain aligned with the displayed calendar dates.

## Fetch view

Click **Fetch** to call Microsoft Graph `calendarView`. The grid displays expanded event instances in the selected period, including subject, start, end, organizer, location, all-day status, recurrence status, and Graph event type.

For a recurring instance, the application retains its series-master ID for later inspection and ICS generation. A right-click action therefore resolves the selected row to the master before generating output.

Double-clicking is primarily supported by the Check view; use Check when you need recurrence analysis and the richer event-information workflow.

## Check view and calendar flags

Click **Check** to:

1. Retrieve calendar instances for the selected period.
2. Retrieve recurring series masters.
3. Expand each master recurrence pattern.
4. Compare expected dates with actual instances.
5. Mark modified and missing occurrences.
6. Show per-window and whole-series counts.

The Check grid contains a **Subject** column and one column per date in the selected window. The date cells use the following text and colors:

| Text | Background | Meaning |
|---|---|---|
| `X` | Lime green | A normal occurrence was returned by Graph for an expected recurrence date. |
| `M` | Gold | A modified/exception occurrence was returned. The occurrence differs from the series pattern or has exception data. |
| `D` | Firebrick with white text | The recurrence pattern expects an occurrence, but Graph did not return one; it is treated as a deletion. |
| blank | Gainsboro | No occurrence is expected on that date, or no matching state was found. |

The Subject text color provides additional master-level warnings:

- **Black**: ordinary size and recurrence span.
- **Dark orange**: the recurrence spans at least one year but not more than two years.
- **Red**: an open-ended or more-than-two-year recurrence, a calendar master at least 50 MB, or more than 30 exceptions.

These colors are warnings for investigation, not proof that an item is corrupt. Large masters and long or highly edited series can require more Graph and Outlook processing.

### Selecting a row

- Click a row to load a cached or calculated whole-series status in the status bar.
- Double-click a row to open **Event Information**.
- Right-click a row and choose **Show Generated ICS** to generate an iCalendar export from the calendar master.

The right-click action is available only when the selected row has a usable event/master ID. Recurrence instances are resolved to their series master before the details are loaded.

## Event Information window

Double-clicking a Check row opens a read-only report containing:

- **Subject**.
- **Time**: start and end values returned by Graph.
- **Identifiers**: Graph event ID, `iCalUId`, and Clean Global Object ID when available.
- **Organizer** and attendee list, including attendee type and response.
- **Storage**: calendar-record size and attachment count/size.
- **Recurrence**: pattern, interval, days, month/day rules, index, range type, range dates, and calculated occurrence totals.
- **Instance counts** within the selected date range.
- **Whole-series counts** from the Outlook recurrence blob when available.
- **Attachment totals** for the selected range and, where available, the whole series.

The warning panel is shown only when warnings exist. Warning colors and reasons include:

| Color | Condition |
|---|---|
| Red | Calendar master is at least 50 MB; recurrence has no end; recurrence exceeds one year; or series has at least 30 exceptions/deletions. |
| Orange | Calendar master is at least 25 MB; or series has at least 20 exceptions/deletions. |
| Cyan | Calendar master is at least 10 MB; or series has at least 10 exceptions/deletions. |

If the recurrence data or attachment data cannot be determined, the report says so rather than treating the value as known.

If recurrence changes were parsed, **List Recurrence Changes** opens a second report window.

## Recurrence Changes window

This window lists changes across the entire recurring series, not only the selected date window.

Choose one of:

- **Exceptions**: modified occurrences.
- **Deletions**: removed occurrences.
- **List Exceptions and Deletions**: both categories, with a kind label.

Click **List** to render the report. Exception entries can include original and new times, overridden subject/location values, and attachment differences when available. Deletion entries show the original scheduled occurrence that was removed.

## Graph API Tests window

Click **Tests** from the main window to open **Graph API Tests**. The test window uses the authenticated context from the main window and does not acquire a second token.

Select one test at a time:

| Test | What it calls | Inputs used |
|---|---|---|
| **GetSchedule** | Microsoft Graph `calendar/getSchedule` | Start, End, availability interval, and one or more schedule UPNs. |
| **CalendarView** | Raw `calendarView` request | Start and End. |
| **Search subject in time range** | Searches events with an exact subject and date range | Start, End, and Subject. |
| **Search all events for subject** | Searches the mailbox for events with an exact subject | Subject. |
| **Event instances in range** | Lists `/events/{id}/instances` | Start, End, and Event ID. |
| **CalendarView delta** | Calls `calendarView/delta` | Start and End. |

The form enables only the inputs relevant to the selected test. Invalid or missing subject, schedule, or event ID values are reported as skipped test results. The date End must be after Start.

Use **Show results in text box** to control whether formatted results are displayed. HTTP status lines are shown with response bodies; JSON bodies are pretty-printed and colorized. When results are hidden, the full request/response detail remains available in the configured log only if logging is enabled.

The test window displays start, end, and elapsed timestamps for the test run. These are diagnostic timestamps and do not change calendar data.

## Generated ICS window

In a Fetch or Check grid, right-click a row and select **Show Generated ICS**.

The application first loads the selected record's type and `seriesMasterId`. If the row is an occurrence or exception, it follows the master ID and loads the series master. It refuses to generate recurring ICS from an instance.

The generated output is an RFC 5545-style iCalendar object containing, when available:

- `VCALENDAR` and `VEVENT` structure.
- Stable UID, timestamps, subject, description, location, organizer, attendees, categories, status, and transparency.
- All-day `VALUE=DATE` times or timezone-aware timed values.
- `RRULE` recurrence data.
- `EXDATE` values for deleted occurrences.
- Additional `VEVENT` records with `RECURRENCE-ID` for modified occurrences.
- `VTIMEZONE` data for non-UTC recurring meetings, including daylight-saving transitions.
- `VALARM` for enabled Graph reminders.
- RFC text escaping, CRLF line endings, and UTF-8-aware 75-octet line folding.

In the viewer:

- `BEGIN:VEVENT` and `END:VEVENT` lines have a light green background.
- `UID:` lines have a light yellow background.
- **Copy** copies the complete ICS text.
- **Save As...** writes UTF-8 ICS content to a `.ics` file.

The generated ICS is a representation of the master and its parsed recurrence changes. It is not a write-back operation and does not update Graph or Outlook.

## Recommended workflow

1. Select the correct sovereign cloud and authentication flow.
2. Acquire a token with read-only calendar permissions.
3. Leave logging disabled unless diagnostics are needed.
4. Choose a date range of no more than one month.
5. Run **Fetch** for a straightforward expanded-instance view.
6. Run **Check** to identify expected, modified, and missing occurrences.
7. Review the row colors, subject colors, warning panel, and whole-series counts.
8. Use **List Recurrence Changes** for detailed exception/deletion inspection.
9. Use **Show Generated ICS** only when a master-based export is needed.
10. Disable logging and protect or delete diagnostic logs after troubleshooting.

## Limitations and interpretation notes

- Graph permissions, tenant policy, throttling, mailbox size, and server-side data determine what can be retrieved.
- Whole-series counts depend on the Outlook `PidLidAppointmentRecur` data being available and parseable.
- Attachment summaries may be unavailable for some items or scans.
- A missing instance is flagged as a deletion only when the recurrence pattern predicts that date and no matching returned instance exists.
- Large, long-running, or heavily modified series are flagged because they deserve additional review; the flags do not by themselves indicate data loss.
- The application is read-only with respect to calendar items, but viewing, logging, copying, and exporting calendar information can still disclose sensitive data.
