namespace ContentHub.Api.Contracts.Requests
{
    public record ResetPasswordRequest(string Email, string NewPassword);
}
