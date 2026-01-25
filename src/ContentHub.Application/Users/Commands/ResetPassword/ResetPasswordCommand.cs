using System;

namespace ContentHub.Application.Users.Commands.ResetPassword
{
    public record ResetPasswordCommand(string Email, string NewPassword);
}
