using System.ComponentModel.DataAnnotations;

namespace Dequeueable.AmazonSQS.Configurations
{
    /// <summary>
    /// HostOptions to configure the settings of the host
    /// </summary>
    public class ListenerHostOptions : HostOptions
    {
        /// <summary>
        /// The threshold at which a new batch of messages will be fetched.
        /// </summary>
        public int? NewBatchThreshold { get; set; }

        /// <summary>
        /// The minimum polling interval to check the queue for new messages.
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be lower than {1}.")]
        public long MinimumPollingIntervalInMilliseconds { get; set; } = 5;

        /// <summary>
        /// The maximum polling interval to check the queue for new messages. 
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Value for {0} must be lower than {1}.")]
        public long MaximumPollingIntervalInMilliseconds { get; set; } = 10000;

        /// <summary>
        /// The delta used to randomize the polling interval.
        /// </summary>
        public TimeSpan? DeltaBackOff { get; set; }

        internal static bool ValidatePollingInterval(ListenerHostOptions options)
        {
            return options.MinimumPollingIntervalInMilliseconds < options.MaximumPollingIntervalInMilliseconds;
        }
        internal static bool ValidateNewBatchThreshold(ListenerHostOptions options)
        {
            return options.NewBatchThreshold is null || options.NewBatchThreshold <= options.BatchSize;
        }
    }
}
