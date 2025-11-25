namespace Dequeueable.AmazonSQS.Configurations
{
    /// <summary>
    /// Interface to builds and setup the dequeueable host
    /// </summary>
    public interface IDequeueableHostBuilder
    {
        /// <summary>
        /// Runs the function as a Distributed Singleton. Queue messages containing the same MessageGroupId will not run in parallel <see href="https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/using-messagegroupid-property.html"/>
        /// </summary>
        /// <returns><see cref="IDequeueableHostBuilder"/></returns>
        IDequeueableHostBuilder AsSingleton();
        /// <summary>
        /// The application will run as a job, from start to finish, and will automatically shutdown when the messages are executed.
        /// </summary>
        /// <param name="options">Action to configure the <see cref="HostOptions"/></param>
        /// <returns><see cref="IDequeueableHostBuilder"/></returns>
        IDequeueableHostBuilder RunAsJob(Action<HostOptions>? options = null);
        /// <summary>
        /// The application will run as a listener, the queue will periodically be polled for new message.
        /// </summary>
        /// <param name="options">Action to configure the <see cref="ListenerHostOptions"/></param>
        /// <returns><see cref="IDequeueableHostBuilder"/></returns>
        IDequeueableHostBuilder RunAsListener(Action<ListenerHostOptions>? options = null);
    }
}
