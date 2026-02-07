namespace ContentHub.Api.Contracts.Requests
{
    public record CompleteContentImageUploadRequest(
        string BlobName,
        string RowVersion);
}
