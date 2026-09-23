using Microsoft.Identity.Client;

namespace CalendarExpansionVisualizer
{
    internal enum AuthFlow
    {
        Delegated,
        Application
    }

    /// <summary>
    /// The Microsoft national/sovereign cloud a tenant lives in. Determines both the
    /// Azure AD (Entra) login authority and the Microsoft Graph service endpoint.
    /// </summary>
    internal enum SovereignCloud
    {
        Commercial,
        Gcc,
        GccHigh,
        Dod
    }

    /// <summary>
    /// Maps a <see cref="SovereignCloud"/> to its Azure AD authority instance and
    /// Microsoft Graph service root. GCC (moderate) uses the commercial login and the
    /// commercial Graph endpoint; GCC High and DoD use the US Government login, with
    /// GCC High on graph.microsoft.us and DoD on dod-graph.microsoft.us.
    /// </summary>
    internal static class CloudEndpoints
    {
        public static AzureCloudInstance AuthorityInstance(SovereignCloud cloud) => cloud switch
        {
            SovereignCloud.GccHigh or SovereignCloud.Dod => AzureCloudInstance.AzureUsGovernment,
            _ => AzureCloudInstance.AzurePublic
        };

        /// <summary>Graph service root with no trailing slash and no version segment.</summary>
        public static string GraphRoot(SovereignCloud cloud) => cloud switch
        {
            SovereignCloud.GccHigh => "https://graph.microsoft.us",
            SovereignCloud.Dod => "https://dod-graph.microsoft.us",
            _ => "https://graph.microsoft.com"
        };
    }

    /// <summary>
    /// Acquires Microsoft Graph access tokens via MSAL for either the delegated
    /// (interactive) flow or the application (client credentials) flow, targeting the
    /// selected sovereign cloud.
    /// </summary>
    internal static class AuthService
    {
        public static async Task<AuthenticationResult> AcquireDelegatedAsync(
            string tenantId, string clientId, string redirectUri, SovereignCloud cloud,
            CancellationToken cancellationToken)
        {
            var app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(CloudEndpoints.AuthorityInstance(cloud), tenantId)
                .WithRedirectUri(string.IsNullOrWhiteSpace(redirectUri) ? "http://localhost" : redirectUri)
                .Build();

            string[] scopes = { $"{CloudEndpoints.GraphRoot(cloud)}/Calendars.Read" };

            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync(cancellationToken);
        }

        public static async Task<AuthenticationResult> AcquireApplicationAsync(
            string tenantId, string clientId, string clientSecret, SovereignCloud cloud,
            CancellationToken cancellationToken)
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(CloudEndpoints.AuthorityInstance(cloud), tenantId)
                .Build();

            string[] scopes = { $"{CloudEndpoints.GraphRoot(cloud)}/.default" };

            return await app.AcquireTokenForClient(scopes)
                .ExecuteAsync(cancellationToken);
        }
    }
}
