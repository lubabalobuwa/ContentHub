namespace ContentHub.Api.Contracts.Responses
{
    public record ContentSummaryResponse(
        Guid Id,
        Guid AuthorId,
        string Title,
        string Body,
        string Status,
        string RowVersion
    );
}
