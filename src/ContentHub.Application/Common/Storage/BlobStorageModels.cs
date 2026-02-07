namespace ContentHub.Application.Common.Storage
{
    public record BlobUploadResult(
        string UploadUrl,
        string BlobUrl,
        string BlobName);

    public record BlobObjectProperties(
        long ContentLength,
        string? ContentType);
}
