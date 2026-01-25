using ContentHub.Domain.Content;

namespace ContentHub.Application.Content.Queries
{
    public record ContentSummaryDto(
        Guid Id,
        string Title,
        string Body,
        ContentStatus Status,
        byte[] RowVersion);
}
