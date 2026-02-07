namespace ContentHub.Api.Contracts.Responses
{
    public record ContentDetailResponse(
        Guid Id,
        Guid AuthorId,
        string AuthorDisplayName,
        string? AuthorProfileImageUrl,
        string Title,
        string Body,
        string Status,
        string? ImageUrl,
        DateTime CreatedAtUtc,
        DateTime? PublishedAtUtc,
        string RowVersion);
}
