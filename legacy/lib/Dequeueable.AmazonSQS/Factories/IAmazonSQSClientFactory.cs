using Amazon.SQS;

namespace Dequeueable.AmazonSQS.Factories
{
    /// <summary>
    /// Factory used to create the <see cref="AmazonSQSClient"/>. This interface can be used when mocking the queue client or when you want to override the default client setup.
    /// </summary>
    public interface IAmazonSQSClientFactory
    {
        /// <summary>
        /// Creates the <see cref="AmazonSQSClient"/> 
        /// </summary>
        AmazonSQSClient Create();
    }
}
