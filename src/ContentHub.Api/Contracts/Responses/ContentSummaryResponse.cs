namespace ContentHub.Api.Contracts.Responses
{
    public record ContentSummaryResponse(
        Guid Id,
        string Title,
        string Body,
        string Status,
        string RowVersion
    );
}
