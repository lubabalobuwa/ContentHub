using System;

namespace ContentHub.Application.Users.Queries.GetUserProfile
{
    public record UserProfileResult(
        Guid Id,
        string Email,
        string DisplayName,
        string? ProfileImageUrl,
        string Role,
        DateTime CreatedAtUtc,
        DateTime? LastLoginAtUtc);
}
