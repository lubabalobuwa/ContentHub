namespace ContentHub.Api.Contracts.Requests
{
    public record CreateContentRequest(
        string Title,
        string Body
    );
}
