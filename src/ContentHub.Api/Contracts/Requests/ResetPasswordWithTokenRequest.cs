namespace ContentHub.Api.Contracts.Requests
{
    public record ResetPasswordWithTokenRequest(string Token, string NewPassword);
}
