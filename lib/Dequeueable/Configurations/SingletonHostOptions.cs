using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AzureQueueStorage.Configurations
{
    /// <summary>
    /// SingletonHostOptions to configure the singleton settings of the host
    /// </summary>
    public class SingletonHostOptions
    {
        internal static string Name => $"{HostOptions.Dequeueable}:Singleton";

        /// <summary>
        /// Gets the scope indentifier of the lock. This will be the blob file name to implement the lock.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} cannot be empty.")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// Specifies the duration of the lease, in seconds.
        /// A lease can be between 15 and 60 seconds.
        /// </summary>
        [Range(15, 60, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int LeaseDurationInSeconds { get; set; } = 60;

        /// <summary>
        /// The minimum polling interval to check if a new lease can be acquired.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Value for {0} must not be negative or zero.")]
        public int MinimumPollingIntervalInSeconds { get; set; } = 10;

        /// <summary>
        /// The maximum polling interval to check if a new lease can be acquired.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must not be negative or zero.")]
        public int MaximumPollingIntervalInSeconds { get; set; } = 120;

        /// <summary>
        /// The max retries to acquire a lease. When reached, the host will shutdown.
        /// </summary>
        [Range(0, 10, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// The name of the container used for the lock files. 
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} cannot be empty.")]
        public string ContainerName { get; set; } = "webjobshost";

        /// <summary>
        /// The uri format to the blob storage. Used for identity flow. Use ` {accountName}`, `{containerName}` and `{blobName}` for variable substitution.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} cannot be empty.")]
        public string BlobUriFormat { get; set; } = "https://{accountName}.blob.core.windows.net/{containerName}/{blobName}";

        internal static bool ValidatePollingInterval(SingletonHostOptions options)
        {
            return options.MinimumPollingIntervalInSeconds < options.MaximumPollingIntervalInSeconds;
        }
    }
}
