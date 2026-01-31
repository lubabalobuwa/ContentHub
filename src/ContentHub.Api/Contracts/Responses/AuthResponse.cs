using System;

namespace ContentHub.Api.Contracts.Responses
{
    public record AuthResponse(Guid UserId, string Token, string RefreshToken, string Role);
}
