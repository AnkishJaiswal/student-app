using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace student_app.Auth
{
    public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";

        public string HeaderName { get; set; } = "X-API-Key";

        public string? ApiKey { get; set; }
    }

    public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(Options.ApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API key authentication is not configured."));
            }

            if (!Request.Headers.TryGetValue(Options.HeaderName, out var providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API key is missing."));
            }

            if (providedApiKey.Count != 1 || !IsValidApiKey(providedApiKey.ToString(), Options.ApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API key is invalid."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "api-key-client"),
                new Claim(ClaimTypes.Name, "API Key Client")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.Headers.WWWAuthenticate = ApiKeyAuthenticationOptions.DefaultScheme;

            return Response.WriteAsJsonAsync(new { message = "A valid API key is required." });
        }

        private static bool IsValidApiKey(string providedApiKey, string configuredApiKey)
        {
            var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);
            var configuredBytes = Encoding.UTF8.GetBytes(configuredApiKey);

            return providedBytes.Length == configuredBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
        }
    }
}
