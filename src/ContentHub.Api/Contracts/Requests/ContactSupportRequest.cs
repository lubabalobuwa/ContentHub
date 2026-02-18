namespace ContentHub.Api.Contracts.Requests
{
    public record ContactSupportRequest(
        string Name,
        string Email,
        string Topic,
        string Message,
        string TurnstileToken);
}
