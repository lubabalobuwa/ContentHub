using System;

namespace ContentHub.Application.Users.Commands.RefreshToken
{
    public record RefreshTokenResult(Guid UserId, string AccessToken, string RefreshToken, string Role);
}
