using Dequeueable.AmazonSQS.Services.Hosts;
using Dequeueable.AmazonSQS.Services.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Dequeueable.AmazonSQS.Configurations
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

            services.TryAddSingleton<IHostOptions>(provider =>
            {
                var opt = provider.GetRequiredService<IOptions<HostOptions>>();
                return opt.Value;
            });

            return this;
        }

        public IDequeueableHostBuilder RunAsListener(Action<ListenerHostOptions>? options = null)
        {
            services.AddOptions<ListenerHostOptions>().BindConfiguration(HostOptions.Dequeueable)
                .Validate(ListenerHostOptions.ValidatePollingInterval, $"The '{nameof(ListenerHostOptions.MinimumPollingIntervalInMilliseconds)}' must not be greater than the '{nameof(ListenerHostOptions.MaximumPollingIntervalInMilliseconds)}'.")
                .Validate(ListenerHostOptions.ValidateNewBatchThreshold, $"The '{nameof(ListenerHostOptions.NewBatchThreshold)}' must not be greater than the '{nameof(ListenerHostOptions.BatchSize)}'.")
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (options is not null)
            {
                services.Configure(options);
            }

            services.AddHostedService<QueueListenerHost>();
            services.AddSingleton<IHostExecutor, QueueListenerExecutor>();

            services.TryAddSingleton<IHostOptions>(provider =>
            {
                var opt = provider.GetRequiredService<IOptions<ListenerHostOptions>>();
                return opt.Value;
            });

            return this;
        }

        public IDequeueableHostBuilder AsSingleton()
        {
            services.AddTransient<SingletonManager>();
            services.AddTransient<QueueMessageExecutor>();
            services.AddTransient<IQueueMessageExecutor>(provider =>
            {
                var singletonManager = provider.GetRequiredService<SingletonManager>();
                var executor = provider.GetRequiredService<QueueMessageExecutor>();

                return new SingletonQueueMessageExecutor(executor, singletonManager);
            });

            services.PostConfigure<HostOptions>(options => options.AttributeNames = [.. options.AttributeNames, .. new List<string> { "MessageGroupId" }]);
            services.PostConfigure<ListenerHostOptions>(options => options.AttributeNames = [.. options.AttributeNames, .. new List<string> { "MessageGroupId" }]);
            services.PostConfigure<ListenerHostOptions>(options => options.NewBatchThreshold = 0);

            return this;
        }
    }
}
