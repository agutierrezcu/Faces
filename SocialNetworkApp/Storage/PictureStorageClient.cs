using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace SocialNetworkApp.Storage
{
    public class PictureStorageClient : IPictureStorageClient
    {
        private readonly BlobServiceClient _blobServiceClient;

        private readonly PicturesStorageOptions _picturesStorageOptions;

        public PictureStorageClient(BlobServiceClient blobServiceClient, IOptions<PicturesStorageOptions> picturesStorageOptions)
        {
            _blobServiceClient = blobServiceClient;
            _picturesStorageOptions = picturesStorageOptions.Value;
        }

        public async Task SaveAsync(Stream picture)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_picturesStorageOptions.ContainerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            await blobContainerClient.UploadBlobAsync(Guid.NewGuid().ToString(), picture);
        }
    }
}