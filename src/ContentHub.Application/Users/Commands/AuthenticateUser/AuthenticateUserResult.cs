using System;

namespace ContentHub.Application.Users.Commands.AuthenticateUser
{
    public record AuthenticateUserResult(Guid UserId, string Token, string Role);
}
