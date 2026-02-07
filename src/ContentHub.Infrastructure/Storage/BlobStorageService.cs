using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using ContentHub.Application.Common.Storage;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ContentHub.Infrastructure.Storage
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly BlobStorageSettings _settings;

        public BlobStorageService(IOptions<BlobStorageSettings> options)
        {
            _settings = options.Value;
            if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
                throw new InvalidOperationException("BlobStorage:ConnectionString is not configured.");

            _containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName);
        }

        public async Task<BlobUploadResult> CreateUploadAsync(
            string blobName,
            string contentType,
            TimeSpan expiresIn,
            CancellationToken cancellationToken)
        {
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.Blob,
                cancellationToken: cancellationToken);

            var blobClient = _containerClient.GetBlobClient(blobName);
            if (!blobClient.CanGenerateSasUri)
                throw new InvalidOperationException("Blob client cannot generate SAS URI. Check storage credentials.");

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            var publicUrl = GetBlobUrl(blobName);

            return new BlobUploadResult(sasUri.ToString(), publicUrl, blobName);
        }

        public async Task<BlobObjectProperties?> GetPropertiesAsync(
            string blobName,
            CancellationToken cancellationToken)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.ExistsAsync(cancellationToken);
            if (!response.Value)
                return null;

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            return new BlobObjectProperties(
                properties.Value.ContentLength,
                properties.Value.ContentType);
        }

        public string GetBlobUrl(string blobName)
        {
            if (!string.IsNullOrWhiteSpace(_settings.PublicBaseUrl))
                return $"{_settings.PublicBaseUrl.TrimEnd('/')}/{blobName}";

            return _containerClient.GetBlobClient(blobName).Uri.ToString();
        }
    }
}
