namespace ContentHub.Api.Contracts.Responses
{
    public record BlobUploadResponse(
        string UploadUrl,
        string BlobUrl,
        string BlobName);
}
