namespace ContentHub.Application.Common.Options
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string ContainerName { get; set; } = "contenthub";
        public string? PublicBaseUrl { get; set; }
        public int SasExpiryMinutes { get; set; } = 10;
    }
}
