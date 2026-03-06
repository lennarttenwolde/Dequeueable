using Azure.Storage.Blobs;

namespace Dequeueable.AzureQueueStorage.Services.Singleton
{
    /// <summary>
    /// Provides the <see cref="BlobClient"/> used for leasing the blob. Can be overriden if custom implementation is needed.
    /// </summary>
    public interface IBlobClientProvider
    {
        /// <summary>
        /// Gets the <see cref="BlobClient"/> used for leasing the blob.
        /// </summary>
        /// <param name="blobName">The blob name used for the lease.</param>
        /// <returns></returns>
        BlobClient GetClient(string blobName);
    }
}