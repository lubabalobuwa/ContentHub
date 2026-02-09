using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ContentHub.Api.Services
{
    public class TurnstileVerifier
    {
        private readonly HttpClient _httpClient;
        private readonly TurnstileSettings _settings;

        public TurnstileVerifier(HttpClient httpClient, IOptions<TurnstileSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<TurnstileVerificationResult> VerifyAsync(string token, string? remoteIp)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
                return TurnstileVerificationResult.Failure("Captcha is not configured.");

            if (string.IsNullOrWhiteSpace(token))
                return TurnstileVerificationResult.Failure("Captcha token is required.");

            var payload = new Dictionary<string, string>
            {
                ["secret"] = _settings.SecretKey,
                ["response"] = token
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
                payload["remoteip"] = remoteIp;

            var response = await _httpClient.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                new FormUrlEncodedContent(payload));

            if (!response.IsSuccessStatusCode)
                return TurnstileVerificationResult.Failure("Captcha verification failed.");

            var body = await response.Content.ReadFromJsonAsync<TurnstileVerificationResponse>();
            if (body is null || !body.Success)
                return TurnstileVerificationResult.Failure("Captcha verification failed.");

            return TurnstileVerificationResult.Success();
        }
    }

    public record TurnstileVerificationResult(bool IsSuccess, string Error)
    {
        public static TurnstileVerificationResult Success() => new(true, string.Empty);
        public static TurnstileVerificationResult Failure(string error) => new(false, error);
    }

    public record TurnstileVerificationResponse(
        bool Success,
        string[]? ErrorCodes);
}
