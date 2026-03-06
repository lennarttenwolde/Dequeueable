using Azure.Storage.Queues;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Factories;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal sealed class QueueClientProvider(
        IQueueClientFactory factory,
        IHostOptions options,
        ILogger<QueueClientProvider> logger) : IQueueClientProvider
    {
        public QueueClient GetQueue()
        {
            return Get(options.QueueName);
        }

        public QueueClient GetPoisonQueue()
        {
            return Get(options.PoisonQueueName);
        }

        private QueueClient Get(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
            }

            if (options.AuthenticationScheme is not null)
            {
                logger.LogDebug("Authenticate the QueueClient through Active Directory");

                var uri = BuildUri(options.QueueUriFormat, options.AccountName, queueName);
                return factory.Create(uri, options.AuthenticationScheme, options.QueueClientOptions);
            }

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
            }

            logger.LogDebug("Authenticate the QueueClient through the ConnectionString");
            return factory.Create(options.ConnectionString, queueName, options.QueueClientOptions);
        }

        private Uri BuildUri(string? uriFormat, string? accountName, string queueName)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
            {
                throw new ArgumentException($"'{nameof(uriFormat)}' cannot be null or whitespace.", nameof(uriFormat));
            }

            if (string.IsNullOrWhiteSpace(accountName) == false)
            {
                uriFormat = uriFormat.Replace($"{{{nameof(IHostOptions.AccountName)}}}", accountName, StringComparison.InvariantCultureIgnoreCase);
            }

            uriFormat = uriFormat.Replace($"{{queueName}}", queueName, StringComparison.InvariantCultureIgnoreCase);

            try
            {
                return new Uri(uriFormat);
            }
            catch (UriFormatException)
            {
                logger.LogError("Invalid Uri: The Queue Uri could not be parsed. Format: '{Uri}'", uriFormat);
                throw;
            }
        }
    }
}
