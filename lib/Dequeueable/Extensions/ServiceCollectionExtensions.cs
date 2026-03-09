using Dequeueable.Factories;
using Dequeueable.Services.Queues;
using Dequeueable.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Dequeueable.Services.Hosts;

namespace Dequeueable.Extensions
{
    /// <summary>
    /// Extension methods for adding configuration related of the Queue services to the DI container via <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Azure Queue services and the job of the type specified in <typeparamref name="TJob"/> to the
        /// specified <see cref="IServiceCollection"/>. 
        /// </summary>
        /// <typeparam name="TJob">The type implementing the <see cref="IQueueJob"/></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
        /// <param name="options">Action to configure the <see cref="HostOptions"/></param>
        /// <returns> <see cref="IDequeueableHostBuilder"/> </returns>
        public static IDequeueableHostBuilder AddDequeueable<TJob>(this IServiceCollection services, Action<HostOptions>? options = null)
            where TJob : class, IQueueJob
        {
            services.AddOptions<HostOptions>().BindConfiguration(HostOptions.Dequeueable)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (options is not null)
            {
                services.Configure(options);
            }

            services.AddSingleton<IJobExecutor, JobExecutor>();
            services.AddSingleton<IQueueMessageManager, QueueMessageManager>();
            services.AddTransient<IQueueMessageHandler, QueueMessageHandler>();
            services.AddTransient<IQueueMessageExecutor, QueueMessageExecutor>();
            services.AddTransient<IQueueClientFactory, QueueClientFactory>();
            services.AddTransient<IQueueJob, TJob>();
            services.TryAddTransient<IQueueClientProvider, QueueClientProvider>();
            services.TryAddSingleton(TimeProvider.System);

            return new DequeueableHostBuilder(services);
        }
    }
}
