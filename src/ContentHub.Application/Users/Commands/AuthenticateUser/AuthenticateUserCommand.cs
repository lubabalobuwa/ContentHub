using System;

namespace ContentHub.Application.Users.Commands.AuthenticateUser
{
    public record AuthenticateUserCommand(string Email, string Password);
}
