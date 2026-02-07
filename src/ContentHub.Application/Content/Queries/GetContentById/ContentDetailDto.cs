using ContentHub.Domain.Content;

namespace ContentHub.Application.Content.Queries.GetContentById
{
    public record ContentDetailDto(
        Guid Id,
        Guid AuthorId,
        string AuthorDisplayName,
        string? AuthorProfileImageUrl,
        string Title,
        string Body,
        ContentStatus Status,
        string? ImageUrl,
        DateTime CreatedAtUtc,
        DateTime? PublishedAtUtc,
        byte[] RowVersion);
}
