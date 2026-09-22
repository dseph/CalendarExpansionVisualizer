using System.Net.Http.Headers;
using System.Text;

namespace CalendarExpansionVisualizer
{
    /// <summary>
    /// Logs every Graph HTTP request and response (headers + bodies) to a log file.
    /// The OAuth bearer token in the Authorization header is always redacted.
    /// </summary>
    internal sealed class GraphLoggingHandler : DelegatingHandler
    {
        private static readonly SemaphoreSlim FileLock = new(1, 1);
        private readonly string _logPath;

        public GraphLoggingHandler(string logPath, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _logPath = logPath;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================================================================");
            sb.AppendLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] REQUEST");
            sb.AppendLine($"{request.Method} {request.RequestUri}");
            sb.AppendLine("-- Request Headers --");
            AppendHeaders(sb, request.Headers);
            if (request.Content is not null)
            {
                AppendHeaders(sb, request.Content.Headers);
                string reqBody = await request.Content.ReadAsStringAsync(cancellationToken);
                sb.AppendLine("-- Request Body --");
                sb.AppendLine(string.IsNullOrEmpty(reqBody) ? "(empty)" : reqBody);
            }

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                sb.AppendLine("-- Request FAILED --");
                sb.AppendLine(ex.ToString());
                await WriteAsync(sb.ToString());
                throw;
            }

            sb.AppendLine();
            sb.AppendLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz}] RESPONSE");
            sb.AppendLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            sb.AppendLine("-- Response Headers --");
            AppendHeaders(sb, response.Headers);
            if (response.Content is not null)
            {
                AppendHeaders(sb, response.Content.Headers);
                string respBody = await response.Content.ReadAsStringAsync(cancellationToken);
                sb.AppendLine("-- Response Body --");
                sb.AppendLine(string.IsNullOrEmpty(respBody) ? "(empty)" : respBody);
            }
            sb.AppendLine();

            await WriteAsync(sb.ToString());
            return response;
        }

        private static void AppendHeaders(StringBuilder sb, HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{header.Key}: [REDACTED]");
                    continue;
                }

                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        private async Task WriteAsync(string text)
        {
            await FileLock.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_logPath, text, Encoding.UTF8);
            }
            finally
            {
                FileLock.Release();
            }
        }
    }
}
