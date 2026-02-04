using System;

namespace ContentHub.Api.Contracts.Responses
{
    public record AdminUserResponse(
        Guid Id,
        string Email,
        string DisplayName,
        string Role,
        bool EmailConfirmed,
        bool IsDisabled,
        DateTime CreatedAtUtc,
        DateTime? LastLoginAtUtc);
}
