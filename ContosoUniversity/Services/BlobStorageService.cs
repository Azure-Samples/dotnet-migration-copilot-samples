using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ContosoUniversity.Services
{
    public class BlobStorageService : IDisposable
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService()
        {
            // Get configuration from Web.config
            string endpoint = ConfigurationManager.AppSettings["AzureStorageBlob:Endpoint"];
            _containerName = ConfigurationManager.AppSettings["AzureStorageBlob:ContainerName"];

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new InvalidOperationException("AzureStorageBlob:Endpoint configuration is missing in Web.config");
            }

            if (string.IsNullOrEmpty(_containerName))
            {
                throw new InvalidOperationException("AzureStorageBlob:ContainerName configuration is missing in Web.config");
            }

            // Create BlobServiceClient using DefaultAzureCredential for Managed Identity support
            _blobServiceClient = new BlobServiceClient(
                new Uri(endpoint),
                new DefaultAzureCredential());

            // Ensure container exists
            EnsureContainerExists();
        }

        private void EnsureContainerExists()
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Create container if it doesn't exist with public read access for blobs
                containerClient.CreateIfNotExists(PublicAccessType.Blob);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring container exists: {ex.Message}");
                // Don't throw - container might already exist or will be created on first upload
            }
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage
        /// </summary>
        /// <param name="stream">File stream to upload</param>
        /// <param name="blobName">Name for the blob (e.g., "course_1045_guid.jpg")</param>
        /// <param name="contentType">Content type of the file (e.g., "image/jpeg")</param>
        /// <returns>URI of the uploaded blob</returns>
        public async Task<string> UploadBlobAsync(Stream stream, string blobName, string contentType)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Set blob upload options with content type
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                // Upload the blob
                await blobClient.UploadAsync(stream, uploadOptions);

                // Return the blob URI
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading blob {blobName}: {ex.Message}");
                throw new ApplicationException($"Failed to upload file to blob storage: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a blob from Azure Blob Storage
        /// </summary>
        /// <param name="blobUri">Full URI of the blob to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteBlobAsync(string blobUri)
        {
            if (string.IsNullOrEmpty(blobUri))
            {
                return false;
            }

            try
            {
                // Extract blob name from URI
                var uri = new Uri(blobUri);
                var blobName = uri.Segments[uri.Segments.Length - 1];

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                // Delete the blob if it exists
                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting blob {blobUri}: {ex.Message}");
                // Return false instead of throwing to avoid breaking course deletion
                return false;
            }
        }

        /// <summary>
        /// Gets the URI for a blob
        /// </summary>
        /// <param name="blobName">Name of the blob</param>
        /// <returns>URI of the blob</returns>
        public string GetBlobUri(string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Checks if a blob exists
        /// </summary>
        /// <param name="blobUri">Full URI of the blob</param>
        /// <returns>True if exists, false otherwise</returns>
        public async Task<bool> BlobExistsAsync(string blobUri)
        {
            if (string.IsNullOrEmpty(blobUri))
            {
                return false;
            }

            try
            {
                var uri = new Uri(blobUri);
                var blobName = uri.Segments[uri.Segments.Length - 1];

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            // BlobServiceClient doesn't require explicit disposal
        }
    }
}
