using Microsoft.Identity.Client;

namespace CalendarExpansionVisualizer
{
    internal enum AuthFlow
    {
        Delegated,
        Application
    }

    /// <summary>
    /// Acquires Microsoft Graph access tokens via MSAL for either the delegated
    /// (interactive) flow or the application (client credentials) flow.
    /// </summary>
    internal static class AuthService
    {
        private const string GraphBase = "https://graph.microsoft.com";

        public static async Task<AuthenticationResult> AcquireDelegatedAsync(
            string tenantId, string clientId, string redirectUri, CancellationToken cancellationToken)
        {
            var app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithRedirectUri(string.IsNullOrWhiteSpace(redirectUri) ? "http://localhost" : redirectUri)
                .Build();

            string[] scopes = { $"{GraphBase}/Calendars.Read" };

            return await app.AcquireTokenInteractive(scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync(cancellationToken);
        }

        public static async Task<AuthenticationResult> AcquireApplicationAsync(
            string tenantId, string clientId, string clientSecret, CancellationToken cancellationToken)
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .Build();

            string[] scopes = { $"{GraphBase}/.default" };

            return await app.AcquireTokenForClient(scopes)
                .ExecuteAsync(cancellationToken);
        }
    }
}
