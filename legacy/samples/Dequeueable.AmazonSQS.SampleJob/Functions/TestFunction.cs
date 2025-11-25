using Dequeueable.AmazonSQS.Models;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AmazonSQS.SampleJob.Functions
{
    internal sealed class TestFunction(ILogger<TestFunction> logger) : IAmazonSQSFunction
    {
        public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Function called with MessageId {MessageId} and content {MessageBody}", message.MessageId, message.Body.ToString());
            return Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
        }
    }
}
