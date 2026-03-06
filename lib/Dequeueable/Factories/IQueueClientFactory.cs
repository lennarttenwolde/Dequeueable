using Azure.Core;
using Azure.Storage.Queues;

namespace Dequeueable.AzureQueueStorage.Factories
{
    /// <summary>
    /// Factory used to create the <see cref="QueueClient"/>. This interface can be used when mocking the queue client.
    /// </summary>
    public interface IQueueClientFactory
    {
        /// <summary>
        /// Creates the <see cref="QueueClient"/> with a SAS token
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
        /// <param name="queueName">The name of the queue</param>
        /// <param name="queueClientOptions">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        /// <returns>A <see cref="QueueClient"/></returns>
        QueueClient Create(string connectionString, string queueName, QueueClientOptions? queueClientOptions = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">
        /// A <see cref="Uri"/> referencing the queue that includes the
        /// name of the account, the name of the queue, and a SAS token.
        /// This is likely to be similar to "https://{account_name}.queue.core.windows.net/{queue_name}?{sas_token}".
        /// </param>
        /// <param name="tokenCredential">The token credential used to sign requests.</param>
        /// <param name="queueClientOptions">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        /// <returns>A <see cref="QueueClient"/></returns>
        QueueClient Create(Uri uri, TokenCredential tokenCredential, QueueClientOptions? queueClientOptions = null);
    }
}