using Dequeueable.Factories;
using Dequeueable.Services.DistributedLock;
using Dequeueable.Services.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.Configurations
{
    internal class DequeueableHostBuilder(IServiceCollection services) : IDequeueableHostBuilder
    {
        public IDequeueableHostBuilder WithDistributedLock(Action<DistributedLockOptions>? options = null)
        {
            services.AddOptions<DistributedLockOptions>().BindConfiguration(DistributedLockOptions.Name)
                .Validate(DistributedLockOptions.ValidatePollingInterval, $"The '{nameof(DistributedLockOptions.MinimumPollingIntervalInSeconds)}' must not be greater than the '{nameof(DistributedLockOptions.MaximumPollingIntervalInSeconds)}'.")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (options is not null)
            {
                services.Configure(options);
            }

            services.AddTransient<IDistributedLockManager, DistributedLockManager>();

            services.AddTransient<IBlobClientProvider, BlobClientProvider>();
            services.AddTransient<IBlobLeaseManager, BlobLeaseManager>();
            services.AddTransient<IBlobLeaseManagerFactory, BlobLeaseManagerFactory>();
            services.AddTransient<IBlobClientFactory, BlobClientFactory>();
            services.AddTransient<QueueMessageExecutor>();
            services.AddTransient<IQueueMessageExecutor>(provider =>
            {
                var lockManager = provider.GetRequiredService<IDistributedLockManager>();
                var executor = provider.GetRequiredService<QueueMessageExecutor>();
                var attribute = provider.GetRequiredService<IOptions<DistributedLockOptions>>();
                var timeProvider = provider.GetRequiredService<TimeProvider>();

                return new DistributedLockQueueMessageExecutor(lockManager, executor, timeProvider, attribute);
            });

            return this;
        }
    }
}
