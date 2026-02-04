using System;

namespace ContentHub.Application.Users.Queries.GetUsers
{
    public record UserSummaryDto(
        Guid Id,
        string Email,
        string DisplayName,
        string Role,
        bool EmailConfirmed,
        bool IsDisabled,
        DateTime CreatedAtUtc,
        DateTime? LastLoginAtUtc);
}
