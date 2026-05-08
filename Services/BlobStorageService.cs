using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = string.Empty;

        public BlobStorageService(IConfiguration configuration)
        {
            _blobServiceClient = new BlobServiceClient(
                configuration["AzureStorage:ConnectionString"]);
            _containerName = configuration["AzureStorage:ContainerName"] ?? "images";
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient
                .GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var fileName = Guid.NewGuid().ToString() +
                Path.GetExtension(file.FileName);

            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });
            }

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var uri = new Uri(imageUrl);
            var fileName = Path.GetFileName(uri.LocalPath);

            var containerClient = _blobServiceClient
                .GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}