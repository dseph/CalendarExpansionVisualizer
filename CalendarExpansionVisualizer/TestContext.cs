namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// The authentication/mailbox context captured on <see cref="frmMain"/> and passed to
    /// <see cref="frmTests"/>. All Graph calls made by the test harness flow through
    /// <see cref="GraphCalendarClient"/>, so they are written to the same log file
    /// (with the OAuth token redacted) when logging is enabled.
    /// </summary>
    internal sealed class TestContext
    {
        public required string AccessToken { get; init; }
        public required AuthFlow Flow { get; init; }
        public required string GraphRoot { get; init; }
        public string? Upn { get; init; }
        public required string LogPath { get; init; }
        public bool LogEnabled { get; init; }
        public string CloudName { get; init; } = string.Empty;
    }
}
