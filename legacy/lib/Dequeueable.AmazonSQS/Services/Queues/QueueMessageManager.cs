using Amazon.SQS;
using Amazon.SQS.Model;
using Dequeueable.AmazonSQS.Configurations;
using Dequeueable.AmazonSQS.Factories;

namespace Dequeueable.AmazonSQS.Services.Queues
{
    internal sealed class QueueMessageManager(IAmazonSQSClientFactory amazonSQSClientFactory,
        IHostOptions hostOptions) : IQueueMessageManager
    {
        private readonly AmazonSQSClient _client = amazonSQSClientFactory.Create();

        public async Task<Models.Message[]> RetrieveMessagesAsync(CancellationToken cancellationToken = default)
        {
            var request = new ReceiveMessageRequest { QueueUrl = hostOptions.QueueUrl, MaxNumberOfMessages = hostOptions.BatchSize, VisibilityTimeout = hostOptions.VisibilityTimeoutInSeconds, MessageSystemAttributeNames = hostOptions.AttributeNames.ToList() };
            var res = await _client.ReceiveMessageAsync(request, cancellationToken);

            var nextVisbileOn = NextVisbileOn();
            return res.Messages.Select(m => new Models.Message(m.MessageId, m.ReceiptHandle, nextVisbileOn, BinaryData.FromString(m.Body ?? string.Empty), m.Attributes)).ToArray();
        }

        public async Task DeleteMessageAsync(Models.Message message, CancellationToken cancellationToken)
        {
            try
            {
                await _client.DeleteMessageAsync(hostOptions.QueueUrl, message.ReceiptHandle, cancellationToken);
            }
            catch (AmazonSQSException ex) when (ex.ErrorCode?.Contains("NonExistentQueue", StringComparison.InvariantCultureIgnoreCase) ?? false || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
            }
        }

        public async Task<DateTimeOffset> UpdateVisibilityTimeOutAsync(Models.Message message, CancellationToken cancellationToken)
        {
            var request = new ChangeMessageVisibilityRequest(hostOptions.QueueUrl, message.ReceiptHandle, hostOptions.VisibilityTimeoutInSeconds);
            await _client.ChangeMessageVisibilityAsync(request, cancellationToken);

            return NextVisbileOn();
        }

        public async Task EnqueueMessageAsync(Models.Message message, CancellationToken cancellationToken)
        {
            var request = new ChangeMessageVisibilityRequest(hostOptions.QueueUrl, message.ReceiptHandle, 0);
            await _client.ChangeMessageVisibilityAsync(request, cancellationToken);
        }

        private DateTimeOffset NextVisbileOn()
        {
            return DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(hostOptions.VisibilityTimeoutInSeconds));
        }
    }
}
