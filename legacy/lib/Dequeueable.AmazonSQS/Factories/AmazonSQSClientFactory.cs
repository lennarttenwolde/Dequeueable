using Amazon.SQS;

namespace Dequeueable.AmazonSQS.Factories
{
    internal sealed class AmazonSQSClientFactory : IAmazonSQSClientFactory
    {
        private AmazonSQSClient? _client;
        public AmazonSQSClient Create() => _client ??= new();
    }
}
