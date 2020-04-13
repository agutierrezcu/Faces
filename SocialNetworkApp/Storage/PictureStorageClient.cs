using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SocialShared.Logging;

namespace SocialNetworkApp.Storage
{
    public class PictureStorageClient : IPictureStorageClient
    {
        private readonly BlobServiceClient _blobServiceClient;

        private readonly PicturesStorageOptions _picturesStorageOptions;

        private readonly ILogger<PictureStorageClient> _logger;

        public PictureStorageClient(BlobServiceClient blobServiceClient,
            IOptions<PicturesStorageOptions> picturesStorageOptions, ILogger<PictureStorageClient> logger)
        {
            _blobServiceClient = blobServiceClient;
            _picturesStorageOptions = picturesStorageOptions.Value;
            _logger = logger;
        }

        public async Task SaveAsync(Stream picture)
        {
            using var scopedLogger = new ScopedLogger(_logger, "Uploading picture to blob storage.");

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_picturesStorageOptions.ContainerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.UploadBlobAsync(Guid.NewGuid().ToString(), picture);
        }
    }
}