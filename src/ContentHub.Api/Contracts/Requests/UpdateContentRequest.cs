namespace ContentHub.Api.Contracts.Requests
{
    public record UpdateContentRequest(string Title, string Body, string RowVersion);
}
