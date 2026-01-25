using System;

namespace ContentHub.Api.Contracts.Responses
{
    public record UserProfileResponse(Guid Id, string Email, string DisplayName, string Role);
}
