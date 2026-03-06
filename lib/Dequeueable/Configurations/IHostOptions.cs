using Azure.Core;
using Azure.Storage.Queues;

namespace Dequeueable.AzureQueueStorage.Configurations
{
    /// <summary>
    /// Use the IHostOptions to configure the settings of the host
    /// </summary>
    public interface IHostOptions
    {
        /// <summary>
        /// The connection string used to authenticate to the queue. 
        /// </summary>
        string? ConnectionString { get; set; }
        /// <summary>
        /// The storage account name, used for identity flow.
        /// </summary>
        string? AccountName { get; set; }
        /// <summary>
        /// The queue used to retrieve the messages.
        /// </summary>
        string QueueName { get; set; }

        /// <summary>
        /// The poisen queue used to post queue message that reach the <see cref="MaxDequeueCount">MaxDequeueCount</see>.
        /// </summary>
        string PoisonQueueName { get; }

        /// <summary>
        /// Suffix that will be used after the <see cref="QueueName">QueueName</see>, eg queuename-suffix.
        /// </summary>
        string PoisonQueueSuffix { get; set; }

        /// <summary>
        /// The uri format to the queue storage. Used for identity flow. Use ` {accountName}` and `{queueName}` for variable substitution.
        /// </summary>
        string QueueUriFormat { get; set; }

        /// <summary>
        /// Token credential used to authenticate via AD, Any token credential provider can be used that inherits the abstract class <see cref="TokenCredential">TokenCredential</see>.
        /// </summary>
        TokenCredential? AuthenticationScheme { get; set; }

        /// <summary>
        /// Provides the client configuration options for connecting to Azure Queue Storage, see <see cref="QueueClientOptions">QueueClientOptions</see>.
        /// </summary>
        QueueClientOptions? QueueClientOptions { get; set; }

        /// <summary>
        /// The maximum number of messages processed in parallel.
        /// </summary>
        int BatchSize { get; set; }

        /// <summary>
        /// Max dequeue count before moving to the poison queue. 
        /// </summary>
        long MaxDequeueCount { get; set; }

        /// <summary>
        /// The timeout after the queue message is visible again for other services.
        /// </summary>
        long VisibilityTimeoutInSeconds { get; set; }
    }
}