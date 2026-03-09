using Dequeueable.Factories;
using Dequeueable.Services.Hosts;
using Dequeueable.Services.Queues;
using Dequeueable.Services.Singleton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Dequeueable.Configurations
{
    internal class HostBuilder(IServiceCollection services) : IDequeueableHostBuilder
    {
        public IDequeueableHostBuilder RunAsJob(Action<HostOptions>? options = null)
        {
            services.AddOptions<HostOptions>().BindConfiguration(HostOptions.Dequeueable)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (options is not null)
            {
                services.Configure(options);
            }

            services.AddHostedService<JobHost>();
            services.AddSingleton<IHostExecutor, JobExecutor>();

            services.TryAddSingleton<HostOptions>(provider =>
            {
                var opt = provider.GetRequiredService<IOptions<HostOptions>>();
                return opt.Value;
            });

            return this;
        }

        public IDequeueableHostBuilder AsSingleton(Action<SingletonHostOptions>? options = null)
        {
            services.AddOptions<SingletonHostOptions>().BindConfiguration(SingletonHostOptions.Name)
                .Validate(SingletonHostOptions.ValidatePollingInterval, $"The '{nameof(SingletonHostOptions.MinimumPollingIntervalInSeconds)}' must not be greater than the '{nameof(SingletonHostOptions.MaximumPollingIntervalInSeconds)}'.")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (options is not null)
            {
                services.Configure(options);
            }

            services.AddTransient<IDistributedLockManager, DistributedLockManager>();
            services.AddTransient<IDistributedLockManagerFactory, DistributedLockManagerFactory>();
            services.AddTransient<IBlobClientProvider, BlobClientProvider>();
            services.AddTransient<ISingletonLockManager, SingletonLockManager>();
            services.AddTransient<IBlobClientFactory, BlobClientFactory>();
            services.AddTransient<QueueMessageExecutor>();
            services.AddTransient<IQueueMessageExecutor>(provider =>
            {
                var singletonManager = provider.GetRequiredService<ISingletonLockManager>();
                var executor = provider.GetRequiredService<QueueMessageExecutor>();
                var attribute = provider.GetRequiredService<IOptions<SingletonHostOptions>>();
                var timeProvider = provider.GetRequiredService<TimeProvider>();

                return new SingletonQueueMessageExecutor(singletonManager, executor, timeProvider, attribute);
            });

            return this;
        }
    }
}
