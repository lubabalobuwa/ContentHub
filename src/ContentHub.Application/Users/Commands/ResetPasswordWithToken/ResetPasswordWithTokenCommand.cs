namespace ContentHub.Application.Users.Commands.ResetPasswordWithToken
{
    public record ResetPasswordWithTokenCommand(string Token, string NewPassword);
}
