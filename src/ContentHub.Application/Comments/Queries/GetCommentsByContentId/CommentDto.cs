namespace ContentHub.Application.Comments.Queries.GetCommentsByContentId
{
    public record CommentDto(
        Guid Id,
        Guid ContentItemId,
        Guid UserId,
        string UserDisplayName,
        string? UserProfileImageUrl,
        string Text,
        DateTime CreatedAtUtc);
}
