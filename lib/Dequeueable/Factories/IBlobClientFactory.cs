using Azure.Core;
using Azure.Storage.Blobs;

namespace Dequeueable.AzureQueueStorage.Factories
{
    /// <summary>
    /// Factory used to create the <see cref="BlobClient"/>. This interface can be used when mocking the blob client.
    /// </summary>
    public interface IBlobClientFactory
    {
        /// <summary>
        /// Creates the <see cref="BlobClient"/> with a SAS token
        /// </summary>
        /// <param name="connectionString"> 
        /// A connection string includes the authentication information
        /// required for your application to access data in an Azure Storage
        /// account at runtime.
        ///
        /// For more information,
        /// <see href="https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string">
        /// Configure Azure Storage connection strings</see>.
        /// </param>
        /// <param name="containerName">The name of the blob container.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <returns>A <see cref="BlobClient"/> </returns>
        BlobClient Create(string connectionString, string containerName, string blobName);

        /// <summary>
        /// Creates the <see cref="BlobClient"/> with Identity
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> referencing the blob that includes the
        /// name of the account, the name of the container, and the name of
        /// the blob.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}/{blob_name}".</param>
        /// <param name="tokenCredential">The token credential used to sign requests.</param>
        /// <returns>A <see cref="BlobClient"/> </returns>
        BlobClient Create(Uri uri, TokenCredential tokenCredential);
    }
}