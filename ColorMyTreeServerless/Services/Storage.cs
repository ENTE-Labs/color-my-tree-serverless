using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace ColorMyTree.Services
{
    public class Storage
    {
        private readonly string _connectionString;

        public Storage(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("AzureStorage");
        }

        public async Task<string> UploadAsync(Stream content, string containerName, string blobName)
        {
            var client = new BlobContainerClient(_connectionString, containerName);

            var blob = client.GetBlobClient(blobName);
            var result = await blob.UploadAsync(content);

            return blob.Uri.ToString();
        }
    }
}
