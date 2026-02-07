namespace ContentHub.Api.Contracts.Requests
{
    public record BlobUploadRequest(
        string FileName,
        string ContentType,
        long ContentLength);
}
