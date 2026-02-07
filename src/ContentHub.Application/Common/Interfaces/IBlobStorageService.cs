using ContentHub.Application.Common.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ContentHub.Application.Common.Interfaces
{
    public interface IBlobStorageService
    {
        Task<BlobUploadResult> CreateUploadAsync(
            string blobName,
            string contentType,
            TimeSpan expiresIn,
            CancellationToken cancellationToken);

        Task<BlobObjectProperties?> GetPropertiesAsync(
            string blobName,
            CancellationToken cancellationToken);

        string GetBlobUrl(string blobName);
    }
}
