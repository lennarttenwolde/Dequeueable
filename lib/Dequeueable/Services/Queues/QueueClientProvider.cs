using Azure.Storage.Queues;
using Dequeueable.Configurations;
using Dequeueable.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.Services.Queues
{
    internal sealed class QueueClientProvider(
    IQueueClientFactory factory,
    IOptions<HostOptions> options,
    ILogger<QueueClientProvider> logger) : IQueueClientProvider
    {

        private readonly HostOptions _options = options.Value;


        public QueueClient GetQueue()
        {
            return Get(_options.QueueName);
        }

        public QueueClient GetPoisonQueue()
        {
            return Get(_options.PoisonQueueName);
        }

        private QueueClient Get(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
            }

            if (_options.AuthenticationScheme is not null)
            {
                logger.LogDebug("Authenticate the QueueClient through Active Directory");

                var uri = BuildUri(_options.QueueUriFormat, _options.AccountName, queueName);
                return factory.Create(uri, _options.AuthenticationScheme, _options.QueueClientOptions);
            }

            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                throw new InvalidOperationException("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
            }

            logger.LogDebug("Authenticate the QueueClient through the ConnectionString");
            return factory.Create(_options.ConnectionString, queueName, _options.QueueClientOptions);
        }

        private Uri BuildUri(string? uriFormat, string? accountName, string queueName)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
            {
                throw new ArgumentException($"'{nameof(uriFormat)}' cannot be null or whitespace.", nameof(uriFormat));
            }

            if (string.IsNullOrWhiteSpace(accountName) == false)
            {
                uriFormat = uriFormat.Replace($"{{{nameof(HostOptions.AccountName)}}}", accountName, StringComparison.InvariantCultureIgnoreCase);
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
