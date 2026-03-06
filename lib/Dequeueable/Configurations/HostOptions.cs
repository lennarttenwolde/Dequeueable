using Azure.Core;
using Azure.Storage.Queues;
using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AzureQueueStorage.Configurations
{
    /// <summary>
    /// HostOptions to configure the settings of the host
    /// </summary>
    public class HostOptions : IHostOptions
    {
        internal static string Dequeueable => nameof(Dequeueable);

        /// <summary>
        /// The connection string used to authenticate to the queue. 
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// The storage account name, used for identity flow.
        /// </summary>
        public string? AccountName { get; set; }

        /// <summary>
        /// The queue used to retrieve the messages.
        /// </summary>
        [Required(AllowEmptyStrings = true, ErrorMessage = "Value for {0} cannot be null.")]
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// The poisen queue used to post queue message that reach the <see cref="MaxDequeueCount">MaxDequeueCount</see>.
        /// </summary>
        public string PoisonQueueName => $"{QueueName}-{PoisonQueueSuffix}";

        /// <summary>
        /// Suffix that will be used after the <see cref="QueueName">QueueName</see>, eg queuename-suffix.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} cannot be null or whitespace.")]
        public string PoisonQueueSuffix { get; set; } = "poison";

        /// <summary>
        /// The uri format to the queue storage. Used for identity flow. Use ` {accountName}` and `{queueName}` for variable substitution.
        /// </summary>
        public string QueueUriFormat { get; set; } = "https://{accountName}.queue.core.windows.net/{queueName}";

        /// <summary>
        /// Token credential used to authenticate via AD, Any token credential provider can be used that inherits the abstract class <see cref="TokenCredential">TokenCredential</see>.
        /// </summary>
        public TokenCredential? AuthenticationScheme { get; set; }

        /// <summary>
        /// Provides the client configuration options for connecting to Azure Queue Storage, see <see cref="QueueClientOptions">QueueClientOptions</see>.
        /// </summary>
        public QueueClientOptions? QueueClientOptions { get; set; } = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };

        /// <summary>
        /// The maximum number of messages processed in parallel.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int BatchSize { get; set; } = 16;

        /// <summary>
        /// Max dequeue count before moving to the poison queue. 
        /// </summary>
        [Range(0, 20, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public long MaxDequeueCount { get; set; } = 5;

        /// <summary>
        /// The timeout after the queue message is visible again for other services.
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must not be negative or zero.")]
        public long VisibilityTimeoutInSeconds { get; set; } = 300;
    }
}
