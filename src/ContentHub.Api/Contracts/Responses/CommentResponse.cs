namespace ContentHub.Api.Contracts.Responses
{
    public record CommentResponse(
        Guid Id,
        Guid ContentItemId,
        Guid UserId,
        string UserDisplayName,
        string? UserProfileImageUrl,
        string Text,
        DateTime CreatedAtUtc);
}
