namespace Dequeueable.AmazonSQS.Services.Queues
{
    internal interface IQueueMessageHandler
    {
        Task HandleAsync(Models.Message message, CancellationToken cancellationToken);
    }
}
